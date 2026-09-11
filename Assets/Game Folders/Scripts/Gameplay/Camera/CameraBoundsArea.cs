using UnityEngine;

/// <summary>
/// CameraBoundsArea — Mendefinisikan area batas kamera secara VISUAL di Scene View.
///
/// MENDUKUNG DUA MODE:
///
/// ─── MODE A: Drag & Drop Collider Existing (DIREKOMENDASIKAN untuk setup kamu) ───
///   Cocok jika sudah punya PolygonCollider2D / BoxCollider2D di background/tilemap.
///   1. Assign field "Bounds Source" di Inspector → drag PolygonCollider2D background
///   2. Kamera otomatis clamp ke AABB (bounding box) dari collider tersebut
///   3. Masih butuh trigger collider di GameObject INI untuk deteksi masuk/keluar area
///
/// ─── MODE B: Self (BoxCollider2D di GameObject yang sama) ───
///   Tidak assign "Bounds Source" → script pakai BoxCollider2D di GameObject ini.
///   Cocok untuk buat area bounds manual dengan resize di scene view.
///
/// PRIORITY SYSTEM (di PlayerCameraController):
///   CameraBoundsArea aktif → override bounds dari ScriptableObject
///   Tidak ada area aktif   → fallback ke CameraSettingsSO (jika UseBounds = true)
///
/// WORKFLOW UNTUK SETUP KAMU:
///   1. Buat Empty GameObject → beri nama "CamBounds_[NamaLevel]"
///   2. Add Component: Collider2D (BoxCollider2D besar yang jadi trigger, mencakup seluruh area)
///      → centang Is Trigger = true
///   3. Add Component: CameraBoundsArea
///   4. Di field "Bounds Source" → drag PolygonCollider2D dari background/tilemap
///   5. Done! Kamera tahu batas dari polygon collider background.
/// </summary>
public class CameraBoundsArea : MonoBehaviour
{
    [Header("Identification")]
    [Tooltip("Nama area ini, untuk debugging di Editor.")]
    [SerializeField] private string areaName = "BoundsArea";

    [Header("Bounds Source")]
    [Tooltip(
        "MODE A — Drag & Drop: Assign collider dari GameObject lain (misal: PolygonCollider2D background/tilemap).\n" +
        "Kamera akan clamp ke AABB (bounding box) dari collider tersebut.\n\n" +
        "MODE B — Biarkan kosong: Script akan pakai BoxCollider2D di GameObject ini sendiri.\n" +
        "Resize BoxCollider2D di scene view untuk mengatur batas."
    )]
    [SerializeField] private Collider2D boundsSource;

    [Header("Trigger Detection")]
    [Tooltip("Tag player untuk deteksi masuk/keluar area.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Transition")]
    [Tooltip("Durasi transisi smooth saat bounds area ini mulai aktif.")]
    [SerializeField, Range(0.05f, 2f)] private float activationDuration = 0.4f;

    [Header("Debug Visuals")]
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0.5f, 0.12f);
    [SerializeField] private Color gizmoOutlineColor = new Color(0f, 1f, 0.5f, 0.9f);
    [SerializeField] private bool showLabel = true;

    // ─── Internal ──────────────────────────────────────────────────────────
    private Collider2D selfCollider;          // Trigger collider di GameObject ini (untuk deteksi)
    private static CameraBoundsArea currentActive;

    // ─── Properties ────────────────────────────────────────────────────────

    /// <summary>
    /// AABB (Axis-Aligned Bounding Box) dari bounds area dalam world space.
    ///
    /// Prioritas:
    ///   1. boundsSource.bounds jika boundsSource di-assign (MODE A)
    ///   2. selfCollider.bounds jika ada BoxCollider2D di GameObject ini (MODE B)
    ///   3. Bounds kosong di posisi transform sebagai fallback terakhir
    /// </summary>
    public Bounds WorldBounds
    {
        get
        {
            // MODE A: Pakai collider external (misal PolygonCollider2D background)
            if (boundsSource != null)
                return boundsSource.bounds;

            // MODE B: Pakai collider di GameObject ini
            if (selfCollider != null)
                return selfCollider.bounds;

            // Fallback: area kosong di posisi transform
            Debug.LogWarning(
                $"[CameraBoundsArea] '{areaName}': Tidak ada Bounds Source maupun collider di GameObject ini! " +
                "Assign Bounds Source di Inspector, atau tambahkan BoxCollider2D.",
                this
            );
            return new Bounds(transform.position, Vector3.zero);
        }
    }

    public float ActivationDuration   => activationDuration;
    public string AreaName            => areaName;

    /// <summary>Bounds area yang sedang aktif (player ada di dalamnya).</summary>
    public static CameraBoundsArea Active => currentActive;

    // ─── Lifecycle ─────────────────────────────────────────────────────────

    private void Awake()
    {
        // Cari trigger collider di GameObject ini untuk deteksi masuk/keluar
        selfCollider = GetComponent<Collider2D>();

        if (selfCollider == null && boundsSource == null)
        {
            Debug.LogError(
                $"[CameraBoundsArea] '{areaName}': Setup belum lengkap!\n" +
                "Butuh salah satu:\n" +
                "  • Assign 'Bounds Source' di Inspector (drag PolygonCollider2D background), ATAU\n" +
                "  • Tambahkan BoxCollider2D ke GameObject ini.\n" +
                "Untuk trigger detection, tambahkan BoxCollider2D (Is Trigger = true) ke GameObject ini.",
                this
            );
            return;
        }

        // Pastikan selfCollider adalah trigger (untuk Enter/Exit detection)
        if (selfCollider != null && !selfCollider.isTrigger)
        {
            selfCollider.isTrigger = true;
            Debug.LogWarning(
                $"[CameraBoundsArea] '{areaName}': Collider di-set ke Is Trigger = true secara otomatis.",
                this
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        currentActive = this;
        PlayerCameraController.Instance?.SetBoundsArea(this);

#if UNITY_EDITOR
        string sourceInfo = boundsSource != null
            ? $"Source: {boundsSource.name} ({boundsSource.GetType().Name})"
            : "Source: Self collider";
        Debug.Log($"[CamBounds] Entered: '{areaName}' | {sourceInfo} | Bounds: {WorldBounds}");
#endif
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (currentActive != this) return;

        currentActive = null;
        PlayerCameraController.Instance?.ClearBoundsArea();

#if UNITY_EDITOR
        Debug.Log($"[CamBounds] Exited: '{areaName}'");
#endif
    }

    private void OnDisable()
    {
        if (currentActive == this)
        {
            currentActive = null;
            PlayerCameraController.Instance?.ClearBoundsArea();
        }
    }

    // ─── Editor Utility ────────────────────────────────────────────────────

    /// <summary>
    /// Validasi setup di editor tanpa perlu play.
    /// Akan muncul warning di Inspector jika ada masalah konfigurasi.
    /// </summary>
    private void OnValidate()
    {
#if UNITY_EDITOR
        // Cek apakah ada trigger collider untuk deteksi
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            // OK jika ada boundsSource, tapi tetap perlu trigger collider di GameObject ini
            Debug.LogWarning(
                $"[CameraBoundsArea] '{areaName}': Tidak ada Collider2D di GameObject ini.\n" +
                "Tambahkan BoxCollider2D (Is Trigger = true) sebesar area yang ingin di-detect.",
                this
            );
        }
#endif
    }

    // ─── Gizmos ────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        // Gambar batas dari WorldBounds (AABB dari boundsSource atau selfCollider)
        DrawBoundsGizmo(false);
    }

    private void OnDrawGizmosSelected()
    {
        DrawBoundsGizmo(true);

#if UNITY_EDITOR
        if (!showLabel) return;

        Bounds b = GetPreviewBounds();
        bool isActive = Application.isPlaying && currentActive == this;

        string modeStr  = boundsSource != null
            ? $"src: {boundsSource.name}"
            : "src: self";

        string label = isActive
            ? $"📷 ACTIVE\n{areaName}\n({b.size.x:F1} × {b.size.y:F1})\n{modeStr}"
            : $"📷 {areaName}\n({b.size.x:F1} × {b.size.y:F1})\n{modeStr}";

        GUIStyle style = new GUIStyle();
        style.normal.textColor = isActive ? Color.white : gizmoOutlineColor;
        style.fontSize         = 10;
        style.fontStyle        = isActive ? FontStyle.Bold : FontStyle.Normal;
        style.alignment        = TextAnchor.MiddleCenter;

        UnityEditor.Handles.Label(
            b.center + Vector3.up * (b.extents.y + 0.5f),
            label,
            style
        );
#endif
    }

    private void DrawBoundsGizmo(bool selected)
    {
        Bounds b = GetPreviewBounds();
        bool isActive = Application.isPlaying && currentActive == this;

        // Fill
        Gizmos.color = isActive
            ? new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, gizmoColor.a * 3f)
            : gizmoColor;
        Gizmos.DrawCube(b.center, b.size);

        // Outline
        Gizmos.color = isActive ? Color.white : gizmoOutlineColor;
        Gizmos.DrawWireCube(b.center, b.size);

        if (!selected) return;

        // Saat dipilih: gambar tanda silang di sudut
        float crossSize = Mathf.Max(b.size.x, b.size.y) * 0.02f;
        DrawCornerCross(new Vector3(b.min.x, b.min.y, 0f), crossSize);
        DrawCornerCross(new Vector3(b.max.x, b.min.y, 0f), crossSize);
        DrawCornerCross(new Vector3(b.min.x, b.max.y, 0f), crossSize);
        DrawCornerCross(new Vector3(b.max.x, b.max.y, 0f), crossSize);
    }

    /// <summary>
    /// Preview bounds untuk Gizmos — bekerja saat Play maupun Edit mode.
    /// </summary>
    private Bounds GetPreviewBounds()
    {
        // Saat Edit mode, Application.isPlaying = false
        // Jadi kita harus langsung ambil dari boundsSource / selfCollider

        if (boundsSource != null)
            return boundsSource.bounds;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            return col.bounds;

        // Fallback: tampilkan dot kecil di posisi transform
        return new Bounds(transform.position, Vector3.one * 0.5f);
    }

    private void DrawCornerCross(Vector3 pos, float size)
    {
        float half = size * 0.5f;
        Gizmos.DrawLine(pos + Vector3.left * half, pos + Vector3.right * half);
        Gizmos.DrawLine(pos + Vector3.down * half, pos + Vector3.up * half);
    }
}
