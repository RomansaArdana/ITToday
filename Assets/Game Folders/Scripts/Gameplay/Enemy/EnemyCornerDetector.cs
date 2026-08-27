using UnityEngine;

public class EnemyCornerDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyFlee flee;

    [Header("Detection")]
    [SerializeField] private LayerMask cornerObstacleLayer;
    [SerializeField] private Transform sensorOrigin;
    [SerializeField] private float detectionDistance = 0.35f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmo = true;

    public bool IsCornered { get; private set; }

    public RaycastHit2D CurrentHit { get; private set; }

    private void Awake()
    {
        flee ??= GetComponent<EnemyFlee>();

        if (sensorOrigin == null)
        {
            sensorOrigin = transform;
        }
    }

    private void Update()
    {
        DetectCorner();
    }

    private void DetectCorner()
    {
        IsCornered = false;
        CurrentHit = default;

        if (flee == null)
        {
            return;
        }

        if (!flee.enabled)
        {
            return;
        }

        Vector2 direction =
            flee.FleeDirection;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        CurrentHit = Physics2D.Raycast(
            sensorOrigin.position,
            direction,
            detectionDistance,
            cornerObstacleLayer
        );

        IsCornered =
            CurrentHit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmo)
        {
            return;
        }

        Vector2 origin =
            sensorOrigin != null
                ? sensorOrigin.position
                : transform.position;

        Vector2 direction = Vector2.right;

        if (flee != null)
        {
            direction = flee.FleeDirection;
        }

        Gizmos.DrawLine(
            origin,
            origin +
            direction * detectionDistance
        );

        if (IsCornered)
        {
            Gizmos.DrawSphere(
                CurrentHit.point,
                0.08f
            );
        }
    }
}