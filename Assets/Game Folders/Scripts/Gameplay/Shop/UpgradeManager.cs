using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mengelola state upgrade yang telah dibeli pemain dan memproses purchase.
///
/// Tanggung jawab:
/// - Menyimpan level upgrade saat ini (single source of truth untuk upgrade state)
/// - Memvalidasi purchase: level max, prerequisite, saldo Token
/// - Mendelegasikan pembayaran ke TokenManager
/// - Meng-apply nilai upgrade ke gameplay systems setelah purchase
///
/// Yang TIDAK dilakukan UpgradeManager:
/// - Menyimpan Token (→ TokenManager)
/// - Menampilkan UI (→ ShopUI)
/// - Mengubah Lantern/Sanity/Cloak secara langsung (→ via Apply methods)
///
/// Flow:
///   ShopUI.Buy() → UpgradeManager.TryPurchase() → TokenManager.TrySpend()
///                → level++ → ApplyUpgrade() → gameplay system updated
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Gameplay References")]
    [Tooltip("PlayerLanternAttack di scene. Auto-find jika kosong.")]
    [SerializeField] private PlayerLanternAttack lanternAttack;

    [Tooltip("SanityController di scene. Auto-find jika kosong.")]
    [SerializeField] private SanityController sanityController;

    [Tooltip("PlayerCloakOfInvisibility di scene. Auto-find jika kosong.")]
    [SerializeField] private PlayerCloakOfInvisibility cloakSystem;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    // ─────────────────────────────────────────────────────────────────────────
    // State
    // ─────────────────────────────────────────────────────────────────────────

    private readonly Dictionary<UpgradeType, int> upgradeLevels = new();

    // ─────────────────────────────────────────────────────────────────────────
    // Events
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Dipanggil setelah upgrade berhasil dibeli.
    /// Parameter: UpgradeType yang baru saja naik level.
    /// ShopUI subscribe ke event ini untuk refresh tampilan.
    /// </summary>
    public event Action<UpgradeType> OnUpgradePurchased;

    // ─────────────────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeLevels();
        Log("[UpgradeManager] Initialized.");
    }

    private void Start()
    {
        // Auto-find gameplay references jika tidak di-assign di Inspector
        if (lanternAttack == null) lanternAttack = FindFirstObjectByType<PlayerLanternAttack>();
        if (sanityController == null) sanityController = FindFirstObjectByType<SanityController>();
        if (cloakSystem == null) cloakSystem = FindFirstObjectByType<PlayerCloakOfInvisibility>();
    }

    private void InitializeLevels()
    {
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
            upgradeLevels[type] = 0;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Query API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Level upgrade saat ini untuk tipe yang diberikan.</summary>
    public int GetLevel(UpgradeType type)
    {
        return upgradeLevels.TryGetValue(type, out int level) ? level : 0;
    }

    /// <summary>
    /// Nilai gameplay kumulatif untuk tipe upgrade pada level saat ini.
    /// Contoh: LanternAOE Level 2 → radius tambahan total.
    /// </summary>
    public float GetCurrentValue(UpgradeDataSO data)
    {
        if (data == null) return 0f;
        return data.GetCumulativeValue(GetLevel(data.UpgradeType));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Purchase State
    // ─────────────────────────────────────────────────────────────────────────

    public enum PurchaseState
    {
        Available,          // Bisa dibeli
        Affordable,         // Bisa dibeli DAN Token cukup (ready to buy)
        InsufficientTokens, // Logic OK tapi Token tidak cukup
        Locked,             // Prerequisite belum terpenuhi
        MaxLevel            // Sudah level maksimum
    }

    /// <summary>
    /// Evaluasi state pembelian upgrade ini tanpa benar-benar melakukan pembelian.
    /// ShopUI menggunakan ini untuk menentukan warna/label tombol.
    /// </summary>
    public PurchaseState EvaluatePurchaseState(UpgradeDataSO data)
    {
        if (data == null) return PurchaseState.Locked;

        int currentLevel = GetLevel(data.UpgradeType);

        if (data.IsMaxLevel(currentLevel))
            return PurchaseState.MaxLevel;

        if (!IsPrerequisiteMet(data))
            return PurchaseState.Locked;

        int price = data.GetPriceForNextLevel(currentLevel);
        if (price < 0) return PurchaseState.Locked; // data tidak valid

        if (TokenManager.Instance == null || !TokenManager.Instance.HasEnough(price))
            return PurchaseState.InsufficientTokens;

        return PurchaseState.Affordable;
    }

    /// <summary>
    /// True jika upgrade dapat dibeli saat ini (state == Affordable).
    /// </summary>
    public bool CanPurchase(UpgradeDataSO data) =>
        EvaluatePurchaseState(data) == PurchaseState.Affordable;

    // ─────────────────────────────────────────────────────────────────────────
    // Purchase
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Coba beli upgrade. Memvalidasi semua kondisi, mendelegasikan pembayaran ke TokenManager,
    /// menaikkan level, lalu meng-apply nilai ke gameplay system.
    /// </summary>
    /// <returns>True jika berhasil dibeli.</returns>
    public bool TryPurchase(UpgradeDataSO data)
    {
        if (data == null)
        {
            Debug.LogWarning("[UpgradeManager] TryPurchase dipanggil dengan data null.", this);
            return false;
        }

        PurchaseState state = EvaluatePurchaseState(data);

        if (state != PurchaseState.Affordable)
        {
            Log($"[UpgradeManager] TryPurchase gagal [{data.DisplayName}] → State: {state}");
            return false;
        }

        int currentLevel = GetLevel(data.UpgradeType);
        int price = data.GetPriceForNextLevel(currentLevel);

        // Delegasikan pembayaran ke TokenManager
        if (!TokenManager.Instance.TrySpend(price))
        {
            Log($"[UpgradeManager] TryPurchase gagal [{data.DisplayName}] → TokenManager.TrySpend gagal.");
            return false;
        }

        // Naikkan level
        upgradeLevels[data.UpgradeType] = currentLevel + 1;
        int newLevel = upgradeLevels[data.UpgradeType];

        Log($"[UpgradeManager] Purchase berhasil [{data.DisplayName}] Level {currentLevel} → {newLevel}");

        // Apply ke gameplay system
        ApplyUpgrade(data, newLevel);

        OnUpgradePurchased?.Invoke(data.UpgradeType);
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Gameplay Application
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Push nilai upgrade ke gameplay system yang relevan.
    /// UpgradeManager-lah yang tahu bagaimana upgrade diterapkan — bukan ShopUI.
    /// </summary>
    private void ApplyUpgrade(UpgradeDataSO data, int newLevel)
    {
        float cumulativeValue = data.GetCumulativeValue(newLevel);

        switch (data.UpgradeType)
        {
            case UpgradeType.LanternAOE:
                ApplyLanternAOE(cumulativeValue);
                break;

            case UpgradeType.MaxSanity:
                ApplyMaxSanity(cumulativeValue);
                break;

            case UpgradeType.CloakDuration:
                ApplyCloak(cumulativeValue);
                break;

            default:
                Debug.LogWarning($"[UpgradeManager] ApplyUpgrade: UpgradeType [{data.UpgradeType}] tidak dikenali.", this);
                break;
        }
    }

    private void ApplyLanternAOE(float bonusRadius)
    {
        if (lanternAttack == null)
        {
            Debug.LogWarning("[UpgradeManager] PlayerLanternAttack tidak ditemukan — Lantern AOE tidak ter-apply.", this);
            return;
        }
        lanternAttack.SetAttackRadiusBonus(bonusRadius);
        Log($"[UpgradeManager] Lantern AOE bonus radius: +{bonusRadius:F2}");
    }

    private void ApplyMaxSanity(float bonusSanity)
    {
        if (sanityController == null)
        {
            Debug.LogWarning("[UpgradeManager] SanityController tidak ditemukan — Max Sanity tidak ter-apply.", this);
            return;
        }
        sanityController.SetMaxSanityBonus(bonusSanity);
        Log($"[UpgradeManager] Max Sanity bonus: +{bonusSanity:F1}");
    }

    private void ApplyCloak(float bonusDuration)
    {
        if (cloakSystem == null)
        {
            Debug.LogWarning("[UpgradeManager] PlayerCloakOfInvisibility tidak ditemukan — Cloak Duration tidak ter-apply.", this);
            return;
        }
        cloakSystem.SetDurationBonus(bonusDuration);
        Log($"[UpgradeManager] Cloak Duration bonus: +{bonusDuration:F1}s");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Prerequisite
    // ─────────────────────────────────────────────────────────────────────────

    private bool IsPrerequisiteMet(UpgradeDataSO data)
    {
        if (!data.HasPrerequisite) return true;
        return GetLevel(data.PrerequisiteType) >= data.PrerequisiteLevel;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Re-apply on scene load (jika gameplay objects baru di-load)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Re-apply semua upgrade yang sudah dibeli ke gameplay system baru di scene.
    /// Dipanggil saat berganti scene dan gameplay objects sudah selesai di-instantiate.
    /// Membutuhkan array UpgradeDataSO yang sama dengan yang tersedia di Shop.
    /// </summary>
    public void ReapplyAllUpgrades(UpgradeDataSO[] allUpgrades)
    {
        if (allUpgrades == null) return;

        // Refresh gameplay references
        if (lanternAttack == null) lanternAttack = FindFirstObjectByType<PlayerLanternAttack>();
        if (sanityController == null) sanityController = FindFirstObjectByType<SanityController>();
        if (cloakSystem == null) cloakSystem = FindFirstObjectByType<PlayerCloakOfInvisibility>();

        foreach (UpgradeDataSO data in allUpgrades)
        {
            if (data == null) continue;
            int level = GetLevel(data.UpgradeType);
            if (level <= 0) continue;

            float cumulativeValue = data.GetCumulativeValue(level);
            ApplyUpgrade(data, level);
            Log($"[UpgradeManager] Re-applied [{data.UpgradeType}] Level {level} = {cumulativeValue:F2}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Reset
    // ─────────────────────────────────────────────────────────────────────────

    public void ResetAllUpgrades()
    {
        InitializeLevels();
        Log("[UpgradeManager] Semua upgrade direset.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Debug
    // ─────────────────────────────────────────────────────────────────────────

    private void Log(string msg)
    {
        if (enableDebugLog) Debug.Log(msg, this);
    }

    [ContextMenu("Debug — Print All Upgrade Levels")]
    private void DebugPrint()
    {
        foreach (var kvp in upgradeLevels)
            Debug.Log($"[UpgradeManager] {kvp.Key}: Level {kvp.Value}", this);
    }

    [ContextMenu("Debug — Reset All Upgrades")]
    private void DebugReset() => ResetAllUpgrades();
}
