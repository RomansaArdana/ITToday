using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJump : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatsSO stats;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStateController stateController;
    [SerializeField] private PlayerGroundCheck groundCheck;

    [Header("Jump Tuning")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float fallGravityMultiplier = 2.2f;
    [SerializeField] private float minimumFallSpeed = 0.05f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;

    private Rigidbody2D rb;
    private float baseGravityScale;
    private float coyoteCounter;
    private float jumpBufferCounter;

    public bool IsGrounded => groundCheck != null && groundCheck.IsGrounded;
    public bool IsJumping => rb != null && !IsGrounded && Mathf.Abs(rb.linearVelocity.y) > minimumFallSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (stateController == null) stateController = GetComponent<PlayerStateController>();
        if (groundCheck == null) groundCheck = GetComponent<PlayerGroundCheck>();

        if (jumpForce <= 0.1f && stats != null && stats.JumpForce > 0.1f)
        {
            jumpForce = stats.JumpForce;
        }

        baseGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        if (IsGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        if (inputReader != null && inputReader.JumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            TryJump();
        }
    }

    private void FixedUpdate()
    {
        UpdateGravity();
    }

    private void TryJump()
    {
        if (rb == null) return;
        if (stateController != null && !stateController.CanMove()) return;

        jumpBufferCounter = 0f;
        coyoteCounter = 0f;

        Vector2 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;

        // ── SFX ───────────────────────────────────────────────────────────────
        AudioManager.Instance?.PlaySFX("jump");

        if (enableDebugLog) Debug.Log("[PlayerJump] JUMP", this);
    }

    private void UpdateGravity()
    {
        if (IsGrounded || rb.linearVelocity.y >= 0f)
        {
            rb.gravityScale = baseGravityScale;
            return;
        }

        rb.gravityScale = baseGravityScale * fallGravityMultiplier;
    }

    public void ResetGravity()
    {
        rb.gravityScale = baseGravityScale;
    }
}