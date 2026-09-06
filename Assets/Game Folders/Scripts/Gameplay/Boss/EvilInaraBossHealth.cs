using UnityEngine;

/// <summary>
/// Mengatur HP bos dan penerimaan damage dari Lentera.
/// Guard utama: hanya menerima damage saat bos dalam state Exhausted.
/// Implement IDamageable agar kompatibel dengan sistem Lentera yang sudah ada.
/// </summary>
public class EvilInaraBossHealth : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private EvilInaraBossController bossController;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    // ─── IDamageable ───────────────────────────────────────
    public bool IsAlive => bossController != null && !bossController.IsDefeated;

    private void Awake()
    {
        bossController ??= GetComponent<EvilInaraBossController>();

        if (bossController == null)
            Debug.LogError("[BossHealth] EvilInaraBossController tidak ditemukan!", this);
    }

    /// <summary>
    /// Dipanggil oleh PlayerLanternAttack via OverlapCircleAll saat tombol F ditekan.
    /// Guard clause: HANYA memproses damage jika bos sedang Exhausted.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        // ═══ GUARD UTAMA ═══════════════════════════════════
        // Lentera tidak mempan jika bos tidak sedang kelelahan.
        if (!bossController.IsExhausted)
        {
            Log("Lentera tidak mempan — bos belum kelelahan.");
            return;
        }
        // ═══════════════════════════════════════════════════

        Log($"Bos terkena Lentera! Memurnikan... (amount={amount})");
        bossController.ReceivePurification();
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[BossHealth] {message}", this);
    }
}
