using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    private enum DeathReason
    {
        None,
        Drowned,
        LostSanity
    }

    [Header("References")]
    [SerializeField] private PlayerDeath playerDeath;
    [SerializeField] private PlayerRespawn playerRespawn;
    [SerializeField] private PlayerLife playerLife;
    [SerializeField] private SanityController sanityController;
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Water")]
    [SerializeField] private string waterZoneTag = "WaterZone";

    [SerializeField] private float gameOverDelay = 1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool enableDebugDeathKey = true;
    [SerializeField] private KeyCode debugDeathKey = KeyCode.K;

    private bool isInWaterZone;
    private bool waitingForDeathAnimation;
    private DeathReason currentDeathReason = DeathReason.None;

    private void Awake()
    {
        if (playerDeath == null) playerDeath = GetComponent<PlayerDeath>();
        if (playerRespawn == null) playerRespawn = GetComponent<PlayerRespawn>();
        if (playerLife == null) playerLife = GetComponent<PlayerLife>();
        if (sanityController == null) sanityController = GetComponent<SanityController>();
        if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameOver)
            return;

        if (waitingForDeathAnimation)
            return;

        if (enableDebugDeathKey && Input.GetKeyDown(debugDeathKey))
        {
            HandleDebugDeath();
            return;
        }

        if (sanityController != null && sanityController.IsDepleted)
            HandleSanityDeath();
    }

    private void HandleDebugDeath()
    {
        DeathReason reason = isInWaterZone ? DeathReason.Drowned : DeathReason.LostSanity;
        HandleDeath(reason);
    }

    private void HandleSanityDeath()
    {
        HandleDeath(DeathReason.LostSanity);
    }

    public void HandleDeath()
    {
        DeathReason reason = isInWaterZone ? DeathReason.Drowned : DeathReason.LostSanity;
        HandleDeath(reason);
    }

    private void HandleDeath(DeathReason reason)
    {
        if (playerDeath == null || playerLife == null || playerDeath.IsDead)
            return;

        currentDeathReason = reason;

        bool canRespawn = playerLife.ConsumeAttempt();

        Log($"Death reason: {currentDeathReason} | Attempt tersisa: {playerLife.Attempts}");

        playerDeath.Die();

        // ── SFX ───────────────────────────────────────────────────────────────
        AudioManager.Instance?.PlayRandomDeath();

        if (!canRespawn)
        {
            Log("Attempt habis → GAME OVER");
            GameOver();
            return;
        }

        Log("Masih ada attempt → RESPAWN");
        Respawn();
    }

    private void Respawn()
    {
        if (playerRespawn == null)
        {
            Debug.LogWarning("[Player Death] PlayerRespawn tidak ditemukan.", this);
            return;
        }

        playerRespawn.Respawn();

        if (sanityController != null)
            sanityController.RestoreFullSanity();

        playerDeath.Revive();
        currentDeathReason = DeathReason.None;
        waitingForDeathAnimation = false;

        Log($"Respawn berhasil | Attempt tersisa: {playerLife.Attempts}");
    }

    private void GameOver()
    {
        if (GameOverManager.Instance == null)
        {
            Debug.LogWarning("[Player Death] GameOverManager tidak ditemukan.", this);
            return;
        }

        if (waitingForDeathAnimation)
            return;

        waitingForDeathAnimation = true;

        if (playerMovement != null)
            playerMovement.SetMovementLocked(true);

        if (playerAnimator != null)
        {
            switch (currentDeathReason)
            {
                case DeathReason.Drowned:
                    Log("GAME OVER → DROWNED animation");
                    playerAnimator.TriggerDrowned();
                    break;

                case DeathReason.LostSanity:
                    Log("GAME OVER → SANITY LOST animation");
                    playerAnimator.TriggerSanityLost();
                    break;

                default:
                    Log("GAME OVER → UNKNOWN DEATH REASON");
                    playerAnimator.TriggerSanityLost();
                    break;
            }
        }
        else
        {
            FinishDeathAnimation();
        }
    }

    public void FinishDeathAnimation()
    {
        if (!waitingForDeathAnimation)
            return;

        waitingForDeathAnimation = false;
        StartCoroutine(ShowGameOverAfterDelay());
    }

    private System.Collections.IEnumerator ShowGameOverAfterDelay()
    {
        Log($"Death animation selesai → menunggu {gameOverDelay:F1}s sebelum Game Over");

        yield return new WaitForSecondsRealtime(gameOverDelay);

        Log("Delay selesai → membuka Game Over");

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.TriggerGameOver();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(waterZoneTag))
            isInWaterZone = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(waterZoneTag))
            isInWaterZone = false;
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[Player Death] {message}", this);
    }
}