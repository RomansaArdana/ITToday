using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider2D playerCollider;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float checkDistance = 0.08f;
    [SerializeField, Range(0.5f, 1f)] private float checkWidth = 0.85f;

    [Header("Debug")]
    [SerializeField] private bool showGizmo = true;

    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
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

        IsGrounded = Physics2D.BoxCast(origin, size, 0f, Vector2.down, checkDistance, groundLayer).collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo || playerCollider == null) return;

        Bounds bounds = playerCollider.bounds;
        Vector3 center = new Vector3(bounds.center.x, bounds.min.y - checkDistance * 0.5f, 0f);
        Vector3 size = new Vector3(bounds.size.x * checkWidth, 0.05f + checkDistance, 0f);

        Gizmos.DrawWireCube(center, size);
    }
}