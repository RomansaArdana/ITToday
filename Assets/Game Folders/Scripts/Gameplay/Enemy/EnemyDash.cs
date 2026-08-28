using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyDash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyStateController stateController;
    [SerializeField] private EnemyChase chase;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.15f;

    [Header("Automatic Dash")]
    [SerializeField] private float triggerDistance = 2f;
    [SerializeField] private float dashForce = 7f;
    [SerializeField] private float dashCooldown = 2f;

    [Header("Hit Evade")]
    [SerializeField] private float evadeForce = 9f;
    [SerializeField] private float evadeDuration = 0.3f;
    [SerializeField] private float evadeCooldown = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool showGizmo = true;

    private Rigidbody2D rb;

    private float dashCooldownTimer;
    private float evadeCooldownTimer;
    private float evadeTimer;

    public bool IsGrounded { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsEvading { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        enemyController ??= GetComponent<EnemyController>();
        stateController ??= GetComponent<EnemyStateController>();
        chase ??= GetComponent<EnemyChase>();
    }

    private void Update()
    {
        UpdateGrounded();
        UpdateCooldowns();
        UpdateEvade();
    }

    private void UpdateGrounded()
    {
        if (groundCheck == null)
        {
            IsGrounded = false;
            return;
        }

        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    private void UpdateCooldowns()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer < 0f) dashCooldownTimer = 0f;
        }

        if (evadeCooldownTimer > 0f)
        {
            evadeCooldownTimer -= Time.deltaTime;
            if (evadeCooldownTimer < 0f) evadeCooldownTimer = 0f;
        }
    }

    private void UpdateEvade()
    {
        if (!IsEvading) return;

        evadeTimer -= Time.deltaTime;

        if (evadeTimer > 0f) return;

        EndEvade();
    }

    // =========================================================
    // AUTOMATIC DASH
    // =========================================================

    public bool TryDash()
    {
        if (!CanDash()) return false;

        float direction = GetAwayDirection();

        ApplyDash(direction, dashForce);

        dashCooldownTimer = Mathf.Max(0f, dashCooldown);

        LogAction("DASH");

        return true;
    }

    private bool CanDash()
    {
        if (rb == null || enemyController == null || stateController == null) return false;
        if (IsEvading) return false;
        if (enemyController.Stats == null) return false;
        if (enemyController.Stats.Archetype != EnemyArchetype.Cyan) return false;
        if (!stateController.IsChase) return false;
        if (!enemyController.HasTarget) return false;
        if (!IsGrounded) return false;
        if (dashCooldownTimer > 0f) return false;

        float distance = Vector2.Distance(transform.position, enemyController.PlayerTarget.position);
        return distance <= triggerDistance;
    }

    // =========================================================
    // HIT EVADE
    // =========================================================

    public bool TryEvade()
    {
        if (!CanEvade()) return false;

        BeginEvade();

        float direction = GetAwayDirection();

        ApplyDash(direction, evadeForce);

        evadeCooldownTimer = Mathf.Max(0f, evadeCooldown);
        evadeTimer = Mathf.Max(0.05f, evadeDuration);

        LogAction("EVADE");

        return true;
    }

    private bool CanEvade()
    {
        if (rb == null || enemyController == null) return false;
        if (enemyController.Stats == null) return false;
        if (enemyController.Stats.Archetype != EnemyArchetype.Cyan) return false;

        if (!enemyController.HasTarget)
        {
            if (enableDebugLog) Debug.Log("[EnemyAI] CYAN EVADE FAILED → No Target", this);
            return false;
        }

        if (!EnemyHealthAlive()) return false;
        if (evadeCooldownTimer > 0f) return false;
        if (IsEvading) return false;

        return true;
    }

    private void BeginEvade()
    {
        IsEvading = true;
        if (chase != null) chase.enabled = false;
    }

    private void EndEvade()
    {
        IsEvading = false;
        evadeTimer = 0f;

        if (chase != null && stateController != null && stateController.IsChase && !stateController.IsDead)
            chase.enabled = true;
    }

    private bool EnemyHealthAlive()
    {
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health == null) return true;
        return health.IsAlive;
    }

    private float GetAwayDirection()
    {
        if (enemyController == null || enemyController.PlayerTarget == null) return 1f;

        float playerX = enemyController.PlayerTarget.position.x;
        float enemyX = transform.position.x;

        return enemyX < playerX ? -1f : 1f;
    }

    private void ApplyDash(float direction, float force)
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector2.right * direction * force, ForceMode2D.Impulse);

        IsDashing = true;
        UpdateFacing(direction);
        IsDashing = false;
    }

    private void UpdateFacing(float direction)
    {
        if (Mathf.Abs(direction) < 0.01f) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction);
        transform.localScale = scale;
    }

    private void LogAction(string action)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[EnemyAI] CYAN → {action}", this);
    }

    public void StopDash()
    {
        IsDashing = false;
        IsEvading = false;

        dashCooldownTimer = 0f;
        evadeCooldownTimer = 0f;
        evadeTimer = 0f;

        if (chase != null) chase.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        Gizmos.DrawWireSphere(transform.position, triggerDistance);

        if (groundCheck != null) Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}