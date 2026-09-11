using UnityEngine;

/// <summary>
/// Trigger 2D yang mengubah setting kamera secara otomatis ketika Inara memasuki suatu zona.
/// 
/// Cara pakai:
/// 1. Buat GameObject kosong di scene
/// 2. Tambahkan Collider2D (BoxCollider2D) dan set IsTrigger = true
/// 3. Attach script ini
/// 4. Assign CameraSettingsSO yang sesuai untuk zona ini
/// 
/// Contoh: Buat CameraZoneTrigger di entrance Chapter 1 (Kebencian),
/// assign CameraSettings_Kebencian.asset.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("Nama zona ini (hanya untuk debugging di editor).")]
    [SerializeField] private string zoneName = "ZoneName";

    [Tooltip("Pengaturan kamera yang akan digunakan saat Inara memasuki zona ini.")]
    [SerializeField] private CameraSettingsSO settings;

    [Tooltip("Tag yang digunakan oleh player. Harus sama dengan tag GameObject player.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Transition")]
    [Tooltip("Durasi transisi smooth saat kamera beralih ke setting baru (dalam detik).")]
    [SerializeField, Range(0.1f, 3f)] private float transitionDuration = 0.8f;

    [Tooltip("Apakah zona ini hanya bisa aktif sekali (tidak bisa di-reset saat keluar)?")]
    [SerializeField] private bool oneShot = false;

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (oneShot && activated) return;
        if (settings == null)
        {
            Debug.LogWarning($"[CameraZoneTrigger] Zone '{zoneName}': CameraSettingsSO belum diassign!", this);
            return;
        }

        activated = true;

        PlayerCameraController cam = PlayerCameraController.Instance;
        if (cam != null)
        {
            cam.TransitionToSettings(settings, transitionDuration);

#if UNITY_EDITOR
            Debug.Log($"[Camera] Entering zone: {zoneName} | OrthoSize: {settings.OrthographicSize}");
#endif
        }
    }

    // ─── Gizmos untuk visualisasi di editor ───────────────────────────────
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.2f);
        if (col is BoxCollider2D box)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawCube(box.offset, box.size);
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
            Gizmos.DrawWireCube(box.offset, box.size);
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

    private void OnDrawGizmosSelected()
    {
        // Tampilkan label nama zona
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f,
            $"📷 {zoneName}",
            new GUIStyle { normal = new GUIStyleState { textColor = Color.cyan }, fontSize = 11 }
        );
#endif
    }
}
