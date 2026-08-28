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
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float fallGravityMultiplier = 2.2f;
    [SerializeField] private float minimumFallSpeed = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private Rigidbody2D rb;
    private float baseGravityScale;

    public bool IsGrounded => groundCheck != null && groundCheck.IsGrounded;
    public bool IsJumping => rb != null && !IsGrounded && Mathf.Abs(rb.linearVelocity.y) > minimumFallSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (stateController == null) stateController = GetComponent<PlayerStateController>();
        if (groundCheck == null) groundCheck = GetComponent<PlayerGroundCheck>();

        baseGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        if (inputReader == null || !inputReader.JumpPressed) return;
        TryJump();
    }

    private void FixedUpdate()
    {
        UpdateGravity();
    }

    private void TryJump()
    {
        if (rb == null) return;
        if (stateController != null && !stateController.CanMove()) return;
        if (!IsGrounded) return;

        Vector2 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;

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