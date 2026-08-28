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
    private float currentVelocityX;
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

        float inputX = Mathf.Clamp(inputReader.MoveInput.x, -1f, 1f);
        UpdateFacing(inputX);

        Vector2 movementInput = new Vector2(inputX, 0f);
        if (stateController != null) stateController.UpdateMovementState(movementInput);

        float movementMultiplier = (stealth != null ? stealth.MovementMultiplier : 1f) * dragMultiplier;
        float targetVelocityX = inputX * stats.MoveSpeed * movementMultiplier;
        float accelerationRate = Mathf.Abs(inputX) > 0.01f ? stats.Acceleration : stats.Deceleration;

        currentVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, accelerationRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(currentVelocityX, rb.linearVelocity.y);
    }

    private void UpdateFacing(float inputX)
    {
        if (inputX > 0.01f) SetFacingRight(true);
        else if (inputX < -0.01f) SetFacingRight(false);
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
        currentVelocityX = 0f;

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
}