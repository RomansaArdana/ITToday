using System.Collections;
using UnityEngine;

/// <summary>
/// Mengatur pola serangan AoE Sweep dari bos.
/// Serangan berupa OverlapBox yang menyapu area depan bos,
/// mengenai Cover (pilar/dinding) dan Player secara bersamaan.
/// </summary>
public class BossAttackController : MonoBehaviour
{
    [Header("Attack Pattern")]
    [Tooltip("Jumlah ayunan serangan sebelum bos kelelahan.")]
    [SerializeField] private int sweepCount = 2;
    [Tooltip("Jeda (windup) sebelum setiap ayunan menghantam.")]
    [SerializeField] private float attackWindupDuration = 1.2f;
    [Tooltip("Jeda setelah setiap ayunan selesai.")]
    [SerializeField] private float delayBetweenSweeps = 0.8f;

    [Header("AoE Sweep")]
    [Tooltip("Ukuran area sweep (lebar x tinggi). Sesuaikan di Gizmo.")]
    [SerializeField] private Vector2 sweepSize = new Vector2(5f, 2.5f);
    [Tooltip("Offset dari posisi bos ke arah mana sweep diarahkan.")]
    [SerializeField] private Vector2 sweepOffset = new Vector2(3f, 0f);
    [SerializeField] private LayerMask hitLayers; // Layer: Cover + Player

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool showGizmo = true;

    private Transform bossTransform;

    private void Awake()
    {
        bossTransform = transform;
    }

    // ─────────────────────────────────────────
    // Attack Coroutine (dipanggil oleh BossController)
    // ─────────────────────────────────────────

    /// <summary>
    /// Coroutine utama yang menjalankan siklus serangan bos.
    /// BossController menunggu coroutine ini selesai sebelum pindah ke Exhausted.
    /// </summary>
    public IEnumerator PerformAttack()
    {
        for (int i = 0; i < sweepCount; i++)
        {
            Log($"Windup serangan ke-{i + 1}...");
            yield return new WaitForSeconds(attackWindupDuration);

            ExecuteSweep();

            if (i < sweepCount - 1)
                yield return new WaitForSeconds(delayBetweenSweeps);
        }

        Log("Selesai menyerang.");
    }

    // ─────────────────────────────────────────
    // Sweep Logic
    // ─────────────────────────────────────────

    private void ExecuteSweep()
    {
        // Hitung arah sweep berdasarkan facing bos
        float facingDir = bossTransform.localScale.x >= 0 ? 1f : -1f;
        Vector2 origin = (Vector2)bossTransform.position + new Vector2(sweepOffset.x * facingDir, sweepOffset.y);

        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, sweepSize, 0f, hitLayers);

        Log($"AoE Sweep — Origin={origin} | Mengenai {hits.Length} collider(s)");

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            // Cek apakah mengenai Cover
            BossCover cover = hit.GetComponentInParent<BossCover>();
            if (cover != null)
            {
                Log($"Sweep mengenai Cover: {hit.name}");
                cover.TakeHit();
                continue;
            }

            // Cek apakah mengenai Player
            BossChanceController playerChance = hit.GetComponentInParent<BossChanceController>();
            if (playerChance != null)
            {
                Log($"Sweep mengenai Player!");
                playerChance.TakeHit();
            }
        }
    }

    // ─────────────────────────────────────────
    // Gizmo (untuk kalibrasi ukuran AoE di Editor)
    // ─────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        float facingDir = transform.localScale.x >= 0 ? 1f : -1f;
        Vector2 origin = (Vector2)transform.position + new Vector2(sweepOffset.x * facingDir, sweepOffset.y);
        Gizmos.DrawCube(origin, sweepSize);
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[BossAttack] {message}", this);
    }
}
