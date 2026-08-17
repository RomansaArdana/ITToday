using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    [SerializeField] private PlayerState currentState = PlayerState.Idle;

    public PlayerState CurrentState => currentState;

    public bool IsMovementLocked =>
        currentState == PlayerState.Interact ||
        currentState == PlayerState.Hide ||
        currentState == PlayerState.Dead;

    public bool IsAlive =>
        currentState != PlayerState.Dead;

    public void SetState(PlayerState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;
    }

    public bool CanMove()
    {
        return !IsMovementLocked;
    }

    public bool CanInteract()
    {
        return currentState != PlayerState.Dead &&
               currentState != PlayerState.Hide;
    }

    public bool CanHide()
    {
        return currentState != PlayerState.Dead &&
               currentState != PlayerState.Interact;
    }

    public void UpdateMovementState(Vector2 movementInput)
    {
        if (IsMovementLocked)
        {
            return;
        }

        if (movementInput.sqrMagnitude > 0.01f)
        {
            SetState(PlayerState.Walk);
        }
        else
        {
            SetState(PlayerState.Idle);
        }
    }
}