using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJump : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatsSO stats;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStateController stateController;
    [SerializeField] private PlayerGroundCheck groundCheck;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private Rigidbody2D rb;

    public bool IsGrounded => groundCheck != null && groundCheck.IsGrounded;
    public bool IsJumping => rb != null && Mathf.Abs(rb.linearVelocity.y) > 0.05f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (stateController == null) stateController = GetComponent<PlayerStateController>();
        if (groundCheck == null) groundCheck = GetComponent<PlayerGroundCheck>();
    }

    private void Update()
    {
        if (inputReader == null || !inputReader.JumpPressed) return;
        TryJump();
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
}