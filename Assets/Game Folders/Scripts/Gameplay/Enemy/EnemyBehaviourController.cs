using UnityEngine;

public class EnemyBehaviourController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyStateController stateController;
    [SerializeField] private EnemyDetection detection;

    [Header("Behaviours")]
    [SerializeField] private EnemyPatrol patrol;
    [SerializeField] private EnemySearch search;
    [SerializeField] private EnemyChase chase;
    [SerializeField] private EnemyFlee flee;

    [Header("Red Movement")]
    [SerializeField] private EnemyCornerDetector cornerDetector;
    [SerializeField] private EnemyObstacleDetector obstacleDetector;
    [SerializeField] private EnemyJump jump;
    [SerializeField] private EnemyLowGapDetector lowGapDetector;
    [SerializeField] private EnemyCrouch crouch;
    [SerializeField] private EnemyVulnerable vulnerable;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private EnemyDetectionState previousDetectionState;

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

        flee ??=
            GetComponent<EnemyFlee>();

        cornerDetector ??=
            GetComponent<EnemyCornerDetector>();

        obstacleDetector ??=
            GetComponent<EnemyObstacleDetector>();

        jump ??=
            GetComponent<EnemyJump>();

        lowGapDetector ??=
            GetComponent<EnemyLowGapDetector>();

        crouch ??=
            GetComponent<EnemyCrouch>();

        vulnerable ??=
            GetComponent<EnemyVulnerable>();
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

        if (stateController.IsVulnerable)
        {
            return;
        }

        if (stateController.IsCornered)
        {
            return;
        }

        if (stateController.IsFlee)
        {
            CheckCorneredState();

            if (stateController.IsCornered)
            {
                return;
            }

            CheckObstacleState();
            CheckLowGapState();
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

    private void CheckCorneredState()
    {
        if (cornerDetector == null)
        {
            return;
        }

        if (!cornerDetector.IsCornered)
        {
            return;
        }

        EnterCornered();
    }

    private void EnterCornered()
    {
        StopAllBehaviours();

        stateController.SetState(
            EnemyState.Cornered
        );

        LogState("CORNERED");

        EnableVulnerable();
    }

    private void CheckObstacleState()
    {
        if (obstacleDetector == null ||
            jump == null)
        {
            return;
        }

        if (!obstacleDetector.IsObstacleDetected)
        {
            return;
        }

        jump.TryJump();
    }

    private void CheckLowGapState()
    {
        if (lowGapDetector == null ||
            crouch == null)
        {
            return;
        }

        if (lowGapDetector.IsLowGapDetected)
        {
            crouch.StartCrouch();
        }
        else
        {
            crouch.StopCrouch();
        }
    }

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
        if (enemyController == null ||
            enemyController.Stats == null)
        {
            return;
        }

        switch (enemyController.Stats.Archetype)
        {
            case EnemyArchetype.Red:
                EnableFlee();
                break;

            default:
                EnableChase();
                break;
        }
    }

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

    private void EnableFlee()
    {
        stateController.SetState(
            EnemyState.Flee
        );

        LogState("FLEE");

        if (flee == null)
        {
            Debug.LogWarning(
                "[EnemyAI] Flee tidak ditemukan.",
                this
            );

            return;
        }

        flee.enabled = true;
    }

    private void EnableVulnerable()
    {
        if (vulnerable == null)
        {
            Debug.LogWarning(
                "[EnemyAI] Vulnerable tidak ditemukan.",
                this
            );

            return;
        }

        vulnerable.enabled = true;
    }

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

        if (flee != null)
        {
            flee.StopFlee();
            flee.enabled = false;
        }

        if (crouch != null)
        {
            crouch.StopCrouch();
            crouch.enabled = false;
        }

        if (vulnerable != null)
        {
            vulnerable.StopVulnerable();
            vulnerable.enabled = false;
        }
    }

    private void LogState(string state)
    {
        if (!enableDebugLog)
        {
            return;
        }

        string enemyName =
            enemyController != null &&
            enemyController.Stats != null
                ? enemyController.Stats.Archetype
                    .ToString()
                    .ToUpper()
                : gameObject.name;

        Debug.Log(
            $"[EnemyAI] {enemyName} → {state}",
            this
        );
    }
}