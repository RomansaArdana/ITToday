using UnityEngine;

public interface IPlayerInput
{
    Vector2 MoveInput { get; }

    bool InteractPressed { get; }

    bool HidePressed { get; }

    bool JumpPressed { get; }

    // true selama tombol Attack (F) ditekan, false ketika dilepas
    bool AttackHeld { get; }
}