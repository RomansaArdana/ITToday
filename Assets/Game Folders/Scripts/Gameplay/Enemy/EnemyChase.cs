using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChase : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyDetection detection;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        enemyController ??=
            GetComponent<EnemyController>();

        detection ??=
            GetComponent<EnemyDetection>();
    }

    private void FixedUpdate()
    {
        if (enemyController == null ||
            detection == null ||
            enemyController.Stats == null)
        {
            StopChase();
            return;
        }

        if (detection.CurrentState !=
            EnemyDetectionState.Detected)
        {
            StopChase();
            return;
        }

        if (!enemyController.HasTarget)
        {
            StopChase();
            return;
        }

        ChaseTarget();
    }

    private void ChaseTarget()
    {
        float difference =
            enemyController.PlayerTarget.position.x -
            transform.position.x;

        if (Mathf.Abs(difference) <=
            0.01f)
        {
            StopChase();
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

        UpdateFacingDirection(
            direction
        );
    }

    private void UpdateFacingDirection(
        float direction)
    {
        if (Mathf.Abs(direction) <
            0.01f)
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

    public void StopChase()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity =
            new Vector2(
                0f,
                rb.linearVelocity.y
            );
    }
}