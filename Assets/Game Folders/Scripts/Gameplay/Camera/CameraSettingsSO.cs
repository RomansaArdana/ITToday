using UnityEngine;

/// <summary>
/// ScriptableObject berisi konfigurasi kamera untuk setiap zona/environment.
/// Buat asset baru lewat: klik kanan di Project → The Day After → Camera → Camera Settings
/// </summary>
[CreateAssetMenu(fileName = "CameraSettings_New", menuName = "The Day After/Camera/Camera Settings")]
public class CameraSettingsSO : ScriptableObject
{
    [Header("Orthographic Size (Zoom)")]
    [Tooltip("Ukuran ortografis kamera. Lebih besar = lebih jauh / lebih kecil = lebih dekat.\nNilai normal: 4–6. Boss arena: 7–8. Lorong sempit: 3–4.")]
    [SerializeField] private float orthographicSize = 5f;

    [Tooltip(
        "Batas TERDEKAT zoom kamera (nilai terkecil orthographicSize yang diizinkan).\n" +
        "Kamera tidak akan bisa zoom-in melampaui nilai ini.\n" +
        "Contoh: 3 = tidak akan lebih dekat dari orthoSize 3."
    )]
    [SerializeField, Min(0.5f)] private float minOrthographicSize = 3f;

    [Tooltip(
        "Batas TERJAUH zoom kamera (nilai terbesar orthographicSize yang diizinkan).\n" +
        "Kamera tidak akan bisa zoom-out melampaui nilai ini.\n" +
        "Contoh: 9 = tidak akan lebih jauh dari orthoSize 9."
    )]
    [SerializeField, Min(0.5f)] private float maxOrthographicSize = 9f;

    [Header("Dynamic Zoom saat Berlari")]
    [Tooltip(
        "Aktifkan zoom-out otomatis saat Inara berlari?\n" +
        "Efek klasik side-scroller: kamera 'mundur' agar pemain bisa lihat lebih jauh ke depan."
    )]
    [SerializeField] private bool enableDynamicZoomOnRun = false;

    [Tooltip(
        "Seberapa jauh zoom-out saat berlari (tambahan ke orthographicSize).\n" +
        "Contoh: 0.8 = orthoSize bertambah 0.8 saat sprint, dikembalikan saat berhenti."
    )]
    [SerializeField, Range(0f, 3f)] private float runZoomOutAmount = 0.8f;

    [Tooltip("Kecepatan transisi zoom in↔out saat mulai/berhenti berlari.")]
    [SerializeField, Range(0.5f, 8f)] private float dynamicZoomSpeed = 3f;

    [Header("Follow Smoothness")]
    [Tooltip("Waktu smooth damp untuk follow horizontal. Lebih besar = lebih lambat mengikuti.")]
    [SerializeField, Range(0.05f, 1f)] private float horizontalSmoothTime = 0.15f;

    [Tooltip("Waktu smooth damp untuk follow vertikal.")]
    [SerializeField, Range(0.05f, 1f)] private float verticalSmoothTime = 0.2f;

    [Header("Look-Ahead")]
    [Tooltip("Seberapa jauh kamera melihat ke arah hadap Inara saat berjalan.")]
    [SerializeField, Range(0f, 5f)] private float lookAheadDistance = 2f;

    [Tooltip("Seberapa jauh kamera melihat ke arah hadap Inara saat berlari.")]
    [SerializeField, Range(0f, 8f)] private float runLookAheadDistance = 4f;

    [Tooltip("Kecepatan lerp look-ahead berpindah arah.")]
    [SerializeField, Range(0.5f, 10f)] private float lookAheadSpeed = 3f;

    [Header("Deadzone (area di mana kamera tidak bergerak)")]
    [Tooltip("Deadzone horizontal sebelum kamera mulai bergerak.")]
    [SerializeField, Range(0f, 2f)] private float horizontalDeadzone = 0.1f;

    [Tooltip("Deadzone vertikal. Lebih besar = lebih stabil saat di tanah.")]
    [SerializeField, Range(0f, 3f)] private float verticalDeadzone = 0.5f;

    [Header("Camera Offset")]
    [Tooltip("Offset tambahan kamera dari posisi player (posisi normal/berdiri).")]
    [SerializeField] private Vector2 baseOffset = new Vector2(0f, 0.5f);

    [Tooltip("Offset kamera saat Inara sedang crouch (biasanya negatif Y agar melihat bawah).")]
    [SerializeField] private Vector2 crouchOffset = new Vector2(0f, -0.8f);

    [Tooltip("Kecepatan transisi offset saat masuk/keluar crouch.")]
    [SerializeField, Range(1f, 15f)] private float crouchOffsetSpeed = 6f;

    [Header("Camera Bounds")]
    [Tooltip("Apakah kamera dibatasi dalam zona ini?")]
    [SerializeField] private bool useBounds = false;

    [Tooltip("Batas posisi kamera (world space). X = kiri, Y = kanan, Z = bawah, W = atas.")]
    [SerializeField] private Vector4 cameraBounds = new Vector4(-20f, 20f, -10f, 10f);

    // ─── Properties ───────────────────────────────────────────────────────

    public float OrthographicSize        => orthographicSize;
    public float MinOrthographicSize     => minOrthographicSize;
    public float MaxOrthographicSize     => maxOrthographicSize;
    public bool  EnableDynamicZoomOnRun  => enableDynamicZoomOnRun;
    public float RunZoomOutAmount        => runZoomOutAmount;
    public float DynamicZoomSpeed        => dynamicZoomSpeed;
    public float HorizontalSmoothTime    => horizontalSmoothTime;
    public float VerticalSmoothTime      => verticalSmoothTime;
    public float LookAheadDistance       => lookAheadDistance;
    public float RunLookAheadDistance    => runLookAheadDistance;
    public float LookAheadSpeed          => lookAheadSpeed;
    public float HorizontalDeadzone      => horizontalDeadzone;
    public float VerticalDeadzone        => verticalDeadzone;
    public Vector2 BaseOffset            => baseOffset;
    public Vector2 CrouchOffset          => crouchOffset;
    public float CrouchOffsetSpeed       => crouchOffsetSpeed;
    public bool UseBounds                => useBounds;

    // Batas kamera: MinX, MaxX, MinY, MaxY
    public float BoundsMinX => cameraBounds.x;
    public float BoundsMaxX => cameraBounds.y;
    public float BoundsMinY => cameraBounds.z;
    public float BoundsMaxY => cameraBounds.w;

#if UNITY_EDITOR
    /// <summary>
    /// Validasi nilai min/max di Editor agar tidak salah isi.
    /// </summary>
    private void OnValidate()
    {
        // orthographicSize harus dalam range [min, max]
        orthographicSize = Mathf.Clamp(orthographicSize, minOrthographicSize, maxOrthographicSize);

        // min tidak boleh lebih besar dari max
        if (minOrthographicSize > maxOrthographicSize)
            minOrthographicSize = maxOrthographicSize;
    }
#endif
}
