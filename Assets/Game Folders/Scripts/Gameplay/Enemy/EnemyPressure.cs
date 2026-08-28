using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPressure : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;

    [Header("Pressure")]
    [SerializeField] private float pressureSpeedMultiplier = 0.65f;
    [SerializeField] private float stopDistance = 1.2f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private Rigidbody2D rb;

    public bool IsPressuring { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        enemyController ??=
            GetComponent<EnemyController>();
    }

    private void FixedUpdate()
    {
        if (enemyController == null ||
            enemyController.Stats == null ||
            !enemyController.HasTarget)
        {
            StopPressure();
            return;
        }

        PressureTarget();
    }

    private void PressureTarget()
    {
        float difference =
            enemyController.PlayerTarget.position.x -
            transform.position.x;

        float distance =
            Mathf.Abs(difference);

        if (distance <= stopDistance)
        {
            StopPressure();
            return;
        }

        float direction =
            Mathf.Sign(difference);

        float speed =
            enemyController.Stats.MoveSpeed *
            pressureSpeedMultiplier;

        rb.linearVelocity =
            new Vector2(
                direction * speed,
                rb.linearVelocity.y
            );

        UpdateFacing(direction);

        if (!IsPressuring)
        {
            IsPressuring = true;

            if (enableDebugLog)
            {
                Debug.Log(
                    "[EnemyAI] BLUE → PRESSURE",
                    this
                );
            }
        }
    }

    private void UpdateFacing(float direction)
    {
        if (Mathf.Abs(direction) <= 0.01f)
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

    public void StopPressure()
    {
        if (IsPressuring &&
            enableDebugLog)
        {
            Debug.Log(
                "[EnemyAI] BLUE → STOP",
                this
            );
        }

        IsPressuring = false;

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