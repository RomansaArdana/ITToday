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

        if (enemyController == null)
        {
            enemyController = GetComponent<EnemyController>();
        }

        if (detection == null)
        {
            detection = GetComponent<EnemyDetection>();
        }
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

        if (detection.CurrentState != EnemyDetectionState.Detected)
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
        Vector2 currentPosition = rb.position;
        Vector2 targetPosition =
            enemyController.PlayerTarget.position;

        Vector2 direction =
            targetPosition - currentPosition;

        if (direction.sqrMagnitude <= 0.001f)
        {
            StopChase();
            return;
        }

        direction.Normalize();

        float speed = enemyController.Stats.MoveSpeed;

        rb.linearVelocity = direction * speed;

        UpdateFacingDirection(direction);
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

    public void StopChase()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
    }
}