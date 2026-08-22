using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private PlayerDeath playerDeath;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (playerDeath == null)
            playerDeath = GetComponent<PlayerDeath>();
    }

    public void Respawn()
    {
        if (respawnPoint == null || playerDeath == null) return;

        transform.position = respawnPoint.position;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        playerDeath.Revive();
    }
}