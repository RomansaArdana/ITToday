using UnityEngine;

public class EnemySanityDamage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private SanityController sanityController;

    [Header("Damage")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float damageDistance = 1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;

    private float damageTimer;

    private void Awake()
    {
        if (enemyController == null) enemyController = GetComponent<EnemyController>();
    }

    private void Update()
    {
        if (enemyController == null || !enemyController.HasTarget)
        {
            ResetTimer();
            return;
        }

        if (sanityController == null)
            sanityController = enemyController.PlayerTarget.GetComponent<SanityController>();

        if (sanityController == null)
        {
            ResetTimer();
            return;
        }

        if (sanityController.IsDepleted)
        {
            ResetTimer();
            return;
        }

        if (!IsPlayerInDamageRange())
        {
            ResetTimer();
            return;
        }

        UpdateDamage();
    }

    private void UpdateDamage()
    {
        damageTimer += Time.deltaTime;

        if (damageTimer < damageInterval) return;

        damageTimer = 0f;

        ApplyDamage();
    }

    private bool IsPlayerInDamageRange()
    {
        float distance = Vector2.Distance(transform.position, enemyController.PlayerTarget.position);
        return distance <= damageDistance;
    }

    private void ApplyDamage()
    {
        float previousSanity = sanityController.CurrentSanity;

        sanityController.DrainSanity(damageAmount);

        float currentSanity = sanityController.CurrentSanity;

        if (!enableDebugLog) return;

        Debug.Log($"[EnemySanityDamage] Sanity: {previousSanity:F1} → {currentSanity:F1} (-{damageAmount:F1})", this);
    }

    private void ResetTimer()
    {
        damageTimer = 0f;
    }
}