using System;
using UnityEngine;

/// <summary>
/// Pasang script ini di GameObject lampu (bersamaan dengan FlickeringLight).
/// Cast ray dari lampu ke tulisan "KENAPA?".
/// Jika meja menghalangi ray → mulai hitung durasi → fire event OnShadowConfirmed.
///
/// Setup:
/// 1. Buat Layer baru: "PuzzleObstacle" → assign ke meja
/// 2. Buat Layer baru: "PuzzleTarget"   → assign ke collider di tulisan "KENAPA?"
/// 3. Assign wallTextTransform ke Transform tulisan "KENAPA?"
/// 4. Atur obstacleLayer dan targetLayer di Inspector
/// 5. Atur confirmDuration (berapa detik shadow harus stabil sebelum trigger)
/// </summary>
public class ShadowRaycastChecker : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform posisi tulisan 'KENAPA?' di dinding")]
    [SerializeField] private Transform wallTextTransform;

    [Header("Layer Settings")]
    [Tooltip("Layer yang dipakai meja (PuzzleObstacle)")]
    [SerializeField] private LayerMask obstacleLayer;
    [Tooltip("Layer yang dipakai collider tulisan KENAPA? (PuzzleTarget)")]
    [SerializeField] private LayerMask targetLayer;

    [Header("Puzzle Settings")]
    [Tooltip("Berapa detik bayangan harus stabil sebelum puzzle selesai")]
    [SerializeField] private float confirmDuration = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    // ── Events ──────────────────────────────────────────────────────────────
    /// <summary>Dipanggil saat bayangan berhasil menutupi tulisan selama confirmDuration.</summary>
    public event Action OnShadowConfirmed;
    /// <summary>Dipanggil setiap frame — true jika bayangan sedang aktif.</summary>
    public event Action<bool> OnShadowStateChanged;

    // ── State ────────────────────────────────────────────────────────────────
    private bool isShadowActive = false;
    private bool puzzleSolved = false;
    private float shadowTimer = 0f;

    // Untuk gizmo drawing
    private bool lastHitObstacle = false;
    private Vector2 lastRayEnd;

    public bool IsShadowActive => isShadowActive;
    public bool IsPuzzleSolved => puzzleSolved;
    public float ShadowProgress => confirmDuration > 0 ? shadowTimer / confirmDuration : 0f;

    private void Update()
    {
        if (puzzleSolved || wallTextTransform == null)
            return;

        CheckShadow();
    }

    private void CheckShadow()
    {
        Vector2 origin = transform.position;
        Vector2 target = wallTextTransform.position;
        Vector2 direction = (target - origin).normalized;
        float distance = Vector2.Distance(origin, target);

        // LayerMask gabungan: cek obstacle DAN target
        LayerMask combinedMask = obstacleLayer | targetLayer;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, combinedMask);

        bool shadowDetected = false;

        if (hit.collider != null)
        {
            // Jika ray mengenai obstacle (meja) terlebih dahulu → bayangan aktif
            int hitLayer = hit.collider.gameObject.layer;
            shadowDetected = ((1 << hitLayer) & obstacleLayer) != 0;
            lastRayEnd = hit.point;
        }
        else
        {
            // Ray tidak mengenai apapun
            lastRayEnd = target;
        }

        lastHitObstacle = shadowDetected;

        // Deteksi perubahan state
        if (shadowDetected != isShadowActive)
        {
            isShadowActive = shadowDetected;
            OnShadowStateChanged?.Invoke(isShadowActive);

            if (!isShadowActive)
                shadowTimer = 0f; // Reset timer jika shadow hilang
        }

        // Hitung timer konfirmasi
        if (isShadowActive)
        {
            shadowTimer += Time.deltaTime;

            if (shadowTimer >= confirmDuration)
            {
                ConfirmPuzzle();
            }
        }
    }

    private void ConfirmPuzzle()
    {
        if (puzzleSolved)
            return;

        puzzleSolved = true;
        Debug.Log("[ShadowRaycastChecker] Puzzle selesai! Bayangan menutupi tulisan.");
        OnShadowConfirmed?.Invoke();
    }

    /// <summary>Reset puzzle ke kondisi awal (misal setelah checkpoint)</summary>
    public void ResetPuzzle()
    {
        puzzleSolved = false;
        isShadowActive = false;
        shadowTimer = 0f;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || wallTextTransform == null)
            return;

        // Gambar ray di Scene view
        // Hijau = shadow aktif (meja menghalangi), Merah = tidak terblokir
        Gizmos.color = lastHitObstacle ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, lastRayEnd);

        // Titik asal (lampu)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.15f);

        // Titik target (tulisan)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallTextTransform.position, 0.2f);

        // Progress bar shadow timer (tampil di dekat lampu)
        if (Application.isPlaying && isShadowActive && !puzzleSolved)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                (Vector2)transform.position + Vector2.down * 0.3f,
                (Vector2)transform.position + Vector2.down * 0.3f + Vector2.right * ShadowProgress
            );
        }
    }
}
