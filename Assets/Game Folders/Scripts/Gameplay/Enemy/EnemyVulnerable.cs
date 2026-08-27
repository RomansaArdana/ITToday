using UnityEngine;

public class EnemyVulnerable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStateController stateController;
    [SerializeField] private Rigidbody2D rb;

    [Header("Vulnerable")]
    [SerializeField] private float reactionDelay = 0.15f;

    private float reactionTimer;

    public bool IsVulnerable { get; private set; }

    private void Awake()
    {
        stateController ??=
            GetComponent<EnemyStateController>();

        rb ??=
            GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        IsVulnerable = false;
        reactionTimer = 0f;

        StopMovement();
    }

    private void Update()
    {
        if (stateController == null)
        {
            return;
        }

        if (stateController.IsDead)
        {
            return;
        }

        // Vulnerable hanya boleh dimulai
        // setelah enemy benar-benar Cornered.
        if (!stateController.IsCornered &&
            !stateController.IsVulnerable)
        {
            StopMovement();
            return;
        }

        if (stateController.IsVulnerable)
        {
            return;
        }

        reactionTimer += Time.deltaTime;

        if (reactionTimer < reactionDelay)
        {
            return;
        }

        BecomeVulnerable();
    }

    private void BecomeVulnerable()
    {
        if (IsVulnerable)
        {
            return;
        }

        if (stateController == null ||
            !stateController.IsCornered)
        {
            return;
        }

        IsVulnerable = true;

        stateController.SetState(
            EnemyState.Vulnerable
        );

        StopMovement();
    }

    private void StopMovement()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public void StopVulnerable()
    {
        IsVulnerable = false;
        reactionTimer = 0f;

        StopMovement();
    }
}