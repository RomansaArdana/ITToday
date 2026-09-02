using UnityEngine;

/// <summary>
/// Menangani logika crouch Player: collider resize, offset, ceiling check, dan standing validation.
///
/// Tanggung jawab:
/// - Membaca CrouchHeld dari input
/// - Mengubah ukuran dan offset CapsuleCollider2D saat crouch
/// - Melakukan ceiling check sebelum mengizinkan berdiri
/// - Mengekspose IsCrouching sebagai source of truth untuk system lain
///
/// TIDAK bertanggung jawab atas:
/// - Horizontal movement (PlayerMovement)
/// - Jump (PlayerJump)
/// - Ground detection (PlayerGroundCheck)
/// - Stealth multiplier (PlayerStealth)
/// - Hide spot (PlayerStealth)
/// </summary>
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerCrouch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStateController stateController;

    [Header("Collider — Standing")]
    [Tooltip("Tinggi collider saat berdiri normal.")]
    [SerializeField] private float standingHeight = 1.6f;
    [Tooltip("Offset Y collider saat berdiri normal (dari pivot/kaki karakter).")]
    [SerializeField] private float standingOffsetY = 0.8f;

    [Header("Collider — Crouching")]
    [Tooltip("Tinggi collider saat crouch.")]
    [SerializeField] private float crouchHeight = 0.9f;
    [Tooltip("Offset Y collider saat crouch. Harus lebih kecil dari standingOffsetY agar collider tidak melayang.")]
    [SerializeField] private float crouchOffsetY = 0.45f;

    [Header("Ceiling Check")]
    [Tooltip("Layer yang dianggap sebagai langit-langit (ceiling). Gunakan layer yang sama dengan ground.")]
    [SerializeField] private LayerMask ceilingLayer = ~0;
    [Tooltip("Jarak tambahan di atas collider untuk mendeteksi ceiling sebelum berdiri.")]
    [SerializeField] private float ceilingCheckMargin = 0.05f;
    [Tooltip("Lebar raycast ceiling check relatif terhadap lebar collider (0.5–1.0).")]
    [SerializeField, Range(0.5f, 1f)] private float ceilingCheckWidth = 0.8f;

    [Header("Debug")]
    [SerializeField] private bool showGizmo = true;

    private CapsuleCollider2D capsule;

    /// <summary>
    /// True jika collider sedang dalam mode crouch dan PlayerState == Crouch.
    /// Ini adalah ground truth yang dibaca oleh PlayerStealth dan sistem lain.
    /// </summary>
    public bool IsCrouching { get; private set; }

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider2D>();

        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (stateController == null) stateController = GetComponent<PlayerStateController>();

        // Terapkan ukuran standing sebagai baseline di awal
        ApplyStandingCollider();
    }

    private void Update()
    {
        if (inputReader == null || stateController == null) return;

        bool wantsCrouch = inputReader.CrouchHeld;

        if (wantsCrouch)
        {
            TryEnterCrouch();
        }
        else
        {
            TryExitCrouch();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Crouch Entry
    // ─────────────────────────────────────────────────────────────────────────

    private void TryEnterCrouch()
    {
        if (IsCrouching) return;
        if (!stateController.CanCrouch()) return;

        ApplyCrouchCollider();
        IsCrouching = true;
        stateController.EnterCrouch();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Crouch Exit
    // ─────────────────────────────────────────────────────────────────────────

    private void TryExitCrouch()
    {
        if (!IsCrouching) return;

        if (!CanStandUp()) return;

        ApplyStandingCollider();
        IsCrouching = false;
        stateController.ExitCrouch();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Ceiling Check
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Memeriksa apakah ada obstacle di atas yang mencegah pemain berdiri.
    /// Menggunakan BoxCast dari posisi top collider saat ini ke tinggi berdiri.
    /// </summary>
    private bool CanStandUp()
    {
        if (capsule == null) return true;

        // Hitung berapa ruang yang dibutuhkan untuk berdiri mulai dari top collider saat ini
        float currentTop = capsule.bounds.max.y;
        float neededClearance = (standingHeight - crouchHeight) + ceilingCheckMargin;

        Vector2 checkOrigin = new Vector2(capsule.bounds.center.x, currentTop);
        Vector2 checkSize = new Vector2(capsule.bounds.size.x * ceilingCheckWidth, 0.05f);

        LayerMask mask = ceilingLayer.value == 0 ? ~0 : ceilingLayer;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(checkOrigin, checkSize, 0f, Vector2.up, neededClearance, mask);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.isTrigger) continue;
            if (hit.collider.transform.root == transform.root) continue;

            // Ada obstacle → tidak bisa berdiri
            return false;
        }

        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Collider Manipulation
    // ─────────────────────────────────────────────────────────────────────────

    private void ApplyCrouchCollider()
    {
        if (capsule == null) return;

        capsule.size = new Vector2(capsule.size.x, crouchHeight);
        capsule.offset = new Vector2(capsule.offset.x, crouchOffsetY);
    }

    private void ApplyStandingCollider()
    {
        if (capsule == null) return;

        capsule.size = new Vector2(capsule.size.x, standingHeight);
        capsule.offset = new Vector2(capsule.offset.x, standingOffsetY);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Gizmo
    // ─────────────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        // Visualisasi collider standing (cyan) dan crouch (yellow)
        Vector3 pivot = transform.position;

        // Standing collider (cyan)
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Vector3 standCenter = pivot + new Vector3(0f, standingOffsetY, 0f);
        Gizmos.DrawWireCube(standCenter, new Vector3(0.5f, standingHeight, 0f));

        // Crouch collider (yellow)
        Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
        Vector3 crouchCenter = pivot + new Vector3(0f, crouchOffsetY, 0f);
        Gizmos.DrawWireCube(crouchCenter, new Vector3(0.5f, crouchHeight, 0f));

        // Ceiling check area saat crouch aktif (merah)
        if (Application.isPlaying && IsCrouching && capsule != null)
        {
            float neededClearance = (standingHeight - crouchHeight) + ceilingCheckMargin;
            float currentTop = capsule.bounds.max.y;
            Gizmos.color = CanStandUp() ? Color.green : Color.red;
            Vector3 ceilingCheckCenter = new Vector3(capsule.bounds.center.x, currentTop + neededClearance * 0.5f, 0f);
            Gizmos.DrawWireCube(ceilingCheckCenter, new Vector3(capsule.bounds.size.x * ceilingCheckWidth, neededClearance, 0f));
        }
    }
}
