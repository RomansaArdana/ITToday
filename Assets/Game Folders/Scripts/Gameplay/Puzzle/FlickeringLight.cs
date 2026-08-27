using UnityEngine;
using UnityEngine.Rendering.Universal; // Untuk Light2D (URP)

/// <summary>
/// Pasang script ini di GameObject lampu berkedip.
/// Menganimasikan intensitas Light2D dengan efek flicker realistis.
///
/// Setup:
/// 1. Tambahkan komponen Light2D (URP) di GameObject yang sama
/// 2. Atur MinIntensity, MaxIntensity, FlickerSpeed sesuai kebutuhan
///
/// Catatan: Jika tidak pakai URP Light2D, ganti Light2D dengan SpriteRenderer
/// dan animasikan alpha/color-nya.
/// </summary>
public class FlickeringLight : MonoBehaviour
{
    [Header("Light Reference")]
    [SerializeField] private Light2D light2D;

    [Header("Flicker Settings")]
    [SerializeField] private float minIntensity = 0.6f;
    [SerializeField] private float maxIntensity = 1.4f;
    [SerializeField] private float flickerSpeed = 8f;
    [Tooltip("Seberapa acak flicker-nya (0 = smooth sine, 1 = sangat random)")]
    [SerializeField, Range(0f, 1f)] private float randomness = 0.4f;

    private float noiseOffset;
    private float baseIntensity;

    private void Awake()
    {
        if (light2D == null)
            light2D = GetComponent<Light2D>();

        // Random offset agar setiap lampu berbeda phase-nya
        noiseOffset = Random.Range(0f, 100f);

        if (light2D != null)
            baseIntensity = light2D.intensity;
    }

    private void Update()
    {
        if (light2D == null)
            return;

        // Gabungkan sine wave + perlin noise untuk efek flicker alami
        float sineWave = Mathf.Sin(Time.time * flickerSpeed + noiseOffset);
        float perlinNoise = Mathf.PerlinNoise(Time.time * flickerSpeed * 0.5f + noiseOffset, 0f);

        float blended = Mathf.Lerp(sineWave * 0.5f + 0.5f, perlinNoise, randomness);
        light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, blended);
    }

    /// <summary>
    /// Posisi dunia dari lampu ini (digunakan oleh ShadowRaycastChecker)
    /// </summary>
    public Vector2 LightPosition => transform.position;
}
