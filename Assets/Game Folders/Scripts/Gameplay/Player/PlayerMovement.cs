using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStateController stateController;
    [SerializeField] private PlayerStealth stealth;

    [Header("Facing")]
    [SerializeField] private bool facingRight = true;

    private Rigidbody2D rb;
    private Vector2 currentVelocity;
    private float dragMultiplier = 1f;

    public bool IsFacingRight => facingRight;
    public Vector2 FacingDirection => facingRight ? Vector2.right : Vector2.left;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (stateController == null) stateController = GetComponent<PlayerStateController>();
        if (stealth == null) stealth = GetComponent<PlayerStealth>();

        ApplyFacing();
    }

    private void FixedUpdate()
    {
        if (stats == null || inputReader == null) return;

        if (stateController != null && !stateController.CanMove())
        {
            StopMovement();
            return;
        }

        Vector2 input = Vector2.ClampMagnitude(inputReader.MoveInput, 1f);

        UpdateFacing(input);

        if (stateController != null) stateController.UpdateMovementState(input);

        float movementMultiplier = (stealth != null ? stealth.MovementMultiplier : 1f) * dragMultiplier;
        Vector2 targetVelocity = input * stats.MoveSpeed * movementMultiplier;

        float accelerationRate = input.sqrMagnitude > 0f ? stats.Acceleration : stats.Deceleration;

        currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, accelerationRate * Time.fixedDeltaTime);

        rb.linearVelocity = currentVelocity;
    }

    private void UpdateFacing(Vector2 input)
    {
        if (input.x > 0.01f) SetFacingRight(true);
        else if (input.x < -0.01f) SetFacingRight(false);
    }

    private void SetFacingRight(bool value)
    {
        if (facingRight == value) return;

        facingRight = value;
        ApplyFacing();
    }

    private void ApplyFacing()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
        transform.localScale = scale;
    }

    public void SetDragMultiplier(float multiplier)
    {
        dragMultiplier = Mathf.Clamp(multiplier, 0.05f, 1f);
    }

    public void StopMovement()
    {
        currentVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }
}