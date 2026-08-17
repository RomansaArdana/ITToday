using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStateController stateController;

    private Rigidbody2D rb;
    private Vector2 currentVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (inputReader == null)
        {
            inputReader = GetComponent<PlayerInputReader>();
        }
        if (stateController == null)
        {
            stateController = GetComponent<PlayerStateController>();
        }
    }

    private void FixedUpdate()
    {
        if (stats == null || inputReader == null)
        {
            return;
        }
        if (stateController != null && !stateController.CanMove())
        {
            StopMovement();
            return;
        }

        Vector2 input = Vector2.ClampMagnitude(inputReader.MoveInput, 1f);
        Vector2 targetVelocity = input * stats.MoveSpeed;

        float accelerationRate = input.sqrMagnitude > 0f
            ? stats.Acceleration
            : stats.Deceleration;

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            targetVelocity,
            accelerationRate * Time.fixedDeltaTime
        );

        rb.linearVelocity = currentVelocity;
    }

    public void StopMovement()
    {
        currentVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }
}