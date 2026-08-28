using UnityEngine;

public class EnemyPurpleAnomalyDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyHealth enemyHealth;

    [Header("Anomaly Detection")]
    [SerializeField] private float anomalyRadius = 2.5f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool showGizmo = true;

    public bool IsPlayerInRange { get; private set; }
    public bool IsAnomalyDetected { get; private set; }
    public float DistanceToPlayer { get; private set; }

    private void Awake()
    {
        enemyController ??= GetComponent<EnemyController>();
        enemyHealth ??= GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (!IsValid())
        {
            ResetDetection();
            return;
        }

        UpdateDetection();
    }

    private bool IsValid()
    {
        if (enemyController == null) return false;
        if (enemyController.Stats == null) return false;
        if (enemyController.Stats.Archetype != EnemyArchetype.Purple) return false;
        if (enemyHealth != null && !enemyHealth.IsAlive) return false;
        if (enemyController.PlayerTarget == null) return false;

        return true;
    }

    private void UpdateDetection()
    {
        Transform playerTarget = enemyController.PlayerTarget;

        DistanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        bool newInRange = DistanceToPlayer <= anomalyRadius;
        bool newAnomalyDetected = newInRange;

        if (newInRange != IsPlayerInRange)
        {
            IsPlayerInRange = newInRange;
            LogRangeState();
        }

        if (newAnomalyDetected != IsAnomalyDetected)
        {
            IsAnomalyDetected = newAnomalyDetected;
            LogAnomalyState();
        }
    }

    private void LogRangeState()
    {
        if (!enableDebugLog) return;
        Debug.Log(IsPlayerInRange ? "[Purple] Player entered anomaly range." : "[Purple] Player left anomaly range.", this);
    }

    private void LogAnomalyState()
    {
        if (!enableDebugLog) return;
        Debug.Log(IsAnomalyDetected ? "[Purple] ANOMALY DETECTED" : "[Purple] ANOMALY CLEARED", this);
    }

    private void ResetDetection()
    {
        IsPlayerInRange = false;
        IsAnomalyDetected = false;
        DistanceToPlayer = Mathf.Infinity;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;
        Gizmos.DrawWireSphere(transform.position, anomalyRadius);
    }
}