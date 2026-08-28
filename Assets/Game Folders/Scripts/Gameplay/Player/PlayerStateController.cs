using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    [SerializeField] private PlayerState currentState = PlayerState.Idle;
    [SerializeField] private SanityController sanityController;

    public PlayerState CurrentState => currentState;

    public bool IsSanityDepleted => sanityController != null && sanityController.IsDepleted;

    public bool IsMovementLocked =>
        currentState == PlayerState.Interact ||
        currentState == PlayerState.Hide ||
        currentState == PlayerState.Dead ||
        IsSanityDepleted;

    public bool IsAlive => currentState != PlayerState.Dead;

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

    public void UpdateMovementState(Vector2 movementInput)
    {
        if (IsMovementLocked) return;

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
}