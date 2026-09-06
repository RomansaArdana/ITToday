using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Orkestrator Boss Arena. Mengelola dua fase:
///
/// FASE 1 — EKSPLORASI (Ruang Kenangan):
///   Semua komponen bos non-aktif. Pemain bisa menjelajah dengan tenang.
///   Trigger: Interaksi dengan objek Album Foto.
///
/// FASE 2 — BATTLE:
///   Dipicu oleh Album Foto. SanityController di-disable,
///   BossChanceController di-enable, bos muncul, cover di-spawn.
/// </summary>
public class BossArenaSetup : MonoBehaviour
{
    // ─────────────────────────────────────────
    // References
    // ─────────────────────────────────────────

    [Header("Player References")]
    [SerializeField] private SanityController sanityController;
    [SerializeField] private BossChanceController bossChanceController;

    [Header("Boss References")]
    [SerializeField] private EvilInaraBossController bossController;
    [SerializeField] private BossAttackController bossAttackController;
    [SerializeField] private BossPhaseManager bossPhaseManager;
    [SerializeField] private BossHealthBarUI bossHealthBarUI;

    [Header("Album Foto Trigger")]
    [Tooltip("Objek yang berisi komponen ObjectInteractable (Album Foto).")]
    [SerializeField] private AlbumPhotoInteractable albumPhoto;

    [Header("Cover Objects")]
    [Tooltip("Semua objek BossCover yang ada di arena.")]
    [SerializeField] private BossCover[] coverObjects;

    [Header("Cutscene / Transition")]
    [Tooltip("Durasi animasi transisi sebelum bos mulai bergerak (detik).")]
    [SerializeField] private float bossCutsceneDuration = 2f;

    [Header("Events")]
    [Tooltip("Dipanggil saat Fase Eksplorasi dimulai (scene load).")]
    public UnityEvent OnExplorationPhaseStart;
    [Tooltip("Dipanggil saat transisi ke Fase Battle setelah Album Foto.")]
    public UnityEvent OnBattlePhaseStart;
    [Tooltip("Dipanggil saat bos berhasil dikalahkan.")]
    public UnityEvent OnBossDefeated;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    // ─────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────

    private void Awake()
    {
        // Auto-find jika belum diisi di Inspector
        if (sanityController == null)
            sanityController = FindObjectOfType<SanityController>();
        if (bossChanceController == null)
            bossChanceController = FindObjectOfType<BossChanceController>();
        if (bossController == null)
            bossController = FindObjectOfType<EvilInaraBossController>();
        if (bossAttackController == null)
            bossAttackController = FindObjectOfType<BossAttackController>();
        if (bossPhaseManager == null)
            bossPhaseManager = FindObjectOfType<BossPhaseManager>();
    }

    private void Start()
    {
        StartExplorationPhase();
    }

    // ─────────────────────────────────────────
    // Fase 1: Eksplorasi
    // ─────────────────────────────────────────

    private void StartExplorationPhase()
    {
        Log("=== FASE EKSPLORASI DIMULAI ===");

        // Non-aktifkan semua komponen bos
        SetBossComponentsActive(false);

        // Pastikan BossChanceController non-aktif (SanityController tetap aktif)
        if (bossChanceController != null)
            bossChanceController.enabled = false;

        // Restore semua cover ke kondisi utuh
        foreach (BossCover cover in coverObjects)
            cover?.Restore();

        // Subscribe ke trigger Album Foto
        if (albumPhoto != null)
            albumPhoto.OnAlbumInteracted += TriggerBattlePhase;

        OnExplorationPhaseStart?.Invoke();
        Log("Pemain bisa menjelajah. Interaksi Album Foto akan memulai Boss Battle.");
    }

    // ─────────────────────────────────────────
    // Fase 2: Battle
    // ─────────────────────────────────────────

    private void TriggerBattlePhase()
    {
        Log("=== ALBUM FOTO DIINTERAKSI — TRANSISI KE BATTLE PHASE ===");

        // Unsubscribe agar tidak trigger dua kali
        if (albumPhoto != null)
            albumPhoto.OnAlbumInteracted -= TriggerBattlePhase;

        StartCoroutine(BattlePhaseTransition());
    }

    private IEnumerator BattlePhaseTransition()
    {
        OnBattlePhaseStart?.Invoke();

        Log($"Cutscene bos muncul selama {bossCutsceneDuration}s...");
        yield return new WaitForSeconds(bossCutsceneDuration);

        // ─── Switch sistem player ──────────────────────────
        // 1. Matikan SanityController (tidak ada sanity bar di boss fight)
        if (sanityController != null)
        {
            sanityController.enabled = false;
            Log("SanityController: DISABLED");
        }

        // 2. Aktifkan BossChanceController (3-hit system)
        if (bossChanceController != null)
        {
            bossChanceController.enabled = true;
            Log("BossChanceController: ENABLED");
        }
        // Note: PlayerCloakOfInvisibility dibiarkan aktif sesuai GDD.

        // ─── Aktifkan semua komponen bos ──────────────────
        SetBossComponentsActive(true);

        // ─── Subscribe ke event kemenangan ────────────────
        if (bossController != null)
            bossController.OnBossDefeated += HandleBossDefeated;

        Log("=== BOSS BATTLE DIMULAI ===");
    }

    // ─────────────────────────────────────────
    // Boss Defeated
    // ─────────────────────────────────────────

    private void HandleBossDefeated()
    {
        Log("=== BOSS DIKALAHKAN! ===");

        SetBossComponentsActive(false);
        bossPhaseManager?.ClearAllMinions();
        OnBossDefeated?.Invoke();
    }

    // ─────────────────────────────────────────
    // Utility
    // ─────────────────────────────────────────

    private void SetBossComponentsActive(bool active)
    {
        if (bossController != null) bossController.enabled = active;
        if (bossAttackController != null) bossAttackController.enabled = active;
        if (bossPhaseManager != null) bossPhaseManager.enabled = active;
        if (bossHealthBarUI != null) bossHealthBarUI.gameObject.SetActive(active);
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[BossArenaSetup] {message}", this);
    }
}
