using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    /*
    [Header("References")]
    [SerializeField] private EnemyController enemyController;

    [Header("Attack")]
    [SerializeField] private float attackDistance = 0.8f;
    [SerializeField] private float attackCooldown = 1f;

    private float attackTimer;

    private void Awake()
    {
        if (enemyController == null)
        {
            enemyController = GetComponent<EnemyController>();
        }
    }

    private void Update()
    {
        if (enemyController == null ||
            !enemyController.HasTarget)
        {
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
        {
            return;
        }

        if (!IsPlayerInAttackRange())
        {
            return;
        }

        Attack();
    }

    private bool IsPlayerInAttackRange()
    {
        float distance = Vector2.Distance(
            transform.position,
            enemyController.PlayerTarget.position
        );

        return distance <= attackDistance;
    }

    private void Attack()
    {
        PlayerDeathHandler deathHandler =
            enemyController.PlayerTarget.GetComponent<PlayerDeathHandler>();

        if (deathHandler == null)
        {
            return;
        }

        deathHandler.HandleDeath();

        attackTimer = attackCooldown;
    }*/
}