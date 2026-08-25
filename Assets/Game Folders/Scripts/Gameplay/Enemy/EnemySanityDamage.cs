using UnityEngine;

public class EnemySanityDamage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private SanityController sanityController;

    [Header("Sanity Damage")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float damageDistance = 1f;

    private float damageTimer;

    private void Awake()
    {
        enemyController ??= GetComponent<EnemyController>();
    }

    private void Update()
    {
        if (enemyController == null || !enemyController.HasTarget)
        {
            ResetTimer();
            return;
        }

        if (sanityController == null)
        {
            sanityController = enemyController.PlayerTarget.GetComponent<SanityController>();
        }

        if (sanityController == null)
        {
            ResetTimer();
            return;
        }

        if (!IsPlayerInDamageRange())
        {
            ResetTimer();
            return;
        }

        damageTimer += Time.deltaTime;

        if (damageTimer >= damageInterval)
        {
            ApplyDamage();
            damageTimer = 0f;
        }
    }

    private bool IsPlayerInDamageRange()
    {
        float distance = Vector2.Distance(
            transform.position,
            enemyController.PlayerTarget.position
        );

        return distance <= damageDistance;
    }

    private void ApplyDamage()
    {
        sanityController.DrainSanity(damageAmount);
    }

    private void ResetTimer()
    {
        damageTimer = 0f;
    }
}