using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    [SerializeField] private PlayerState currentState = PlayerState.Idle;
    [SerializeField] private SanityController sanityController;

    public PlayerState CurrentState => currentState;

    public bool IsSanityDepleted => sanityController != null && sanityController.IsDepleted;

    // Crouch tidak masuk ke IsMovementLocked — Inara tetap bisa bergerak saat crouch
    public bool IsMovementLocked =>
        currentState == PlayerState.Interact ||
        currentState == PlayerState.Hide ||
        currentState == PlayerState.Dead ||
        IsSanityDepleted;

    public bool IsAlive => currentState != PlayerState.Dead;
    public bool IsCrouching => currentState == PlayerState.Crouch;

    private void Awake()
    {
        if (sanityController == null) sanityController = GetComponent<SanityController>();
    }

    public void SetState(PlayerState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    public bool CanMove() => !IsMovementLocked;

    public bool CanInteract()
    {
        return currentState != PlayerState.Dead &&
               currentState != PlayerState.Hide &&
               !IsSanityDepleted;
    }

    public bool CanHide()
    {
        return currentState != PlayerState.Dead &&
               currentState != PlayerState.Interact &&
               !IsSanityDepleted;
    }

    public bool CanCrouch()
    {
        return currentState != PlayerState.Dead &&
               currentState != PlayerState.Interact &&
               currentState != PlayerState.Hide &&
               !IsSanityDepleted;
    }

    public void UpdateMovementState(Vector2 movementInput)
    {
        if (IsMovementLocked) return;

        // Jangan override state Crouch dari sini — PlayerCrouch yang bertanggung jawab
        if (currentState == PlayerState.Crouch) return;

        if (movementInput.sqrMagnitude > 0.01f) SetState(PlayerState.Walk);
        else SetState(PlayerState.Idle);
    }

    public void EnterInteraction()
    {
        if (!CanInteract()) return;
        SetState(PlayerState.Interact);
    }

    public void ExitInteraction()
    {
        if (CurrentState == PlayerState.Interact) SetState(PlayerState.Idle);
    }

    /// <summary>
    /// Dipanggil oleh PlayerCrouch saat collider berhasil mengecil.
    /// </summary>
    public void EnterCrouch()
    {
        if (!CanCrouch()) return;
        SetState(PlayerState.Crouch);
    }

    /// <summary>
    /// Dipanggil oleh PlayerCrouch setelah ceiling check lulus dan collider kembali normal.
    /// </summary>
    public void ExitCrouch()
    {
        if (CurrentState != PlayerState.Crouch) return;
        SetState(PlayerState.Idle);
    }
}