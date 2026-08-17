using UnityEngine;

public interface IPlayerInput
{
    Vector2 MoveInput { get; }
    bool InteractPressed { get; }
    bool HidePressed { get; }
}