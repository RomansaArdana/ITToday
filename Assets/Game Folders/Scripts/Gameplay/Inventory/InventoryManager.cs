using System;
using UnityEngine;

/// <summary>
/// Single source of truth untuk item ownership pemain.
/// Token/currency dipindahkan ke TokenManager — InventoryManager hanya menangani item.
///
/// Architecture:
///   TokenManager     → currency (Token)
///   InventoryManager → item ownership (Final Key, Cloak, dll)
///
/// Sistem lain (Gate, Memory, Kedai) harus membaca dari sini.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    // ─────────────────────────────────────────────────────────────────────────
    // State
    // ─────────────────────────────────────────────────────────────────────────

    private bool hasFinalKey;
    private bool hasCloakOfInvisibility;

    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>True jika pemain sudah memiliki Final Key dari Penjaga Kedai.</summary>
    public bool HasFinalKey => hasFinalKey;

    /// <summary>True jika pemain sudah memiliki Cloak of Invisibility.</summary>
    public bool HasCloakOfInvisibility => hasCloakOfInvisibility;

    // ─────────────────────────────────────────────────────────────────────────
    // Events
    // ─────────────────────────────────────────────────────────────────────────

    public event Action OnFinalKeyAcquired;
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
            Log("[InventoryManager] AcquireFinalKey — sudah dimiliki.");
            return;
        }

        hasFinalKey = true;
        Log("[InventoryManager] Final Key diperoleh.");
        OnFinalKeyAcquired?.Invoke();
    }

    /// <summary>
    /// Tandai pemain sebagai pemilik Cloak of Invisibility.
    /// Dipanggil oleh ShopkeeperInteractable atau UpgradeManager saat Cloak dibeli.
    /// </summary>
    public void AcquireCloak()
    {
        if (hasCloakOfInvisibility)
        {
            Log("[InventoryManager] AcquireCloak — sudah dimiliki.");
            return;
        }

        hasCloakOfInvisibility = true;
        Log("[InventoryManager] Cloak of Invisibility diperoleh.");
        OnCloakAcquired?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Reset
    // ─────────────────────────────────────────────────────────────────────────

    public void ResetInventory()
    {
        hasFinalKey = false;
        hasCloakOfInvisibility = false;
        Log("[InventoryManager] Inventory direset.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Debug
    // ─────────────────────────────────────────────────────────────────────────

    private void Log(string msg)
    {
        if (enableDebugLog) Debug.Log(msg, this);
    }

    [ContextMenu("Debug — Acquire Final Key")]
    private void DebugFinalKey() => AcquireFinalKey();

    [ContextMenu("Debug — Acquire Cloak")]
    private void DebugCloak() => AcquireCloak();

    [ContextMenu("Debug — Reset Inventory")]
    private void DebugReset() => ResetInventory();

    [ContextMenu("Debug — Print State")]
    private void DebugPrint() =>
        Debug.Log($"[InventoryManager] HasFinalKey: {hasFinalKey} | HasCloak: {hasCloakOfInvisibility}", this);
}
