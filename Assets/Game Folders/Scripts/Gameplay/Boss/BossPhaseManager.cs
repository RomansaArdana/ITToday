using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mengatur spawn minion berdasarkan HP bos yang tersisa.
/// Juga mengelola mekanik "Minion sebagai Health Pack":
/// setiap minion yang berhasil dipurnikan akan memulihkan 1 nyawa Inara.
///
/// Mapping HP sesuai GDD:
///   6->5 : Tidak ada minion (hanya Evil Inara)
///   5->4 : Merah (cepat)
///   4->3 : Biru (lambat, 2 HP)
///   3->2 : Cyan (gesit, butuh Stealth)
///   2->1 : Ungu (kamuflase, butuh Anomaly Detection)
///   1->0 : Semua tipe muncul bersamaan (Final Wave)
/// </summary>
public class BossPhaseManager : MonoBehaviour
{
    // ─── Definisi fase ─────────────────────────────────────
    [Serializable]
    public class BossPhase
    {
        [Tooltip("HP bos saat fase ini aktif (contoh: 5 = HP bos baru turun ke 5).")]
        public int bossHP;
        [Tooltip("Prefab minion yang di-spawn di fase ini.")]
        public GameObject[] minionPrefabs;
        [Tooltip("Titik spawn untuk minion. Jika kosong, muncul di posisi BossPhaseManager.")]
        public Transform[] spawnPoints;
    }

    [Header("References")]
    [SerializeField] private EvilInaraBossController bossController;
    [SerializeField] private BossChanceController playerChanceController;

    [Header("Phase Configuration")]
    [SerializeField] private List<BossPhase> phases = new List<BossPhase>();

    [Header("Minion Tracking")]
    [Tooltip("Tag yang dimiliki oleh semua minion di arena bos.")]
    [SerializeField] private string minionTag = "BossMinion";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    // ─── Runtime ───────────────────────────────────────────
    private readonly List<GameObject> activeMinions = new List<GameObject>();

    private void Awake()
    {
        bossController ??= FindObjectOfType<EvilInaraBossController>();
        playerChanceController ??= FindObjectOfType<BossChanceController>();
    }

    private void OnEnable()
    {
        if (bossController != null)
            bossController.OnHPChanged += HandleBossHPChanged;
    }

    private void OnDisable()
    {
        if (bossController != null)
            bossController.OnHPChanged -= HandleBossHPChanged;
    }

    // ─────────────────────────────────────────
    // HP Change Handler
    // ─────────────────────────────────────────

    private void HandleBossHPChanged(int newHP)
    {
        Log($"HP Bos berubah menjadi {newHP}. Mencari fase yang sesuai...");

        BossPhase matchingPhase = phases.Find(p => p.bossHP == newHP);

        if (matchingPhase == null)
        {
            Log($"Tidak ada fase yang terdefinisi untuk HP={newHP}.");
            return;
        }

        SpawnMinionsForPhase(matchingPhase);
    }

    // ─────────────────────────────────────────
    // Minion Spawning
    // ─────────────────────────────────────────

    private void SpawnMinionsForPhase(BossPhase phase)
    {
        Log($"Spawning {phase.minionPrefabs.Length} tipe minion untuk HP={phase.bossHP}...");

        foreach (GameObject prefab in phase.minionPrefabs)
        {
            if (prefab == null) continue;

            Vector3 spawnPos = GetSpawnPosition(phase);
            GameObject minion = Instantiate(prefab, spawnPos, Quaternion.identity);
            minion.tag = minionTag;
            activeMinions.Add(minion);

            // ═══ HEALTH PACK HOOK ═══════════════════════════
            // Subscribe ke event kematian minion untuk memulihkan nyawa Inara.
            // Coba cari komponen event pemberitahu kematian yang sudah ada.
            RegisterMinionDeathCallback(minion);
            // ════════════════════════════════════════════════

            Log($"Spawn: {prefab.name} di {spawnPos}");
        }
    }

    private Vector3 GetSpawnPosition(BossPhase phase)
    {
        // Bersihkan referensi null dari minion yang sudah hancur
        activeMinions.RemoveAll(m => m == null);

        if (phase.spawnPoints != null && phase.spawnPoints.Length > 0)
        {
            // Pilih spawn point secara random
            Transform sp = phase.spawnPoints[UnityEngine.Random.Range(0, phase.spawnPoints.Length)];
            if (sp != null) return sp.position;
        }

        // Fallback: spawn di sekitar posisi manager dengan offset acak
        return transform.position + new Vector3(
            UnityEngine.Random.Range(-2f, 2f),
            0f,
            0f
        );
    }

    // ─────────────────────────────────────────
    // Minion Death → Health Pack
    // ─────────────────────────────────────────

    /// <summary>
    /// Mendaftarkan callback ke minion agar saat dipurnikan,
    /// BossChanceController.RestoreChance() dipanggil.
    ///
    /// Cara kerjanya: Cari komponen IDamageable/EnemyStateController di minion.
    /// Gunakan pola polling via MonoBehaviour sederhana agar tidak modif script musuh.
    /// </summary>
    private void RegisterMinionDeathCallback(GameObject minion)
    {
        // Tambahkan komponen tracker ke minion secara dinamis
        BossMinionTracker tracker = minion.AddComponent<BossMinionTracker>();
        tracker.Initialize(this);
    }

    /// <summary>
    /// Dipanggil oleh BossMinionTracker saat minion mati/dipurnikan.
    /// </summary>
    public void OnMinionPurified(GameObject minion)
    {
        activeMinions.Remove(minion);
        Log($"Minion dipurnikan: {minion.name}. Memulihkan 1 nyawa Inara!");

        // ═══ HEALTH PACK MECHANIC ════════════════════════════
        playerChanceController?.RestoreChance();
        // ════════════════════════════════════════════════════
    }

    /// <summary>
    /// Bersihkan semua minion aktif (misalnya saat bos mati).
    /// </summary>
    public void ClearAllMinions()
    {
        foreach (GameObject minion in activeMinions)
        {
            if (minion != null)
                Destroy(minion);
        }
        activeMinions.Clear();
        Log("Semua minion dibersihkan.");
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[BossPhase] {message}", this);
    }
}
