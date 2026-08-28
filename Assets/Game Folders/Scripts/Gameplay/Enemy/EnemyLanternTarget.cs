using UnityEngine;

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyLanternTarget : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private EnemyStateController stateController;

    [Header("Cyan")]
    [SerializeField] private EnemyDash enemyDash;

    [Header("Purple")]
    [SerializeField] private EnemyPurpleAnomalyDetector purpleAnomalyDetector;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    public bool CanBeHit
    {
        get
        {
            if (enemyHealth == null || !enemyHealth.IsAlive) return false;
            if (enemyController == null || enemyController.Stats == null) return false;

            switch (enemyController.Stats.Archetype)
            {
                case EnemyArchetype.Red:
                    return stateController != null && stateController.IsVulnerable;

                case EnemyArchetype.Purple:
                    return purpleAnomalyDetector != null && purpleAnomalyDetector.IsAnomalyDetected;

                default:
                    return true;
            }
        }
    }

    private void Awake()
    {
        enemyController ??= GetComponent<EnemyController>();
        enemyHealth ??= GetComponent<EnemyHealth>();
        stateController ??= GetComponent<EnemyStateController>();
        enemyDash ??= GetComponent<EnemyDash>();
        purpleAnomalyDetector ??= GetComponent<EnemyPurpleAnomalyDetector>();
    }

    public bool TryReceiveLanternHit(float damage)
    {
        if (!CanBeHit)
        {
            LogRejectedHit();
            return false;
        }

        if (damage <= 0f) return false;

        enemyHealth.TakeDamage(damage);

        if (enableDebugLog)
        {
            string archetype = enemyController != null && enemyController.Stats != null
                ? enemyController.Stats.Archetype.ToString().ToUpper()
                : gameObject.name;

            Debug.Log($"[Lantern] {archetype} HIT", this);
        }

        HandleHitReaction();

        return true;
    }

    private void HandleHitReaction()
    {
        if (enemyController == null || enemyController.Stats == null) return;

        switch (enemyController.Stats.Archetype)
        {
            case EnemyArchetype.Cyan: HandleCyanHitReaction(); break;
            case EnemyArchetype.Purple: HandlePurpleHitReaction(); break;
        }
    }

    private void HandleCyanHitReaction()
    {
        if (enemyHealth == null || !enemyHealth.IsAlive) return;
        if (enemyDash == null) return;

        enemyDash.TryEvade();
    }

    private void HandlePurpleHitReaction()
    {
        if (enemyHealth == null || !enemyHealth.IsAlive) return;
        if (enableDebugLog) Debug.Log("[Purple] PURIFIED", this);
    }

    private void LogRejectedHit()
    {
        if (!enableDebugLog) return;
        if (enemyController == null || enemyController.Stats == null) return;

        EnemyArchetype archetype = enemyController.Stats.Archetype;

        switch (archetype)
        {
            case EnemyArchetype.Red:
                if (stateController != null && !stateController.IsVulnerable)
                    Debug.Log("[Lantern] RED MISS", this);
                break;

            case EnemyArchetype.Purple:
                Debug.Log("[Lantern] PURPLE HIDDEN", this);
                break;
        }
    }
}