using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyJump : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStateController stateController;
    [SerializeField] private EnemyObstacleDetector obstacleDetector;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float jumpCooldown = 0.7f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private Rigidbody2D rb;
    private float jumpTimer;

    public bool IsJumping =>
        rb != null && Mathf.Abs(rb.linearVelocity.y) > 0.05f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        stateController ??=
            GetComponent<EnemyStateController>();

        obstacleDetector ??=
            GetComponent<EnemyObstacleDetector>();
    }

    private void Update()
    {
        if (jumpTimer > 0f)
        {
            jumpTimer -= Time.deltaTime;
        }
    }

    public bool TryJump()
    {
        if (rb == null)
        {
            return false;
        }

        if (stateController == null ||
            !stateController.IsFlee)
        {
            return false;
        }

        if (obstacleDetector == null ||
            !obstacleDetector.IsObstacleDetected)
        {
            return false;
        }

        if (jumpTimer > 0f)
        {
            return false;
        }

        if (IsJumping)
        {
            return false;
        }

        Vector2 velocity =
            rb.linearVelocity;

        velocity.y = 0f;

        rb.linearVelocity =
            velocity;

        rb.AddForce(
            Vector2.up * jumpForce,
            ForceMode2D.Impulse
        );

        jumpTimer =
            jumpCooldown;

        if (enableDebugLog)
        {
            Debug.Log(
                "[EnemyAI] RED → JUMP",
                this
            );
        }

        return true;
    }
}