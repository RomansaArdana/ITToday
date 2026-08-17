using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerStateController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;

    private PlayerInputReader inputReader;
    private PlayerMovement movement;
    private PlayerStateController stateController;

    public PlayerStatsSO Stats => stats;
    public IPlayerInput Input => inputReader;
    public PlayerState State => stateController.CurrentState;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
        movement = GetComponent<PlayerMovement>();
        stateController = GetComponent<PlayerStateController>();
    }

    public void SetMovementEnabled(bool enabled)
    {
        if (!enabled)
        {
            movement.StopMovement();
        }
    }
}