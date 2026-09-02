using System;
using UnityEngine;

/// <summary>
/// Single source of truth untuk seluruh resource dan item yang dimiliki pemain.
///
/// Sistem lain (Shop, Gate, Memory, NPC) harus membaca dari sini.
/// Tidak ada sistem lain yang boleh menyimpan salinan state Token, FinalKey, atau CloakOfInvisibility.
///
/// Architecture:
///   Enemy Reward → InventoryManager.AddTokens()
///   Shop         → InventoryManager.SpendTokens()
///   Final Gate   → InventoryManager.HasFinalKey
///   Kedai        → InventoryManager.AcquireFinalKey / AcquireCloak
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    // ─────────────────────────────────────────────────────────────────────────
    // State
    // ─────────────────────────────────────────────────────────────────────────

    private int tokenCount;
    private bool hasFinalKey;
    private bool hasCloakOfInvisibility;

    // ─────────────────────────────────────────────────────────────────────────
    // Properties (Read-Only)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Jumlah Token yang saat ini dimiliki pemain.</summary>
    public int TokenCount => tokenCount;

    /// <summary>True jika pemain sudah memiliki Final Key dari Penjaga Kedai.</summary>
    public bool HasFinalKey => hasFinalKey;

    /// <summary>True jika pemain sudah memiliki Cloak of Invisibility.</summary>
    public bool HasCloakOfInvisibility => hasCloakOfInvisibility;

    // ─────────────────────────────────────────────────────────────────────────
    // Events
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Dipanggil setiap kali TokenCount berubah. Parameter: nilai baru.</summary>
    public event Action<int> OnTokenCountChanged;

    /// <summary>Dipanggil saat Final Key diperoleh.</summary>
    public event Action OnFinalKeyAcquired;

    /// <summary>Dipanggil saat Cloak of Invisibility diperoleh.</summary>
    public event Action OnCloakAcquired;

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

        Log("[InventoryManager] Initialized.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Token API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tambahkan Token ke inventory. Dipanggil oleh EnemyReward atau sistem reward lainnya.
    /// </summary>
    /// <param name="amount">Jumlah Token yang ditambahkan. Harus > 0.</param>
    public void AddTokens(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[InventoryManager] AddTokens dipanggil dengan amount <= 0: {amount}", this);
            return;
        }

        int previous = tokenCount;
        tokenCount += amount;

        Log($"[InventoryManager] AddTokens: {previous} → {tokenCount} (+{amount})");
        OnTokenCountChanged?.Invoke(tokenCount);
    }

    /// <summary>
    /// Kurangi Token dari inventory. Gunakan ini di Shop System.
    /// </summary>
    /// <param name="amount">Jumlah Token yang dikurangi. Harus > 0.</param>
    /// <returns>True jika berhasil dikurangi. False jika saldo tidak mencukupi.</returns>
    public bool SpendTokens(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[InventoryManager] SpendTokens dipanggil dengan amount <= 0: {amount}", this);
            return false;
        }

        if (tokenCount < amount)
        {
            Log($"[InventoryManager] SpendTokens gagal — tidak cukup Token. Dimiliki: {tokenCount}, Dibutuhkan: {amount}");
            return false;
        }

        int previous = tokenCount;
        tokenCount -= amount;

        Log($"[InventoryManager] SpendTokens: {previous} → {tokenCount} (-{amount})");
        OnTokenCountChanged?.Invoke(tokenCount);
        return true;
    }

    /// <summary>
    /// Cek apakah pemain memiliki cukup Token tanpa menguranginya.
    /// Berguna untuk Shop System saat menampilkan tombol yang aktif/nonaktif.
    /// </summary>
    public bool HasEnoughTokens(int amount) => tokenCount >= amount;

    // ─────────────────────────────────────────────────────────────────────────
    // Item API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tandai pemain sebagai pemilik Final Key.
    /// Dipanggil oleh Penjaga Kedai saat final chapter.
    /// </summary>
    public void AcquireFinalKey()
    {
        if (hasFinalKey)
        {
            Log("[InventoryManager] AcquireFinalKey dipanggil tapi sudah dimiliki.");
            return;
        }

        hasFinalKey = true;
        Log("[InventoryManager] Final Key diperoleh.");
        OnFinalKeyAcquired?.Invoke();
    }

    /// <summary>
    /// Tandai pemain sebagai pemilik Cloak of Invisibility.
    /// Dipanggil oleh Penjaga Kedai sebelum Chapter 3.
    /// </summary>
    public void AcquireCloak()
    {
        if (hasCloakOfInvisibility)
        {
            Log("[InventoryManager] AcquireCloak dipanggil tapi sudah dimiliki.");
            return;
        }

        hasCloakOfInvisibility = true;
        Log("[InventoryManager] Cloak of Invisibility diperoleh.");
        OnCloakAcquired?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Debug / Reset
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Reset semua state inventory. Digunakan saat new game atau testing.
    /// </summary>
    public void ResetInventory()
    {
        tokenCount = 0;
        hasFinalKey = false;
        hasCloakOfInvisibility = false;

        OnTokenCountChanged?.Invoke(tokenCount);
        Log("[InventoryManager] Inventory direset.");
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log(message, this);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Context Menu Testing
    // ─────────────────────────────────────────────────────────────────────────

    [ContextMenu("Debug — Add 5 Tokens")]
    private void DebugAdd5Tokens() => AddTokens(5);

    [ContextMenu("Debug — Spend 3 Tokens")]
    private void DebugSpend3Tokens() => SpendTokens(3);

    [ContextMenu("Debug — Acquire Final Key")]
    private void DebugAcquireFinalKey() => AcquireFinalKey();

    [ContextMenu("Debug — Acquire Cloak")]
    private void DebugAcquireCloak() => AcquireCloak();

    [ContextMenu("Debug — Reset Inventory")]
    private void DebugResetInventory() => ResetInventory();

    [ContextMenu("Debug — Print State")]
    private void DebugPrintState()
    {
        Debug.Log($"[InventoryManager] TokenCount: {tokenCount} | HasFinalKey: {hasFinalKey} | HasCloak: {hasCloakOfInvisibility}", this);
    }
}
