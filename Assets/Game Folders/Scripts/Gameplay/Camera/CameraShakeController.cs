using UnityEngine;

/// <summary>
/// Sistem kamera shake untuk "The Day After".
/// Gunakan CameraShakeController.Shake(...) dari script manapun untuk memicu efek shake.
/// </summary>
public class CameraShakeController : MonoBehaviour
{
    // ─── Singleton ─────────────────────────────────────────────────────────
    public static CameraShakeController Instance { get; private set; }

    // ─── State ─────────────────────────────────────────────────────────────
    private float shakeDuration;
    private float shakeMagnitude;
    private float shakeFrequency;
    private float shakeTimer;
    private Vector3 shakeOffset;

    // Untuk menghindari stutter, kita track beberapa shake sekaligus
    private float currentMagnitude;
    private float currentFrequency;
    private float velocityMag;

    // ─── Preset yang sering dipakai di game ────────────────────────────────
    public static class Preset
    {
        /// <summary>Shake ringan saat musuh muncul / distorsi environment</summary>
        public static ShakeData Light    => new ShakeData(0.15f, 0.05f, 20f);
        /// <summary>Shake medium saat serangan lentera berhasil memurnikan musuh</summary>
        public static ShakeData Medium   => new ShakeData(0.25f, 0.12f, 18f);
        /// <summary>Shake berat saat Boss menyerang / wall collapse di Boss Arena</summary>
        public static ShakeData Heavy    => new ShakeData(0.4f, 0.22f, 15f);
        /// <summary>Shake keras sekali saat jumpscare Chapter 4 (musuh kamuflase terungkap)</summary>
        public static ShakeData Jumpscare => new ShakeData(0.35f, 0.35f, 25f);
        /// <summary>Shake pulsing panjang saat sanity kritis (ongoing)</summary>
        public static ShakeData SanityPulse => new ShakeData(0.6f, 0.04f, 30f);
        /// <summary>Shake saat final boss HP turun ke tahap baru</summary>
        public static ShakeData BossPhase => new ShakeData(0.5f, 0.28f, 12f);
    }

    [System.Serializable]
    public struct ShakeData
    {
        public float Duration;
        public float Magnitude;
        public float Frequency;

        public ShakeData(float duration, float magnitude, float frequency)
        {
            Duration  = duration;
            Magnitude = magnitude;
            Frequency = frequency;
        }
    }

    // ─── Unity Lifecycle ───────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ─── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Memicu camera shake dengan preset.
    /// Contoh: CameraShakeController.Shake(CameraShakeController.Preset.Heavy);
    /// </summary>
    public static void Shake(ShakeData preset)
    {
        if (Instance != null)
            Instance.DoShake(preset.Duration, preset.Magnitude, preset.Frequency);
    }

    /// <summary>
    /// Memicu camera shake dengan parameter custom.
    /// </summary>
    /// <param name="duration">Durasi shake (detik)</param>
    /// <param name="magnitude">Amplitudo / kekuatan shake</param>
    /// <param name="frequency">Frekuensi per detik (makin tinggi = makin cepat bergetar)</param>
    public static void Shake(float duration, float magnitude, float frequency = 20f)
    {
        if (Instance != null)
            Instance.DoShake(duration, magnitude, frequency);
    }

    /// <summary>
    /// Hentikan semua shake yang sedang berjalan.
    /// </summary>
    public static void StopShake()
    {
        if (Instance != null)
        {
            Instance.shakeTimer = 0f;
            Instance.shakeOffset = Vector3.zero;
        }
    }

    /// <summary>
    /// Kembalikan offset shake saat ini untuk diterapkan ke posisi kamera.
    /// Dipanggil oleh PlayerCameraController setiap frame.
    /// </summary>
    public Vector3 GetShakeOffset()
    {
        return shakeOffset;
    }

    public bool IsShaking => shakeTimer > 0f;

    // ─── Internal ──────────────────────────────────────────────────────────

    private void DoShake(float duration, float magnitude, float frequency)
    {
        // Jika shake baru lebih kuat dari yang sedang berjalan, override
        if (magnitude >= currentMagnitude || shakeTimer <= 0f)
        {
            shakeDuration  = duration;
            shakeMagnitude = magnitude;
            shakeFrequency = frequency;
            shakeTimer     = duration;
        }
        else
        {
            // Tambahkan durasi jika shake sedang berjalan
            shakeTimer = Mathf.Max(shakeTimer, duration * 0.5f);
        }
    }

    private void Update()
    {
        if (shakeTimer > 0f)
        {
            // Hitung progress (1 = awal, 0 = selesai)
            float progress = shakeTimer / shakeDuration;

            // Smooth magnitude dengan SmoothDamp agar tidak langsung cut
            currentMagnitude = Mathf.SmoothDamp(currentMagnitude, shakeMagnitude * progress, ref velocityMag, 0.05f);

            // Perlin noise untuk shake yang organik (tidak kaku)
            float noiseX = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f;

            shakeOffset = new Vector3(noiseX, noiseY, 0f) * currentMagnitude;

            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f)
            {
                shakeTimer    = 0f;
                shakeOffset   = Vector3.zero;
                currentMagnitude = 0f;
            }
        }
        else
        {
            shakeOffset      = Vector3.zero;
            currentMagnitude = 0f;
        }
    }
}
