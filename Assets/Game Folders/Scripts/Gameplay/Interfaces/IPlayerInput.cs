using UnityEngine;

public interface IPlayerInput
{
    Vector2 MoveInput { get; }
    bool InteractPressed { get; }
    bool HidePressed { get; }
    bool JumpPressed { get; }
    bool AttackPressed { get; }
    bool AttackHeld { get; }
    bool CloakPressed { get; }
    bool CrouchHeld { get; }
}