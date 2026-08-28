using UnityEngine;

public class EnemyIgnorePlayerCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;

    [Header("Collision")]
    [SerializeField] private string playerTag = "Player";

    private Collider2D[] enemyColliders;
    private Collider2D[] playerColliders;

    private void Awake()
    {
        enemyController ??= GetComponent<EnemyController>();

        enemyColliders = GetComponentsInChildren<Collider2D>();

        FindPlayerColliders();
    }

    private void Start()
    {
        if (enemyController == null || enemyController.Stats == null) return;
        if (enemyController.Stats.Archetype != EnemyArchetype.Red) return;

        IgnorePlayerCollision();
    }

    private void FindPlayerColliders()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            Debug.LogWarning("[EnemyIgnorePlayerCollision] Player tidak ditemukan.", this);
            return;
        }

        playerColliders = player.GetComponentsInChildren<Collider2D>();
    }

    private void IgnorePlayerCollision()
    {
        if (enemyColliders == null || playerColliders == null) return;

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            if (enemyCollider == null) continue;

            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider == null) continue;

                Physics2D.IgnoreCollision(enemyCollider, playerCollider, true);
            }
        }
    }
}