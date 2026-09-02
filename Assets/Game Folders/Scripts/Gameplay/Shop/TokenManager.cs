using System;
using UnityEngine;

/// <summary>
/// Single source of truth untuk currency (Token) pemain.
///
/// Prinsip:
///   Hanya TokenManager yang boleh menambah/mengurangi Token.
///   Sistem lain (UpgradeManager, ShopUI) membaca CurrentTokens atau memanggil TrySpend.
///   Tidak ada setter publik — semua perubahan melalui method yang tervalidasi.
///
/// Flow:
///   EnemyReward      → TokenManager.AddTokens()
///   UpgradeManager   → TokenManager.TrySpend()
///   ShopUI           → TokenManager.CurrentTokens (read only)
/// </summary>
public class TokenManager : MonoBehaviour
{
    public static TokenManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    // ─────────────────────────────────────────────────────────────────────────
    // State
    // ─────────────────────────────────────────────────────────────────────────

    private int currentTokens;

    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Jumlah Token yang saat ini dimiliki pemain.</summary>
    public int CurrentTokens => currentTokens;

    // ─────────────────────────────────────────────────────────────────────────
    // Events
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Dipanggil setiap kali CurrentTokens berubah (baik bertambah maupun berkurang).
    /// Parameter: nilai baru CurrentTokens.
    /// ShopUI dan HUD subscribe ke event ini untuk update tampilan.
    /// </summary>
    public event Action<int> OnTokensChanged;

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

        Log("[TokenManager] Initialized.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tambahkan Token. Dipanggil oleh EnemyReward atau sistem reward lainnya.
    /// </summary>
    public void AddTokens(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[TokenManager] AddTokens dipanggil dengan amount tidak valid: {amount}", this);
            return;
        }

        int previous = currentTokens;
        currentTokens += amount;

        Log($"[TokenManager] +{amount} | {previous} → {currentTokens}");
        OnTokensChanged?.Invoke(currentTokens);
    }

    /// <summary>
    /// Cek apakah saldo mencukupi tanpa mengurangi. Digunakan ShopUI untuk
    /// menentukan apakah tombol Buy aktif atau nonaktif.
    /// </summary>
    public bool HasEnough(int amount) => currentTokens >= amount;

    /// <summary>
    /// Kurangi Token jika saldo mencukupi.
    /// </summary>
    /// <returns>True jika berhasil. False jika saldo tidak cukup — Token tidak berubah.</returns>
    public bool TrySpend(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[TokenManager] TrySpend dipanggil dengan amount tidak valid: {amount}", this);
            return false;
        }

        if (currentTokens < amount)
        {
            Log($"[TokenManager] TrySpend gagal — Dimiliki: {currentTokens}, Dibutuhkan: {amount}");
            return false;
        }

        int previous = currentTokens;
        currentTokens -= amount;

        Log($"[TokenManager] -{amount} | {previous} → {currentTokens}");
        OnTokensChanged?.Invoke(currentTokens);
        return true;
    }

    /// <summary>
    /// Reset Token ke 0. Digunakan untuk New Game atau testing.
    /// </summary>
    public void ResetTokens()
    {
        currentTokens = 0;
        OnTokensChanged?.Invoke(currentTokens);
        Log("[TokenManager] Reset.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Debug Context Menu
    // ─────────────────────────────────────────────────────────────────────────

    [ContextMenu("Debug — Add 5 Tokens")]
    private void DebugAdd5() => AddTokens(5);

    [ContextMenu("Debug — Add 20 Tokens")]
    private void DebugAdd20() => AddTokens(20);

    [ContextMenu("Debug — Spend 3 Tokens")]
    private void DebugSpend3() => TrySpend(3);

    [ContextMenu("Debug — Reset")]
    private void DebugReset() => ResetTokens();

    [ContextMenu("Debug — Print State")]
    private void DebugPrint() => Debug.Log($"[TokenManager] CurrentTokens: {currentTokens}", this);

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private void Log(string msg)
    {
        if (enableDebugLog) Debug.Log(msg, this);
    }
}
