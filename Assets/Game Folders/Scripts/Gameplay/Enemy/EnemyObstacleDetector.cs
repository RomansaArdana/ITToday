using UnityEngine;

public class EnemyObstacleDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyFlee flee;

    [Header("Obstacle Detection")]
    [SerializeField] private Transform sensorOrigin;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float detectionDistance = 0.6f;

    [Header("Jump Detection")]
    [SerializeField] private float obstacleHeight = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmo = true;

    public bool IsObstacleDetected { get; private set; }

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
        DetectObstacle();
    }

    private void DetectObstacle()
    {
        IsObstacleDetected = false;
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

        Vector2 origin =
            sensorOrigin.position;

        CurrentHit =
            Physics2D.Raycast(
                origin,
                direction,
                detectionDistance,
                obstacleLayer
            );

        if (CurrentHit.collider == null)
        {
            return;
        }

        float obstacleTop =
            CurrentHit.collider.bounds.max.y;

        float enemyBottom =
            GetComponent<Collider2D>() != null
                ? GetComponent<Collider2D>()
                    .bounds.min.y
                : transform.position.y;

        float height =
            obstacleTop - enemyBottom;

        IsObstacleDetected =
            height <= obstacleHeight;
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

        Vector2 direction =
            flee != null
                ? flee.FleeDirection
                : Vector2.right;

        Gizmos.DrawLine(
            origin,
            origin +
            direction * detectionDistance
        );
    }
}