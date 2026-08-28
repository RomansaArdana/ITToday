using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;

    [Header("Patrol")]
    [SerializeField] private float patrolDistance = 2f;
    [SerializeField] private float arrivalDistance = 0.05f;
    [SerializeField] private float waitDuration = 1f;

    private Rigidbody2D rb;

    private float startX;
    private float targetX;

    private int patrolDirection = 1;
    private float waitTimer;

    private bool isWaiting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        enemyController ??=
            GetComponent<EnemyController>();

        startX =
            rb.position.x;

        SetNextPatrolTarget();
    }

    private void FixedUpdate()
    {
        if (enemyController == null ||
            enemyController.Stats == null)
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

        float currentX =
            rb.position.x;

        float difference =
            targetX - currentX;

        if (Mathf.Abs(difference) <=
            arrivalDistance)
        {
            StopMovement();

            isWaiting = true;
            waitTimer = 0f;

            return;
        }

        float direction =
            Mathf.Sign(difference);

        float speed =
            enemyController.Stats.MoveSpeed;

        rb.linearVelocity =
            new Vector2(
                direction * speed,
                rb.linearVelocity.y
            );

        UpdateFacingDirection(direction);
    }

    private void UpdateWait()
    {
        StopMovement();

        waitTimer +=
            Time.fixedDeltaTime;

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
        targetX =
            startX +
            patrolDistance *
            patrolDirection;
    }

    private void UpdateFacingDirection(
        float direction)
    {
        if (Mathf.Abs(direction) < 0.01f)
        {
            return;
        }

        Vector3 scale =
            transform.localScale;

        scale.x =
            Mathf.Abs(scale.x) *
            Mathf.Sign(direction);

        transform.localScale =
            scale;
    }

    private void StopMovement()
    {
        rb.linearVelocity =
            new Vector2(
                0f,
                rb.linearVelocity.y
            );
    }

    public void StopPatrol()
    {
        isWaiting = false;
        waitTimer = 0f;

        StopMovement();
    }
}