using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private PlayerDeath playerDeath;
    [SerializeField] private PlayerRespawn playerRespawn;
    [SerializeField] private PlayerLife playerLife;

    private void Awake()
    {
        if (playerDeath == null)
        {
            playerDeath = GetComponent<PlayerDeath>();
        }

        if (playerRespawn == null)
        {
            playerRespawn = GetComponent<PlayerRespawn>();
        }

        if (playerLife == null)
        {
            playerLife = GetComponent<PlayerLife>();
        }
    }

    private void Update()
    {
        if (GameOverManager.Instance != null &&
            GameOverManager.Instance.IsGameOver)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            HandleDeath();
        }
    }

    public void HandleDeath()
    {
        if (playerDeath == null ||
            playerLife == null ||
            playerDeath.IsDead)
        {
            return;
        }

        bool canRespawn = playerLife.ConsumeAttempt();

        playerDeath.Die();

        if (!canRespawn)
        {
            GameOver();
            return;
        }

        Respawn();
    }

    private void Respawn()
    {
        if (playerRespawn == null)
        {
            return;
        }

        playerRespawn.Respawn();
    }

    private void GameOver()
    {
        if (GameOverManager.Instance == null)
        {
            return;
        }

        GameOverManager.Instance.TriggerGameOver();
    }
}