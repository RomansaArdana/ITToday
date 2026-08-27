using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFlee : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyCrouch crouch;

    [Header("Flee")]
    [SerializeField] private float fleeSpeedMultiplier = 1.25f;

    [Header("Direction")]
    [SerializeField] private bool runRight = true;

    public bool IsFleeing { get; private set; }

    public Vector2 FleeDirection =>
        runRight
            ? Vector2.right
            : Vector2.left;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        enemyController ??=
            GetComponent<EnemyController>();

        crouch ??=
            GetComponent<EnemyCrouch>();
    }

    private void FixedUpdate()
    {
        if (enemyController == null ||
            enemyController.Stats == null ||
            !enemyController.HasTarget)
        {
            StopFlee();
            return;
        }

        Flee();
    }

    private void Flee()
    {
        IsFleeing = true;

        float direction =
            runRight ? 1f : -1f;

        float speed =
            enemyController.Stats.MoveSpeed *
            fleeSpeedMultiplier;

        if (crouch != null &&
            crouch.IsCrouching)
        {
            speed *= crouch.CrouchSpeedMultiplier;
        }

        rb.linearVelocity =
            new Vector2(
                direction * speed,
                rb.linearVelocity.y
            );

        UpdateFacing(direction);
    }

    private void UpdateFacing(
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

    public void StopFlee()
    {
        IsFleeing = false;

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