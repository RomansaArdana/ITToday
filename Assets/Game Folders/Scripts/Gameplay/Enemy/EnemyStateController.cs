using UnityEngine;

public class EnemyStateController : MonoBehaviour
{
    [SerializeField] private EnemyState currentState = EnemyState.Undetected;
    [SerializeField] private EnemyDetection detection;

    public EnemyState CurrentState => currentState;

    public bool IsUndetected =>
        currentState == EnemyState.Undetected;

    public bool IsSuspicious =>
        currentState == EnemyState.Suspicious;

    public bool IsDetected =>
        currentState == EnemyState.Detected;

    private void Awake()
    {
        if (detection == null)
        {
            detection = GetComponent<EnemyDetection>();
        }
    }

    private void Update()
    {
        if (detection == null)
        {
            return;
        }

        SyncWithDetection();
    }

    private void SyncWithDetection()
    {
        EnemyState newState = ConvertDetectionState(
            detection.CurrentState
        );

        SetState(newState);
    }

    private EnemyState ConvertDetectionState(
        EnemyDetectionState detectionState
    )
    {
        switch (detectionState)
        {
            case EnemyDetectionState.Suspicious:
                return EnemyState.Suspicious;

            case EnemyDetectionState.Detected:
                return EnemyState.Detected;

            case EnemyDetectionState.Undetected:
            default:
                return EnemyState.Undetected;
        }
    }

    public void SetState(EnemyState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        EnemyState previousState = currentState;

        currentState = newState;
    }
}