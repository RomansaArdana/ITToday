using System;
using UnityEngine;

public class EnemyBehaviourController : MonoBehaviour
{
    [Serializable]
    public class RedBehaviourReferences
    {
        [Header("Flee")]
        public EnemyFlee flee;

        [Header("Movement")]
        public EnemyCornerDetector cornerDetector;
        public EnemyObstacleDetector obstacleDetector;
        public EnemyJump jump;
        public EnemyLowGapDetector lowGapDetector;
        public EnemyCrouch crouch;

        [Header("Combat")]
        public EnemyVulnerable vulnerable;
    }

    [Serializable]
    public class BlueBehaviourReferences
    {
        [Header("Movement")]
        public EnemyPressure pressure;
    }

    [Serializable]
    public class CyanBehaviourReferences
    {
        [Header("Movement")]
        public EnemyChase chase;
        public EnemyDash dash;
        public EnemyTeleport teleport;
    }

    [Header("Enemy Type")]
    [SerializeField]
    private EnemyArchetype archetype =
        EnemyArchetype.Red;

    [Header("Core References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyStateController stateController;
    [SerializeField] private EnemyDetection detection;

    [Header("Common Behaviours")]
    [SerializeField] private EnemyPatrol patrol;
    [SerializeField] private EnemySearch search;
    [SerializeField] private EnemyChase chase;

    [Header("Red")]
    [SerializeField]
    private RedBehaviourReferences red =
        new RedBehaviourReferences();

    [Header("Blue")]
    [SerializeField]
    private BlueBehaviourReferences blue =
        new BlueBehaviourReferences();

    [Header("Cyan")]
    [SerializeField]
    private CyanBehaviourReferences cyan =
        new CyanBehaviourReferences();

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private EnemyDetectionState previousDetectionState;

    public EnemyArchetype Archetype =>
        archetype;

    private void Awake()
    {
        enemyController ??=
            GetComponent<EnemyController>();

        stateController ??=
            GetComponent<EnemyStateController>();

        detection ??=
            GetComponent<EnemyDetection>();

        patrol ??=
            GetComponent<EnemyPatrol>();

        search ??=
            GetComponent<EnemySearch>();

        chase ??=
            GetComponent<EnemyChase>();

        AutoAssignReferences();
    }

    private void Start()
    {
        if (detection == null)
        {
            Debug.LogError(
                "[EnemyAI] Detection tidak ditemukan.",
                this
            );

            return;
        }

        ValidateArchetype();

        previousDetectionState =
            detection.CurrentState;

        UpdateBehaviour(
            previousDetectionState
        );
    }

    private void Update()
    {
        if (stateController == null ||
            detection == null)
        {
            return;
        }

        if (stateController.IsDead)
        {
            return;
        }

        if (stateController.IsCornered ||
            stateController.IsVulnerable)
        {
            return;
        }

        switch (archetype)
        {
            case EnemyArchetype.Red:

                if (stateController.IsFlee)
                {
                    CheckRedCornered();

                    if (stateController.IsCornered)
                    {
                        return;
                    }

                    CheckRedObstacle();
                    CheckRedLowGap();
                }

                break;

            case EnemyArchetype.Blue:
                break;

            case EnemyArchetype.Cyan:

                if (stateController.IsChase)
                {
                    CheckCyanDash();
                }

                break;

            case EnemyArchetype.Purple:
                // Purple tidak memiliki
                // movement behavior.
                break;
        }

        EnemyDetectionState currentDetectionState =
            detection.CurrentState;

        if (currentDetectionState ==
            previousDetectionState)
        {
            return;
        }

        previousDetectionState =
            currentDetectionState;

        UpdateBehaviour(
            currentDetectionState
        );
    }

    // =========================================================
    // BEHAVIOUR FLOW
    // =========================================================

    private void UpdateBehaviour(
        EnemyDetectionState detectionState)
    {
        if (stateController.IsDead ||
            stateController.IsCornered ||
            stateController.IsVulnerable)
        {
            return;
        }

        StopAllBehaviours();

        // =====================================================
        // PURPLE SPECIAL CASE
        // =====================================================

        if (archetype == EnemyArchetype.Purple)
        {
            EnablePurpleIdle();
            return;
        }

        // =====================================================
        // COMMON ENEMY FLOW
        // =====================================================

        switch (detectionState)
        {
            case EnemyDetectionState.Undetected:
                EnablePatrol();
                break;

            case EnemyDetectionState.Suspicious:
                EnableSearch();
                break;

            case EnemyDetectionState.Detected:
                HandleDetected();
                break;
        }
    }

    private void HandleDetected()
    {
        switch (archetype)
        {
            case EnemyArchetype.Red:
                EnableRedFlee();
                break;

            case EnemyArchetype.Blue:
                EnableBluePressure();
                break;

            case EnemyArchetype.Cyan:
                EnableCyanChase();
                break;

            case EnemyArchetype.Purple:
                EnablePurpleIdle();
                break;
        }
    }

    // =========================================================
    // PURPLE
    // =========================================================

    private void EnablePurpleIdle()
    {
        stateController.SetState(
            EnemyState.Idle
        );

        LogState("IDLE");
    }

    // =========================================================
    // RED
    // =========================================================

    private void EnableRedFlee()
    {
        stateController.SetState(
            EnemyState.Flee
        );

        LogState("FLEE");

        if (red == null ||
            red.flee == null)
        {
            Debug.LogWarning(
                "[EnemyAI] Red Flee tidak ditemukan.",
                this
            );

            return;
        }

        red.flee.enabled = true;
    }

    private void CheckRedCornered()
    {
        if (red == null ||
            red.cornerDetector == null)
        {
            return;
        }

        if (!red.cornerDetector.IsCornered)
        {
            return;
        }

        EnterRedCornered();
    }

    private void EnterRedCornered()
    {
        StopAllBehaviours();

        stateController.SetState(
            EnemyState.Cornered
        );

        LogState("CORNERED");

        EnableRedVulnerable();
    }

    private void CheckRedObstacle()
    {
        if (red == null ||
            red.obstacleDetector == null ||
            red.jump == null)
        {
            return;
        }

        if (!red.obstacleDetector.IsObstacleDetected)
        {
            return;
        }

        red.jump.TryJump();
    }

    private void CheckRedLowGap()
    {
        if (red == null ||
            red.lowGapDetector == null ||
            red.crouch == null)
        {
            return;
        }

        if (red.lowGapDetector.IsLowGapDetected)
        {
            red.crouch.StartCrouch();
        }
        else
        {
            red.crouch.StopCrouch();
        }
    }

    private void EnableRedVulnerable()
    {
        if (red == null ||
            red.vulnerable == null)
        {
            Debug.LogWarning(
                "[EnemyAI] Red Vulnerable tidak ditemukan.",
                this
            );

            return;
        }

        red.vulnerable.enabled = true;
    }

    // =========================================================
    // BLUE
    // =========================================================

    private void EnableBluePressure()
    {
        stateController.SetState(
            EnemyState.Pressure
        );

        LogState("PRESSURE");

        if (blue == null ||
            blue.pressure == null)
        {
            Debug.LogWarning(
                "[EnemyAI] Blue Pressure tidak ditemukan.",
                this
            );

            return;
        }

        blue.pressure.enabled = true;
    }

    // =========================================================
    // CYAN
    // =========================================================

    private void EnableCyanChase()
    {
        stateController.SetState(
            EnemyState.Chase
        );

        LogState("CHASE");

        if (cyan == null ||
            cyan.chase == null)
        {
            Debug.LogWarning(
                "[EnemyAI] Cyan Chase tidak ditemukan.",
                this
            );

            return;
        }

        cyan.chase.enabled = true;
    }

    private void CheckCyanDash()
    {
        if (cyan == null ||
            cyan.dash == null)
        {
            return;
        }

        cyan.dash.TryDash();
    }

    // =========================================================
    // COMMON
    // =========================================================

    private void EnablePatrol()
    {
        stateController.SetState(
            EnemyState.Patrol
        );

        LogState("PATROL");

        if (patrol == null)
        {
            return;
        }

        patrol.enabled = true;
    }

    private void EnableSearch()
    {
        stateController.SetState(
            EnemyState.Search
        );

        LogState("SEARCH");

        if (search == null)
        {
            return;
        }

        search.enabled = true;
    }

    private void EnableChase()
    {
        stateController.SetState(
            EnemyState.Chase
        );

        LogState("CHASE");

        if (chase == null)
        {
            return;
        }

        chase.enabled = true;
    }

    // =========================================================
    // STOP
    // =========================================================

    public void StopAllBehaviours()
    {
        if (patrol != null)
        {
            patrol.StopPatrol();
            patrol.enabled = false;
        }

        if (search != null)
        {
            search.StopSearch();
            search.enabled = false;
        }

        if (chase != null)
        {
            chase.StopChase();
            chase.enabled = false;
        }

        // RED
        if (red != null)
        {
            if (red.flee != null)
            {
                red.flee.StopFlee();
                red.flee.enabled = false;
            }

            if (red.vulnerable != null)
            {
                red.vulnerable.StopVulnerable();
                red.vulnerable.enabled = false;
            }
        }

        // BLUE
        if (blue != null)
        {
            if (blue.pressure != null)
            {
                blue.pressure.StopPressure();
                blue.pressure.enabled = false;
            }
        }

        // CYAN
        if (cyan != null)
        {
            if (cyan.chase != null)
            {
                cyan.chase.StopChase();
                cyan.chase.enabled = false;
            }

            if (cyan.dash != null)
            {
                cyan.dash.StopDash();
            }

            if (cyan.teleport != null)
            {
                cyan.teleport.ResetCooldown();
            }
        }
    }

    // =========================================================
    // SETUP
    // =========================================================

    private void AutoAssignReferences()
    {
        if (red == null)
        {
            red =
                new RedBehaviourReferences();
        }

        if (blue == null)
        {
            blue =
                new BlueBehaviourReferences();
        }

        if (cyan == null)
        {
            cyan =
                new CyanBehaviourReferences();
        }

        // RED
        red.flee ??=
            GetComponent<EnemyFlee>();

        red.cornerDetector ??=
            GetComponent<EnemyCornerDetector>();

        red.obstacleDetector ??=
            GetComponent<EnemyObstacleDetector>();

        red.jump ??=
            GetComponent<EnemyJump>();

        red.lowGapDetector ??=
            GetComponent<EnemyLowGapDetector>();

        red.crouch ??=
            GetComponent<EnemyCrouch>();

        red.vulnerable ??=
            GetComponent<EnemyVulnerable>();

        // BLUE
        blue.pressure ??=
            GetComponent<EnemyPressure>();

        // CYAN
        cyan.chase ??=
            GetComponent<EnemyChase>();

        cyan.dash ??=
            GetComponent<EnemyDash>();

        cyan.teleport ??=
            GetComponent<EnemyTeleport>();
    }

    // =========================================================
    // VALIDATION
    // =========================================================

    private void ValidateArchetype()
    {
        if (enemyController == null ||
            enemyController.Stats == null)
        {
            return;
        }

        EnemyArchetype statsArchetype =
            enemyController.Stats.Archetype;

        if (statsArchetype != archetype)
        {
            Debug.LogWarning(
                $"[EnemyAI] Archetype mismatch. " +
                $"Controller={archetype}, " +
                $"Stats={statsArchetype}.",
                this
            );
        }
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void LogState(string state)
    {
        if (!enableDebugLog)
        {
            return;
        }

        Debug.Log(
            $"[EnemyAI] " +
            $"{archetype.ToString().ToUpper()} → {state}",
            this
        );
    }
}