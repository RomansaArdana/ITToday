using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider2D playerCollider;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private float checkDistance = 0.12f;
    [SerializeField, Range(0.5f, 1f)] private float checkWidth = 0.85f;

    [Header("Debug")]
    [SerializeField] private bool showGizmo = true;

    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
        if (groundLayer.value == 0) groundLayer = ~0;
    }

    private void Update()
    {
        UpdateGrounded();
    }

    private void UpdateGrounded()
    {
        if (playerCollider == null)
        {
            IsGrounded = false;
            return;
        }

        Bounds bounds = playerCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);
        Vector2 size = new Vector2(bounds.size.x * checkWidth, 0.05f);

        LayerMask mask = groundLayer.value == 0 ? ~0 : groundLayer;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0f, Vector2.down, checkDistance, mask);

        bool hitGround = false;
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.isTrigger) continue;
            if (hit.collider.transform.root == transform.root) continue;

            hitGround = true;
            break;
        }

        IsGrounded = hitGround;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo || playerCollider == null) return;

        Bounds bounds = playerCollider.bounds;
        Vector3 center = new Vector3(bounds.center.x, bounds.min.y - checkDistance * 0.5f, 0f);
        Vector3 size = new Vector3(bounds.size.x * checkWidth, 0.05f + checkDistance, 0f);

        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(center, size);
    }
}