using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyBlueContactDamage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;

    [Header("Damage")]
    [SerializeField] private float damageAmount = 20f;
    [SerializeField] private float damageCooldown = 1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private float damageTimer;
    private SanityController currentSanityController;

    private void Awake()
    {
        enemyController ??=
            GetComponent<EnemyController>();
    }

    private void Update()
    {
        if (damageTimer <= 0f)
        {
            return;
        }

        damageTimer -= Time.deltaTime;

        if (damageTimer < 0f)
        {
            damageTimer = 0f;
        }
    }

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        TryDamagePlayer(
            collision.gameObject
        );
    }

    private void OnCollisionStay2D(
        Collision2D collision)
    {
        TryDamagePlayer(
            collision.gameObject
        );
    }

    private void TryDamagePlayer(
        GameObject other)
    {
        if (damageTimer > 0f)
        {
            return;
        }

        if (other == null ||
            !other.CompareTag("Player"))
        {
            return;
        }

        if (enemyController == null ||
            enemyController.Stats == null)
        {
            return;
        }

        if (enemyController.Stats.Archetype !=
            EnemyArchetype.Blue)
        {
            return;
        }

        currentSanityController ??=
            other.GetComponent<SanityController>();

        if (currentSanityController == null)
        {
            Debug.LogWarning(
                "[BlueContact] SanityController tidak ditemukan.",
                this
            );

            return;
        }

        if (currentSanityController.IsDepleted)
        {
            return;
        }

        float previousSanity =
            currentSanityController.CurrentSanity;

        currentSanityController.DrainSanity(
            damageAmount
        );

        damageTimer =
            Mathf.Max(
                0f,
                damageCooldown
            );

        if (enableDebugLog)
        {
            float currentSanity =
                currentSanityController.CurrentSanity;

            Debug.Log(
                $"[BlueContact] Damage {damageAmount:F1} | " +
                $"{previousSanity:F1} → {currentSanity:F1}",
                this
            );
        }
    }
}