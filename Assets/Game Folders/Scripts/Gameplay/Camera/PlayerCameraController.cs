using UnityEngine;

/// <summary>
/// Camera Controller utama untuk "The Day After" — 2D Psychological Horror Side-Scroller.
///
/// Fitur:
/// - Smooth follow player dengan horizontal & vertical deadzone
/// - Look-ahead ke arah hadap Inara (lebih jauh saat berlari)
/// - Crouch shift: kamera turun saat Inara jongkok (penting untuk puzzle refleksi air Ch.2)
/// - Camera bounds clamping per zona
/// - Orthographic size transition smooth (zoom per area)
/// - Sanity-based visual effect: shake + zoom-in saat sanity kritis
/// - Integrasi dengan CameraShakeController
/// - Integrasi dengan CameraZoneTrigger via TransitionToSettings()
///
/// Setup:
/// 1. Taruh script ini di Camera GameObject (bukan di Player)
/// 2. Assign playerTransform, playerStateController, sanityController
/// 3. Assign defaultSettings (CameraSettingsSO)
/// 4. Tambahkan CameraShakeController ke Camera GameObject yang sama
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlayerCameraController : MonoBehaviour
{
    // ─── Singleton ─────────────────────────────────────────────────────────
    public static PlayerCameraController Instance { get; private set; }

    // ─── Inspector ─────────────────────────────────────────────────────────
    [Header("Target")]
    [Tooltip("Transform player (Inara). Assign di Inspector.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("Referensi ke PlayerStateController untuk membaca state (Crouch, Dead, dll).")]
    [SerializeField] private PlayerStateController playerStateController;

    [Tooltip("Referensi ke SanityController untuk efek visual kritis.")]
    [SerializeField] private SanityController sanityController;

    [Header("Default Settings")]
    [Tooltip("Setting kamera default sebelum zone pertama dimasuki.")]
    [SerializeField] private CameraSettingsSO defaultSettings;

    [Header("Sanity Camera Effects")]
    [Tooltip("Threshold sanity (0-1) di mana efek kamera 'unstable' mulai aktif.")]
    [SerializeField, Range(0f, 1f)] private float sanityUnstableThreshold = 0.7f;

    [Tooltip("Threshold sanity (0-1) di mana efek kamera 'critical' aktif (shake + zoom).")]
    [SerializeField, Range(0f, 1f)] private float sanityCriticalThreshold = 0.3f;

    [Tooltip("Seberapa besar zoom-in (pengurangan orthoSize) saat sanity kritis.")]
    [SerializeField, Range(0f, 1f)] private float criticalZoomAmount = 0.4f;

    [Tooltip("Interval (detik) antara sanity shake saat kritis.")]
    [SerializeField, Range(0.5f, 5f)] private float sanityShakeInterval = 2f;

    [Header("Zoom (Orthographic Size) Transition")]
    [Tooltip("Kecepatan transisi orthographic size antar zona.")]
    [SerializeField, Range(0.5f, 10f)] private float zoomTransitionSpeed = 3f;

    [Header("Zoom Control — Global")]
    [Tooltip(
        "Batas TERDEKAT zoom kamera secara global (override semua SO).\n" +
        "Gunakan ini sebagai safety net agar kamera tidak terlalu dekat.\n" +
        "Biasanya disesuaikan dengan lebar sprite player agar player tidak raksasa di layar."
    )]
    [SerializeField, Min(0.5f)] private float globalMinZoom = 2.5f;

    [Tooltip(
        "Batas TERJAUH zoom kamera secara global (override semua SO).\n" +
        "Gunakan ini agar background tidak terlihat terlalu kecil/kosong."
    )]
    [SerializeField, Min(0.5f)] private float globalMaxZoom = 10f;

    [Tooltip(
        "Offset zoom tambahan yang bisa ditweak secara real-time.\n" +
        "Positif = zoom out (lebih jauh), Negatif = zoom in (lebih dekat).\n" +
        "Berguna untuk fine-tune angle tiap scene tanpa edit SO asset."
    )]
    [SerializeField, Range(-3f, 3f)] private float zoomOffset = 0f;

    [Header("Debug")]
    [SerializeField] private bool showBoundsGizmo = true;

    // Property untuk dibaca dari luar (misal: UI zoom indicator)
    public float CurrentOrthographicSize => currentOrthoSize;
    public float ZoomOffset
    {
        get => zoomOffset;
        set => zoomOffset = Mathf.Clamp(value, -3f, 3f);
    }

    // ─── State Private ─────────────────────────────────────────────────────
    private Camera cam;
    private CameraShakeController shaker;

    private CameraSettingsSO activeSettings;
    private CameraSettingsSO targetSettings;
    private float settingsTransitionProgress = 1f;
    private float settingsTransitionDuration = 1f;

    // Follow state
    private Vector3 currentVelocityH;   // Untuk SmoothDamp horizontal
    private float   currentVelocityV;   // Untuk SmoothDamp vertikal
    private float   currentLookAhead;   // Look-ahead saat ini
    private Vector2 currentOffset;      // Offset aktual (lerp antara base dan crouch)

    // Ortho size
    private float targetOrthoSize;
    private float currentOrthoSize;

    // Dynamic zoom (saat berlari — dari CameraSettingsSO.EnableDynamicZoomOnRun)
    private float currentDynamicZoom;    // Zoom offset yang sedang aktif akibat run
    private float dynamicZoomVelocity;   // Untuk SmoothDamp

    // Sanity
    private float sanityShakeTimer;
    private bool  wasInCriticalSanity;

    // ─── Bounds (CameraBoundsArea) ─────────────────────────────────────────
    // Sistem dua-lapis:
    //   Prioritas 1: activeBoundsArea (CameraBoundsArea di scene — visual, bisa di-resize)
    //   Prioritas 2: UseBounds di CameraSettingsSO (fallback jika tidak ada area aktif)
    private CameraBoundsArea activeBoundsArea;
    private Bounds           currentBounds;          // Bounds yang sedang dipakai
    private Bounds           targetBounds;           // Bounds tujuan (untuk lerp)
    private float            boundsBlend = 1f;       // 0 = dari bounds lama, 1 = bounds baru
    private float            boundsBlendSpeed = 2f;  // Kecepatan lerp antar bounds
    private bool             hasBounds = false;      // Apakah bounds sedang aktif?

    // Posisi kamera (dihitung tanpa shake, shake ditambah terpisah)
    private Vector3 desiredPosition;

    // ─── Lifecycle ─────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cam    = GetComponent<Camera>();
        shaker = GetComponent<CameraShakeController>();

        // Pastikan kamera ortografis
        if (!cam.orthographic)
        {
            Debug.LogWarning("[PlayerCameraController] Kamera bukan ortografis! Game ini 2D side-scroller, ubah ke Orthographic.");
            cam.orthographic = true;
        }

        // Mulai dengan default settings
        if (defaultSettings != null)
        {
            activeSettings   = defaultSettings;
            targetSettings   = defaultSettings;
            targetOrthoSize  = defaultSettings.OrthographicSize;
            currentOrthoSize = defaultSettings.OrthographicSize;
            cam.orthographicSize = currentOrthoSize;
        }
        else
        {
            targetOrthoSize  = cam.orthographicSize;
            currentOrthoSize = cam.orthographicSize;
            Debug.LogWarning("[PlayerCameraController] defaultSettings belum diassign!");
        }

        // Snap kamera ke posisi player saat start (tidak ada lag awal)
        if (playerTransform != null)
        {
            Vector3 startPos = GetDesiredPosition();
            transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
            desiredPosition    = transform.position;
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        float deltaTime = Time.deltaTime;

        // 1. Hitung setting yang aktif (lerp jika transisi sedang berjalan)
        UpdateSettingsTransition(deltaTime);

        // 2. Hitung look-ahead
        UpdateLookAhead(deltaTime);

        // 3. Hitung offset (base vs crouch)
        UpdateOffset(deltaTime);

        // 4. Smooth follow ke desired position
        UpdateCameraFollow(deltaTime);

        // 5. Clamp bounds
        ApplyBounds();

        // 6. Update dynamic zoom (run zoom-out)
        UpdateDynamicZoom(deltaTime);

        // 7. Update orthographic size (zoom) — gabungkan semua modifier
        UpdateOrthoSize(deltaTime);

        // 8. Efek sanity
        UpdateSanityEffects(deltaTime);

        // 8. Terapkan shake offset DI ATAS posisi yang sudah dihitung
        Vector3 finalPos = desiredPosition;
        if (shaker != null && shaker.IsShaking)
            finalPos += shaker.GetShakeOffset();

        transform.position = new Vector3(finalPos.x, finalPos.y, transform.position.z);
    }

    // ─── Setting Transition ────────────────────────────────────────────────

    /// <summary>
    /// Dipanggil oleh CameraZoneTrigger saat Inara memasuki zona baru.
    /// </summary>
    public void TransitionToSettings(CameraSettingsSO newSettings, float duration = 0.8f)
    {
        if (newSettings == null) return;

        targetSettings            = newSettings;
        settingsTransitionDuration = Mathf.Max(0.01f, duration);
        settingsTransitionProgress = 0f;

        // Target ortho size langsung diset; transisi smooth lewat UpdateOrthoSize
        targetOrthoSize = newSettings.OrthographicSize;
    }

    private void UpdateSettingsTransition(float deltaTime)
    {
        if (settingsTransitionProgress >= 1f) return;

        settingsTransitionProgress += deltaTime / settingsTransitionDuration;
        settingsTransitionProgress  = Mathf.Clamp01(settingsTransitionProgress);

        // Saat transisi selesai, commit settings baru sebagai active
        if (settingsTransitionProgress >= 1f)
            activeSettings = targetSettings;
    }

    // ─── Look-Ahead ────────────────────────────────────────────────────────

    private void UpdateLookAhead(float deltaTime)
    {
        if (activeSettings == null) return;

        // Tentukan arah hadap dari PlayerStateController / PlayerMovement
        float facingDir = GetFacingDirection();

        // Tentukan jarak look-ahead (lebih jauh jika berlari)
        bool isRunning = IsPlayerRunning();
        float targetLookAheadDist = isRunning
            ? activeSettings.RunLookAheadDistance
            : activeSettings.LookAheadDistance;

        float targetLookAhead = facingDir * targetLookAheadDist;

        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead,
            activeSettings.LookAheadSpeed * deltaTime);
    }

    // ─── Offset (crouch/stand) ─────────────────────────────────────────────

    private void UpdateOffset(float deltaTime)
    {
        if (activeSettings == null) return;

        bool isCrouching = playerStateController != null && playerStateController.IsCrouching;

        Vector2 targetOffset = isCrouching
            ? activeSettings.CrouchOffset
            : activeSettings.BaseOffset;

        // Lerp transisi offset saat masuk/keluar crouch
        float speed = activeSettings.CrouchOffsetSpeed;

        // Saat transisi zone, gunakan speed dari settings yang dituju
        if (settingsTransitionProgress < 1f && targetSettings != null)
            speed = Mathf.Lerp(activeSettings.CrouchOffsetSpeed, targetSettings.CrouchOffsetSpeed, settingsTransitionProgress);

        currentOffset = Vector2.Lerp(currentOffset, targetOffset, speed * deltaTime);
    }

    // ─── Camera Follow ─────────────────────────────────────────────────────

    private void UpdateCameraFollow(float deltaTime)
    {
        if (activeSettings == null) return;

        Vector3 targetPos = GetDesiredPosition();

        // Tentukan smooth time — lerp antara active dan target saat transisi
        float hSmoothTime = activeSettings.HorizontalSmoothTime;
        float vSmoothTime = activeSettings.VerticalSmoothTime;

        if (settingsTransitionProgress < 1f && targetSettings != null)
        {
            float t = settingsTransitionProgress;
            hSmoothTime = Mathf.Lerp(hSmoothTime, targetSettings.HorizontalSmoothTime, t);
            vSmoothTime = Mathf.Lerp(vSmoothTime, targetSettings.VerticalSmoothTime, t);
        }

        // Horizontal deadzone
        float hDeadzone = activeSettings.HorizontalDeadzone;
        float vDeadzone = activeSettings.VerticalDeadzone;

        float diffX = targetPos.x - desiredPosition.x;
        float diffY = targetPos.y - desiredPosition.y;

        // Hanya gerakkan jika di luar deadzone
        float newX = desiredPosition.x;
        float newY = desiredPosition.y;

        if (Mathf.Abs(diffX) > hDeadzone)
        {
            // SmoothDamp hanya sumbu X
            float targetX = targetPos.x;
            newX = Mathf.SmoothDamp(desiredPosition.x, targetX, ref currentVelocityH.x, hSmoothTime);
        }
        else
        {
            currentVelocityH.x = Mathf.MoveTowards(currentVelocityH.x, 0f, Time.deltaTime * 20f);
        }

        if (Mathf.Abs(diffY) > vDeadzone)
        {
            float targetY = targetPos.y;
            newY = Mathf.SmoothDamp(desiredPosition.y, targetY, ref currentVelocityH.y, vSmoothTime);
        }
        else
        {
            currentVelocityH.y = Mathf.MoveTowards(currentVelocityH.y, 0f, Time.deltaTime * 15f);
        }

        desiredPosition = new Vector3(newX, newY, transform.position.z);
    }

    private Vector3 GetDesiredPosition()
    {
        if (playerTransform == null) return transform.position;

        Vector3 playerPos = playerTransform.position;

        float x = playerPos.x + currentOffset.x + currentLookAhead;
        float y = playerPos.y + currentOffset.y;

        return new Vector3(x, y, transform.position.z);
    }

    // ─── Bounds Clamping ───────────────────────────────────────────────────

    /// <summary>
    /// Dipanggil oleh CameraBoundsArea saat player memasukinya.
    /// </summary>
    public void SetBoundsArea(CameraBoundsArea area)
    {
        if (area == null) return;

        activeBoundsArea = area;
        targetBounds     = area.WorldBounds;
        boundsBlendSpeed = 1f / Mathf.Max(0.01f, area.ActivationDuration);

        // Jika belum punya bounds aktif, snap langsung tanpa lerp
        if (!hasBounds)
        {
            currentBounds = targetBounds;
            boundsBlend   = 1f;
        }
        else
        {
            boundsBlend = 0f; // Mulai lerp dari bounds lama ke baru
        }

        hasBounds = true;
    }

    /// <summary>
    /// Dipanggil oleh CameraBoundsArea saat player keluar.
    /// Kamera kembali ke fallback ScriptableObject bounds (jika ada), atau bebas.
    /// </summary>
    public void ClearBoundsArea()
    {
        activeBoundsArea = null;

        // Cek apakah SO masih punya bounds sebagai fallback
        CameraSettingsSO settings = settingsTransitionProgress >= 1f ? activeSettings : targetSettings;
        if (settings != null && settings.UseBounds)
        {
            targetBounds = new Bounds(
                new Vector3((settings.BoundsMinX + settings.BoundsMaxX) * 0.5f,
                            (settings.BoundsMinY + settings.BoundsMaxY) * 0.5f, 0f),
                new Vector3(settings.BoundsMaxX - settings.BoundsMinX,
                            settings.BoundsMaxY - settings.BoundsMinY, 0f)
            );
            boundsBlend   = 0f;
            boundsBlendSpeed = 2f;
            // hasBounds tetap true — pakai SO bounds sebagai fallback
        }
        else
        {
            // Tidak ada fallback — matikan bounds sepenuhnya
            hasBounds   = false;
            boundsBlend = 1f;
        }
    }

    private void ApplyBounds()
    {
        // Blend bounds jika sedang transisi
        if (boundsBlend < 1f)
        {
            boundsBlend    = Mathf.MoveTowards(boundsBlend, 1f, boundsBlendSpeed * Time.deltaTime);
            currentBounds  = InterpolateBounds(currentBounds, targetBounds, boundsBlend);
        }
        else if (hasBounds)
        {
            // Jika CameraBoundsArea aktif, update setiap frame
            // (area bisa bergerak jika dikombinasikan dengan moving platform)
            if (activeBoundsArea != null)
                currentBounds = activeBoundsArea.WorldBounds;
        }

        if (!hasBounds) return;

        // Hitung setengah ukuran kamera agar kamera tidak memperlihatkan area di luar bounds
        float camHalfH = cam.orthographicSize;
        float camHalfW = cam.orthographicSize * cam.aspect;

        float minX = currentBounds.min.x + camHalfW;
        float maxX = currentBounds.max.x - camHalfW;
        float minY = currentBounds.min.y + camHalfH;
        float maxY = currentBounds.max.y - camHalfH;

        // Guard: jika bounds terlalu kecil untuk kamera, kunci di tengah
        if (minX > maxX) { float mid = (minX + maxX) * 0.5f; minX = maxX = mid; }
        if (minY > maxY) { float mid = (minY + maxY) * 0.5f; minY = maxY = mid; }

        float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);

        desiredPosition = new Vector3(clampedX, clampedY, desiredPosition.z);
    }

    /// <summary>Interpolasi smooth antara dua Bounds.</summary>
    private static Bounds InterpolateBounds(Bounds a, Bounds b, float t)
    {
        return new Bounds(
            Vector3.Lerp(a.center, b.center, t),
            Vector3.Lerp(a.size,   b.size,   t)
        );
    }

    // ─── Orthographic Size ─────────────────────────────────────────────────

    /// <summary>
    /// Update dynamic zoom saat player berlari.
    /// Membaca EnableDynamicZoomOnRun dari CameraSettingsSO aktif.
    /// Zoom-out smooth saat berlari, kembali smooth saat berhenti.
    /// </summary>
    private void UpdateDynamicZoom(float deltaTime)
    {
        if (activeSettings == null) return;

        if (!activeSettings.EnableDynamicZoomOnRun)
        {
            // Kembalikan ke 0 jika fitur dimatikan di zone ini
            currentDynamicZoom = Mathf.SmoothDamp(
                currentDynamicZoom, 0f, ref dynamicZoomVelocity,
                0.3f
            );
            return;
        }

        bool isRunning = IsPlayerRunning();
        float targetDynZoom = isRunning ? activeSettings.RunZoomOutAmount : 0f;
        float dynSpeed = 1f / Mathf.Max(0.01f, activeSettings.DynamicZoomSpeed);

        currentDynamicZoom = Mathf.SmoothDamp(
            currentDynamicZoom, targetDynZoom, ref dynamicZoomVelocity, dynSpeed
        );
    }

    private void UpdateOrthoSize(float deltaTime)
    {
        // ─── Hitung effective target zoom (semua modifier dijumlahkan) ────
        //
        //   Base zoom      : targetOrthoSize (dari CameraSettingsSO zona aktif)
        //   + zoomOffset   : fine-tune manual dari Inspector (bisa negatif / positif)
        //   + dynamicZoom  : zoom-out saat berlari
        //   - sanityZoom   : zoom-in saat sanity kritis (dikurangi = lebih dekat)
        //
        float sanityZoomModifier = GetSanityZoomModifier();
        float effectiveTarget = targetOrthoSize
                                + zoomOffset
                                + currentDynamicZoom
                                - sanityZoomModifier;

        // Clamp dengan batas per zona (dari SO) DAN global (dari Inspector)
        float soMin    = activeSettings != null ? activeSettings.MinOrthographicSize : globalMinZoom;
        float soMax    = activeSettings != null ? activeSettings.MaxOrthographicSize : globalMaxZoom;
        float clampMin = Mathf.Max(soMin, globalMinZoom);
        float clampMax = Mathf.Min(soMax, globalMaxZoom);

        effectiveTarget = Mathf.Clamp(effectiveTarget, clampMin, clampMax);

        // Smooth transition ke target
        currentOrthoSize = Mathf.MoveTowards(
            currentOrthoSize,
            effectiveTarget,
            zoomTransitionSpeed * deltaTime
        );

        cam.orthographicSize = currentOrthoSize;
    }

    private float GetSanityZoomModifier()
    {
        if (sanityController == null) return 0f;

        float sanityRatio = GetSanityRatio();

        if (sanityRatio <= sanityCriticalThreshold)
        {
            // Makin rendah sanity, makin besar zoom-in
            float t = 1f - Mathf.InverseLerp(0f, sanityCriticalThreshold, sanityRatio);
            return criticalZoomAmount * t;
        }

        return 0f;
    }

    // ─── Sanity Effects ────────────────────────────────────────────────────

    private void UpdateSanityEffects(float deltaTime)
    {
        if (sanityController == null || shaker == null) return;

        float sanityRatio = GetSanityRatio();
        bool isCritical = sanityRatio <= sanityCriticalThreshold;

        // Saat pertama kali masuk critical, langsung shake sekali
        if (isCritical && !wasInCriticalSanity)
        {
            CameraShakeController.Shake(CameraShakeController.Preset.Medium);
            sanityShakeTimer = sanityShakeInterval;
        }

        // Shake berulang saat critical
        if (isCritical)
        {
            sanityShakeTimer -= deltaTime;
            if (sanityShakeTimer <= 0f)
            {
                // Kekuatan shake berdasarkan seberapa kritis sanity
                float criticalness = 1f - Mathf.InverseLerp(0f, sanityCriticalThreshold, sanityRatio);
                float magnitude = Mathf.Lerp(0.03f, 0.06f, criticalness);
                float duration  = Mathf.Lerp(0.2f, 0.4f, criticalness);

                CameraShakeController.Shake(duration, magnitude, 25f);
                sanityShakeTimer = Mathf.Lerp(sanityShakeInterval, sanityShakeInterval * 0.4f, criticalness);
            }
        }
        else
        {
            sanityShakeTimer = 0f;
        }

        wasInCriticalSanity = isCritical;
    }

    /// <summary>
    /// Hitung sanity ratio (0-1) dari SanityController yang ada di project.
    /// SanityController tidak punya CurrentSanityRatio property, jadi kita hitung manual.
    /// </summary>
    private float GetSanityRatio()
    {
        if (sanityController == null) return 1f;
        float maxSanity = sanityController.MaxSanity;
        if (maxSanity <= 0f) return 1f;
        return Mathf.Clamp01(sanityController.CurrentSanity / maxSanity);
    }

    // ─── Helper Methods ────────────────────────────────────────────────────

    private float GetFacingDirection()
    {
        // Coba baca dari PlayerMovement via GetComponentInParent/Children
        // Karena kamera bukan child player, kita cari di scene
        if (playerTransform == null) return 1f;

        PlayerMovement movement = playerTransform.GetComponent<PlayerMovement>();
        if (movement != null)
            return movement.IsFacingRight ? 1f : -1f;

        // Fallback: gunakan scale X
        return playerTransform.localScale.x > 0 ? 1f : -1f;
    }

    private bool IsPlayerRunning()
    {
        if (playerTransform == null) return false;

        PlayerInputReader input = playerTransform.GetComponent<PlayerInputReader>();
        if (input == null) return false;

        return input.RunHeld && Mathf.Abs(input.MoveInput.x) > 0.01f && !input.CrouchHeld;
    }

    // ─── Public API untuk Cutscene / Scripted Events ───────────────────────

    /// <summary>
    /// Snap kamera langsung ke posisi player tanpa smooth (berguna saat teleport / scene load).
    /// </summary>
    public void SnapToPlayer()
    {
        if (playerTransform == null) return;

        currentLookAhead = 0f;
        currentOffset    = activeSettings != null ? activeSettings.BaseOffset : Vector2.zero;

        Vector3 snapPos = GetDesiredPosition();
        desiredPosition = snapPos;
        transform.position = new Vector3(snapPos.x, snapPos.y, transform.position.z);

        // Reset velocity
        currentVelocityH = Vector3.zero;
        currentVelocityV = 0f;
    }

    /// <summary>
    /// Override ortho size secara manual (misalnya saat boss cutscene mulai).
    /// </summary>
    public void SetOrthographicSize(float size, float transitionSpeed = -1f)
    {
        targetOrthoSize = size;
        if (transitionSpeed > 0f)
            zoomTransitionSpeed = transitionSpeed;
    }

    /// <summary>
    /// Set target player baru (berguna kalau player respawn / berganti prefab).
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        playerTransform = newTarget;
        SnapToPlayer();
    }

    // ─── Gizmos ────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (!showBoundsGizmo) return;

        // Tampilkan SO bounds sebagai fallback (warna orange, lebih transparan)
        if (activeSettings != null && activeSettings.UseBounds)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            float w = activeSettings.BoundsMaxX - activeSettings.BoundsMinX;
            float h = activeSettings.BoundsMaxY - activeSettings.BoundsMinY;
            Vector3 center = new Vector3(
                (activeSettings.BoundsMinX + activeSettings.BoundsMaxX) * 0.5f,
                (activeSettings.BoundsMinY + activeSettings.BoundsMaxY) * 0.5f,
                0f
            );
            Gizmos.DrawWireCube(center, new Vector3(w, h, 0f));
        }

        // Tampilkan currentBounds yang sedang aktif (warna cyan terang)
        if (Application.isPlaying && hasBounds)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
            Gizmos.DrawWireCube(currentBounds.center, currentBounds.size);

            // Visualisasikan area yang bisa ditempati kamera (dikurangi setengah ukuran kamera)
            if (cam != null)
            {
                float camHalfH = cam.orthographicSize;
                float camHalfW = cam.orthographicSize * cam.aspect;
                Vector3 innerSize = currentBounds.size - new Vector3(camHalfW * 2f, camHalfH * 2f, 0f);
                if (innerSize.x > 0 && innerSize.y > 0)
                {
                    Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
                    Gizmos.DrawCube(currentBounds.center, innerSize);
                }
            }
        }
    }
}
