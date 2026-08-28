using UnityEngine;

public class EnemyTeleport : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyStateController stateController;
    [SerializeField] private EnemyHealth enemyHealth;

    [Header("Teleport")]
    [SerializeField] private float triggerDistance = 4f;
    [SerializeField] private float teleportDistance = 4f;
    [SerializeField] private float teleportCooldown = 4f;

    [Header("Safety")]
    [SerializeField] private LayerMask blockingLayer;
    [SerializeField] private float validationRadius = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool showGizmo = true;

    private float cooldownTimer;

    private void Awake()
    {
        enemyController ??= GetComponent<EnemyController>();
        stateController ??= GetComponent<EnemyStateController>();
        enemyHealth ??= GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        UpdateCooldown();
    }

    private void UpdateCooldown()
    {
        if (cooldownTimer <= 0f)
        {
            cooldownTimer = 0f;
            return;
        }

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer < 0f) cooldownTimer = 0f;
    }

    public bool TryTeleport()
    {
        if (!CanTeleport()) return false;

        Vector2 destination = GetTeleportDestination();

        if (!IsPositionSafe(destination))
        {
            if (enableDebugLog) Debug.Log("[EnemyAI] CYAN TELEPORT FAILED → Blocked", this);
            return false;
        }

        transform.position = destination;
        cooldownTimer = Mathf.Max(0f, teleportCooldown);

        if (enableDebugLog) Debug.Log("[EnemyAI] CYAN → TELEPORT", this);

        return true;
    }

    private bool CanTeleport()
    {
        if (enemyController == null || enemyHealth == null) return false;
        if (enemyController.Stats == null) return false;
        if (enemyController.Stats.Archetype != EnemyArchetype.Cyan) return false;
        if (!enemyController.HasTarget) return false;
        if (!enemyHealth.IsAlive) return false;
        if (cooldownTimer > 0f) return false;

        return true;
    }

    private Vector2 GetTeleportDestination()
    {
        Transform target = enemyController.PlayerTarget;

        float enemyX = transform.position.x;
        float playerX = target.position.x;

        float direction = enemyX < playerX ? -1f : 1f;

        Vector2 destination = (Vector2)transform.position + Vector2.right * direction * teleportDistance;
        destination.y = transform.position.y;

        return destination;
    }

    private bool IsPositionSafe(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapCircle(position, validationRadius, blockingLayer);
        return hit == null;
    }

    public void ResetCooldown()
    {
        cooldownTimer = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        Gizmos.DrawWireSphere(transform.position, triggerDistance);

        if (enemyController != null && enemyController.PlayerTarget != null)
        {
            Vector2 destination = GetTeleportDestination();
            Gizmos.DrawWireSphere(destination, validationRadius);
            Gizmos.DrawLine(transform.position, destination);
        }
    }
}