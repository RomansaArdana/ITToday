using UnityEngine;

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyLanternTarget : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private EnemyStateController stateController;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    public bool CanBeHit
    {
        get
        {
            if (enemyHealth == null ||
                !enemyHealth.IsAlive)
            {
                return false;
            }

            if (enemyController == null ||
                enemyController.Stats == null)
            {
                return false;
            }

            switch (enemyController.Stats.Archetype)
            {
                case EnemyArchetype.Red:
                    return stateController != null &&
                           stateController.IsVulnerable;

                default:
                    return true;
            }
        }
    }

    private void Awake()
    {
        enemyController ??=
            GetComponent<EnemyController>();

        enemyHealth ??=
            GetComponent<EnemyHealth>();

        stateController ??=
            GetComponent<EnemyStateController>();
    }

    public bool TryReceiveLanternHit(float damage)
    {
        if (!CanBeHit)
        {
            LogRejectedHit();
            return false;
        }

        if (damage <= 0f)
        {
            return false;
        }

        enemyHealth.TakeDamage(damage);

        if (enableDebugLog)
        {
            string archetype =
                enemyController != null &&
                enemyController.Stats != null
                    ? enemyController.Stats.Archetype.ToString().ToUpper()
                    : gameObject.name;

            Debug.Log(
                $"[Lantern] {archetype} HIT",
                this
            );
        }

        return true;
    }

    private void LogRejectedHit()
    {
        if (!enableDebugLog)
        {
            return;
        }

        if (enemyController == null ||
            enemyController.Stats == null)
        {
            return;
        }

        if (enemyController.Stats.Archetype ==
            EnemyArchetype.Red &&
            stateController != null &&
            !stateController.IsVulnerable)
        {
            Debug.Log(
                "[Lantern] RED MISS",
                this
            );
        }
    }
}