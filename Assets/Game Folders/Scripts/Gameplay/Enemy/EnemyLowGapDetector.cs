using UnityEngine;

public class EnemyLowGapDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyFlee flee;
    [SerializeField] private EnemyCrouch crouch;

    [Header("Detection")]
    [SerializeField] private Transform sensorOrigin;
    [SerializeField] private LayerMask lowGapLayer;
    [SerializeField] private float detectionDistance = 0.6f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmo = true;

    public bool IsLowGapDetected { get; private set; }

    private void Awake()
    {
        flee ??= GetComponent<EnemyFlee>();
        crouch ??= GetComponent<EnemyCrouch>();

        if (sensorOrigin == null)
        {
            sensorOrigin = transform;
        }
    }

    private void Update()
    {
        DetectLowGap();
    }

    private void DetectLowGap()
    {
        IsLowGapDetected = false;

        if (flee == null ||
            crouch == null)
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

        RaycastHit2D hit =
            Physics2D.Raycast(
                sensorOrigin.position,
                direction,
                detectionDistance,
                lowGapLayer
            );

        if (hit.collider == null)
        {
            return;
        }

        IsLowGapDetected = true;
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