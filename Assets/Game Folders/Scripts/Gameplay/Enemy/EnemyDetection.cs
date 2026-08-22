using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private PlayerStealth playerStealth;

    private Transform playerTarget;

    public bool IsPlayerInRange { get; private set; }
    public bool IsPlayerInVision { get; private set; }
    public bool HasLineOfSight { get; private set; }

    public float DistanceToPlayer { get; private set; }
    public float StealthMultiplier { get; private set; } = 1f;
    public float DetectionStrength { get; private set; }
    public float DetectionProgress { get; private set; }

    public bool IsDetected { get; private set; }
    public EnemyDetectionState CurrentState { get; private set; }
    public Vector2 LastKnownPlayerPosition { get; private set; }

    private void Awake()
    {
        if (enemyController == null) enemyController = GetComponent<EnemyController>();

        CurrentState = EnemyDetectionState.Undetected;
        LastKnownPlayerPosition = transform.position;
    }

    private void Update()
    {
        if (enemyController == null || enemyController.Stats == null) return;

        playerTarget = enemyController.PlayerTarget;

        if (playerTarget == null)
        {
            ResetDetection();
            return;
        }

        CachePlayerStealth();
        UpdateStealthState();
        UpdateDetection();
        UpdateDetectionProgress();
        UpdateDetectionState();
    }

    private void CachePlayerStealth()
    {
        if (playerStealth == null)
            playerStealth = playerTarget.GetComponent<PlayerStealth>();
    }

    private void UpdateStealthState()
    {
        StealthMultiplier = playerStealth != null
            ? playerStealth.StealthMultiplier
            : 1f;
    }

    private void UpdateDetection()
    {
        DistanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        float detectionRange = enemyController.Stats.DetectionRange;

        IsPlayerInRange = DistanceToPlayer <= detectionRange;
        IsPlayerInVision = false;
        HasLineOfSight = false;
        DetectionStrength = 0f;

        if (!IsPlayerInRange) return;

        IsPlayerInVision = CheckVisionAngle();

        if (!IsPlayerInVision) return;

        HasLineOfSight = CheckLineOfSight();

        if (!HasLineOfSight) return;

        LastKnownPlayerPosition = playerTarget.position;
        DetectionStrength = Mathf.Clamp01(StealthMultiplier);
    }

    private void UpdateDetectionProgress()
    {
        float detectionSpeed = enemyController.Stats.DetectionSpeed;
        float decaySpeed = enemyController.Stats.DetectionDecaySpeed;
        float threshold = enemyController.Stats.DetectionThreshold;

        bool canDetectPlayer =
            IsPlayerInRange &&
            IsPlayerInVision &&
            HasLineOfSight &&
            DetectionStrength > 0f;

        DetectionProgress += canDetectPlayer
            ? detectionSpeed * DetectionStrength * Time.deltaTime
            : -decaySpeed * Time.deltaTime;

        DetectionProgress = Mathf.Clamp(DetectionProgress, 0f, threshold);
        IsDetected = DetectionProgress >= threshold;
    }

    private void UpdateDetectionState()
    {
        float threshold = enemyController.Stats.DetectionThreshold;
        float suspiciousThreshold = threshold * enemyController.Stats.SuspiciousThreshold;

        EnemyDetectionState newState = DetectionProgress >= threshold
            ? EnemyDetectionState.Detected
            : DetectionProgress >= suspiciousThreshold
                ? EnemyDetectionState.Suspicious
                : EnemyDetectionState.Undetected;

        if (CurrentState == newState) return;

        CurrentState = newState;
    }

    private bool CheckVisionAngle()
    {
        Vector2 directionToPlayer =
            (playerTarget.position - transform.position).normalized;

        float angle = Vector2.Angle(
            enemyController.FacingDirection,
            directionToPlayer
        );

        return angle <= enemyController.Stats.DetectionAngle * 0.5f;
    }

    private bool CheckLineOfSight()
    {
        Vector2 direction =
            (playerTarget.position - transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            DistanceToPlayer,
            obstacleLayer
        );

        return hit.collider == null;
    }

    private void ResetDetection()
    {
        IsPlayerInRange = false;
        IsPlayerInVision = false;
        HasLineOfSight = false;
        DistanceToPlayer = Mathf.Infinity;

        StealthMultiplier = 1f;
        DetectionStrength = 0f;
        DetectionProgress = 0f;

        IsDetected = false;
        CurrentState = EnemyDetectionState.Undetected;
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyController == null || enemyController.Stats == null) return;

        Gizmos.DrawWireSphere(
            transform.position,
            enemyController.Stats.DetectionRange
        );

        Gizmos.DrawSphere(
            LastKnownPlayerPosition,
            0.15f
        );

        Gizmos.DrawLine(
            transform.position,
            LastKnownPlayerPosition
        );
    }
}