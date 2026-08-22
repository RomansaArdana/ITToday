using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyStateController stateController;

    [SerializeField] private float patrolDistance = 2f;
    [SerializeField] private float arrivalDistance = 0.05f;
    [SerializeField] private float waitDuration = 1f;

    private Rigidbody2D rb;

    private Vector2 startPosition;
    private Vector2 targetPosition;

    private int patrolDirection = 1;
    private float waitTimer;

    private bool isWaiting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (enemyController == null)
        {
            enemyController = GetComponent<EnemyController>();
        }

        if (stateController == null)
        {
            stateController = GetComponent<EnemyStateController>();
        }

        startPosition = rb.position;

        SetNextPatrolTarget();
    }

    private void FixedUpdate()
    {
        if (enemyController == null ||
            stateController == null ||
            enemyController.Stats == null)
        {
            StopPatrol();
            return;
        }

        if (!stateController.IsUndetected)
        {
            StopPatrol();
            return;
        }

        UpdatePatrol();
    }

    private void UpdatePatrol()
    {
        if (isWaiting)
        {
            UpdateWait();
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 direction = targetPosition - currentPosition;

        if (direction.sqrMagnitude <= arrivalDistance * arrivalDistance)
        {
            StopMovement();

            isWaiting = true;
            waitTimer = 0f;

            return;
        }

        direction.Normalize();

        float speed = enemyController.Stats.MoveSpeed;

        rb.linearVelocity = direction * speed;

        UpdateFacingDirection(direction);
    }

    private void UpdateWait()
    {
        StopMovement();

        waitTimer += Time.fixedDeltaTime;

        if (waitTimer < waitDuration)
        {
            return;
        }

        patrolDirection *= -1;

        SetNextPatrolTarget();

        isWaiting = false;
        waitTimer = 0f;
    }

    private void SetNextPatrolTarget()
    {
        targetPosition =
            startPosition +
            Vector2.right *
            patrolDistance *
            patrolDirection;
    }

    private void UpdateFacingDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) < 0.01f)
        {
            return;
        }

        Vector3 scale = transform.localScale;

        scale.x =
            Mathf.Abs(scale.x) *
            Mathf.Sign(direction.x);

        transform.localScale = scale;
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void StopPatrol()
    {
        isWaiting = false;
        waitTimer = 0f;

        StopMovement();
    }
}