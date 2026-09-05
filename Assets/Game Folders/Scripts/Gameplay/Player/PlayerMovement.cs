using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStateController stateController;
    [SerializeField] private PlayerStealth stealth;
    [SerializeField] private PlayerAnimator playerAnimator;

    [Header("Facing")]
    [SerializeField] private bool facingRight = true;

    private Rigidbody2D rb;
    private float currentVelocityX;
    private float dragMultiplier = 1f;
    private bool movementLocked;

    public bool IsFacingRight => facingRight;
    public Vector2 FacingDirection => facingRight ? Vector2.right : Vector2.left;
    public bool IsMovementLocked => movementLocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (stateController == null) stateController = GetComponent<PlayerStateController>();
        if (stealth == null) stealth = GetComponent<PlayerStealth>();
        if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();
        ApplyFacing();
    }

    private void FixedUpdate()
    {
        if (stats == null || inputReader == null) return;

        if (movementLocked)
        {
            StopMovement();
            return;
        }

        if (stateController != null && !stateController.CanMove())
        {
            StopMovement();
            return;
        }

        float inputX = Mathf.Clamp(inputReader.MoveInput.x, -1f, 1f);
        UpdateFacing(inputX);

        Vector2 movementInput = new Vector2(inputX, 0f);
        if (stateController != null) stateController.UpdateMovementState(movementInput);

        bool hasMovementInput = Mathf.Abs(inputX) > 0.01f;
        bool isRunning = hasMovementInput && inputReader.RunHeld && !inputReader.CrouchHeld;

        float baseSpeed = isRunning ? stats.RunSpeed : stats.MoveSpeed;
        float movementMultiplier = (stealth != null ? stealth.MovementMultiplier : 1f) * dragMultiplier;
        float targetVelocityX = inputX * baseSpeed * movementMultiplier;
        float accelerationRate = hasMovementInput ? stats.Acceleration : stats.Deceleration;

        currentVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, accelerationRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(currentVelocityX, rb.linearVelocity.y);

        UpdateAnimationSpeed(isRunning, movementMultiplier);
    }

    private void UpdateAnimationSpeed(bool isRunning, float movementMultiplier)
    {
        if (playerAnimator == null || stats == null) return;

        if (movementLocked)
        {
            playerAnimator.SetSpeed(0f);
            return;
        }

        float safeMultiplier = Mathf.Max(0.01f, movementMultiplier);

        if (isRunning)
        {
            float runNormalizedSpeed = Mathf.Clamp01(Mathf.Abs(currentVelocityX) / (stats.RunSpeed * safeMultiplier));
            playerAnimator.SetSpeed(runNormalizedSpeed * 2f);
        }
        else
        {
            float walkNormalizedSpeed = Mathf.Clamp01(Mathf.Abs(currentVelocityX) / (stats.MoveSpeed * safeMultiplier));
            playerAnimator.SetSpeed(walkNormalizedSpeed);
        }
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

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;

        if (locked)
            StopMovement();
    }

    public void StopMovement()
    {
        currentVelocityX = 0f;

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (playerAnimator != null)
            playerAnimator.SetSpeed(0f);
    }
}