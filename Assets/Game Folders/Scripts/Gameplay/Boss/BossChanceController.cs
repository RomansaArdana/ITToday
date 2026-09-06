using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Komponen Player Override untuk Boss Arena.
/// Menggantikan sistem Sanity dengan sistem 3-Hit nyawa mutlak sesuai GDD.
/// 
/// PENTING: Komponen ini DISABLED saat start, diaktifkan oleh BossArenaSetup
/// setelah interaksi Album Foto. SanityController di-disable bersamaan.
/// 
/// PlayerCloakOfInvisibility TIDAK disentuh — tetap berfungsi normal.
/// </summary>
public class BossChanceController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLife playerLife;
    [SerializeField] private PlayerDeathHandler playerDeathHandler;
    [SerializeField] private PlayerAnimator playerAnimator;

    [Header("Hit Settings")]
    [Tooltip("Durasi invulnerability frame setelah terkena hit (detik).")]
    [SerializeField] private float invulnerabilityDuration = 2f;
    [Tooltip("Batas maksimum nyawa (sama dengan startingAttempts di PlayerLife).")]
    [SerializeField] private int maxChances = 3;

    [Header("Screen Shake")]
    [SerializeField] private bool enableScreenShake = true;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    // ─── State ─────────────────────────────────────────────
    private bool isInvulnerable = false;
    private Coroutine invulnerabilityCoroutine;

    // ─── Events ────────────────────────────────────────────
    public event Action<int> OnChancesChanged; // Dipanggil tiap nyawa berubah, kirim sisa nyawa

    private void Awake()
    {
        playerLife ??= GetComponent<PlayerLife>();
        playerDeathHandler ??= GetComponent<PlayerDeathHandler>();
        playerAnimator ??= GetComponent<PlayerAnimator>();

        if (playerLife == null)
            Debug.LogError("[BossChance] PlayerLife tidak ditemukan!", this);
        if (playerDeathHandler == null)
            Debug.LogError("[BossChance] PlayerDeathHandler tidak ditemukan!", this);
    }

    private void OnEnable()
    {
        // Reset invulnerability saat komponen diaktifkan
        isInvulnerable = false;
        if (invulnerabilityCoroutine != null)
            StopCoroutine(invulnerabilityCoroutine);

        Log($"Boss Chance Controller aktif. Nyawa: {playerLife?.Attempts}");
    }

    // ─────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────

    /// <summary>
    /// Dipanggil oleh BossAttackController saat serangan AoE mengenai Player.
    /// </summary>
    public void TakeHit()
    {
        if (isInvulnerable)
        {
            Log("Invulnerable — hit diabaikan.");
            return;
        }

        if (playerLife == null) return;

        int remainingBefore = playerLife.Attempts;
        bool canContinue = playerLife.ConsumeAttempt();
        int remainingAfter = playerLife.Attempts;

        Log($"Terkena hit! Nyawa: {remainingBefore} -> {remainingAfter}");
        OnChancesChanged?.Invoke(remainingAfter);

        if (enableScreenShake)
            StartCoroutine(ScreenShake());

        if (!canContinue)
        {
            Log("Nyawa habis — GAME OVER!");
            playerDeathHandler?.HandleDeath();
            return;
        }

        Log($"Masih hidup. Sisa nyawa: {remainingAfter}. Memulai invulnerability {invulnerabilityDuration}s...");
        invulnerabilityCoroutine = StartCoroutine(InvulnerabilityFrames());
    }

    /// <summary>
    /// Dipanggil oleh BossPhaseManager saat Inara berhasil memurnikan minion.
    /// Minion berfungsi sebagai "Health Pack" — mekanik balancing kritis dari GDD.
    /// </summary>
    public void RestoreChance()
    {
        if (playerLife == null) return;
        if (playerLife.Attempts >= maxChances)
        {
            Log("Nyawa sudah penuh — restore diabaikan.");
            return;
        }

        // Tambahkan 1 nyawa menggunakan reflection pada field private Attempts
        // Menggunakan cara aman via property
        int before = playerLife.Attempts;

        // Karena PlayerLife.Attempts adalah { get; private set; },
        // kita reset dan set ulang via ResetAttempts trick
        // ATAU cara terbaik: kita pakai field internal. Namun agar tidak modif PlayerLife,
        // kita expose via UnityEvent atau cara di bawah.
        // Solusi bersih: panggil internal method melalui wrapper di sini.
        AddOneAttemptToPlayerLife();

        Log($"Minion dipurnikan! Nyawa pulih: {before} -> {playerLife.Attempts}");
        OnChancesChanged?.Invoke(playerLife.Attempts);
    }

    // ─────────────────────────────────────────
    // Internal
    // ─────────────────────────────────────────

    /// <summary>
    /// Menambahkan 1 nyawa ke PlayerLife tanpa memodifikasi script PlayerLife asli.
    /// Menggunakan reflection untuk mengakses backing field Attempts.
    /// </summary>
    private void AddOneAttemptToPlayerLife()
    {
        if (playerLife == null) return;

        // Gunakan reflection untuk set property private
        var field = typeof(PlayerLife).GetField(
            "<Attempts>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        if (field != null)
        {
            int current = playerLife.Attempts;
            field.SetValue(playerLife, Mathf.Min(current + 1, maxChances));
        }
        else
        {
            Debug.LogWarning("[BossChance] Tidak bisa akses field Attempts via reflection. " +
                             "Pertimbangkan menambahkan method AddAttempt() ke PlayerLife.", this);
        }
    }

    private IEnumerator InvulnerabilityFrames()
    {
        isInvulnerable = true;
        Log($"Invulnerable selama {invulnerabilityDuration}s...");

        // Opsional: flash visual player
        float elapsed = 0f;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        while (elapsed < invulnerabilityDuration)
        {
            elapsed += Time.deltaTime;
            if (sr != null)
                sr.color = Mathf.Sin(elapsed * 20f) > 0 ? Color.white : new Color(1f, 1f, 1f, 0.3f);
            yield return null;
        }

        if (sr != null) sr.color = Color.white;
        isInvulnerable = false;
        Log("Invulnerability selesai.");
    }

    private IEnumerator ScreenShake()
    {
        if (Camera.main == null) yield break;

        Transform camTransform = Camera.main.transform;
        Vector3 originalPos = camTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
            camTransform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            yield return null;
        }

        camTransform.localPosition = originalPos;
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[BossChance] {message}", this);
    }
}
