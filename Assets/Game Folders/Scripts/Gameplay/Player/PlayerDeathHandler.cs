using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerDeath playerDeath;
    [SerializeField] private PlayerRespawn playerRespawn;
    [SerializeField] private PlayerLife playerLife;
    [SerializeField] private SanityController sanityController;

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

        if (sanityController == null)
        {
            sanityController = GetComponent<SanityController>();
        }
    }

    private void Update()
    {
        if (GameOverManager.Instance != null &&
            GameOverManager.Instance.IsGameOver)
        {
            return;
        }

        // DEBUG DEATH
        if (Input.GetKeyDown(KeyCode.K))
        {
            HandleDeath();
            return;
        }

        // SANITY DEPLETED
        if (sanityController != null &&
            sanityController.IsDepleted)
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

        Debug.Log(
            $"[Player Death] Attempt tersisa: {playerLife.Attempts}",
            this
        );

        playerDeath.Die();

        if (!canRespawn)
        {
            Debug.Log(
                "[Player Death] Attempt habis → GAME OVER",
                this
            );

            GameOver();
            return;
        }

        Debug.Log(
            "[Player Death] Masih ada attempt → RESPAWN",
            this
        );

        Respawn();
    }

    private void Respawn()
    {
        if (playerRespawn == null)
        {
            return;
        }

        playerRespawn.Respawn();

        if (sanityController != null)
        {
            sanityController.RestoreFullSanity();
        }

        playerDeath.Revive();

        Debug.Log(
            $"[Player Respawn] Berhasil respawn | Attempt tersisa: {playerLife.Attempts}",
            this
        );
    }

    private void GameOver()
    {
        if (GameOverManager.Instance == null)
        {
            Debug.LogWarning(
                "[Player Death] GameOverManager tidak ditemukan.",
                this
            );

            return;
        }

        GameOverManager.Instance.TriggerGameOver();
    }
}