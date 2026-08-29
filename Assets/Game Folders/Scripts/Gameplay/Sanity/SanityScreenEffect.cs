using UnityEngine;
using UnityEngine.UI;

public class SanityScreenEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SanityController sanityController;
    [Tooltip("Full-screen UI Image untuk tint sanity yang sangat tipis.")]
    [SerializeField] private Image effectImage;
    [Tooltip("Material menggunakan Custom/SanityPostProcess dan dipasang ke Full Screen Pass Renderer Feature.")]
    [SerializeField] private Material postProcessMaterial;

    [Header("Overlay Sprites")]
    [SerializeField] private Sprite unstableSprite;
    [SerializeField] private Sprite criticalSprite;
    [SerializeField] private Sprite depletedSprite;

    [Header("Unstable")]
    [SerializeField] private Color unstableColor = new Color(1f, 0.90f, 0.62f, 1f);
    [SerializeField, Range(0f, 1f)] private float unstableMinAlpha = 0.005f;
    [SerializeField, Range(0f, 1f)] private float unstableMaxAlpha = 0.025f;
    [SerializeField, Range(0f, 5f)] private float unstableBlur = 0.15f;
    [SerializeField, Range(0f, 1f)] private float unstableGrayscale = 0.04f;
    [SerializeField, Range(0f, 0.5f)] private float unstablePulseStrength = 0.025f;
    [SerializeField, Range(0f, 2f)] private float unstablePulseSpeed = 0.28f;
    [SerializeField, Range(0f, 0.1f)] private float unstableDistortion = 0.003f;
    [SerializeField, Range(0f, 0.2f)] private float unstableNoise = 0.004f;
    [SerializeField, Range(0f, 1f)] private float unstableScanline = 0.00f;
    [SerializeField] private float unstableAlphaPulseSpeed = 0.40f;

    [Header("Critical")]
    [SerializeField] private Color criticalColor = new Color(0.90f, 0.16f, 0.10f, 1f);
    [SerializeField, Range(0f, 1f)] private float criticalMinAlpha = 0.01f;
    [SerializeField, Range(0f, 1f)] private float criticalMaxAlpha = 0.045f;
    [SerializeField, Range(0f, 5f)] private float criticalBlur = 0.45f;
    [SerializeField, Range(0f, 1f)] private float criticalGrayscale = 0.12f;
    [SerializeField, Range(0f, 0.5f)] private float criticalPulseStrength = 0.055f;
    [SerializeField, Range(0f, 2f)] private float criticalPulseSpeed = 0.50f;
    [SerializeField, Range(0f, 0.1f)] private float criticalDistortion = 0.008f;
    [SerializeField, Range(0f, 0.2f)] private float criticalNoise = 0.010f;
    [SerializeField, Range(0f, 1f)] private float criticalScanline = 0.015f;
    [SerializeField] private float criticalAlphaPulseSpeed = 0.58f;

    [Header("Depleted")]
    [SerializeField] private Color depletedColor = new Color(0.28f, 0.025f, 0.02f, 1f);
    [SerializeField, Range(0f, 1f)] private float depletedMinAlpha = 0.02f;
    [SerializeField, Range(0f, 1f)] private float depletedMaxAlpha = 0.08f;
    [SerializeField, Range(0f, 5f)] private float depletedBlur = 1.00f;
    [SerializeField, Range(0f, 1f)] private float depletedGrayscale = 0.28f;
    [SerializeField, Range(0f, 0.5f)] private float depletedPulseStrength = 0.09f;
    [SerializeField, Range(0f, 2f)] private float depletedPulseSpeed = 0.65f;
    [SerializeField, Range(0f, 0.1f)] private float depletedDistortion = 0.018f;
    [SerializeField, Range(0f, 0.2f)] private float depletedNoise = 0.025f;
    [SerializeField, Range(0f, 1f)] private float depletedScanline = 0.05f;
    [SerializeField] private float depletedAlphaPulseSpeed = 0.72f;

    [Header("Transition Smoothing")]
    [SerializeField] private float colorSmoothSpeed = 3.5f;
    [SerializeField] private float alphaSmoothSpeed = 2.5f;
    [SerializeField] private float shaderSmoothSpeed = 2.0f;

    [Header("Overlay Pulse Shape")]
    [Tooltip("Semakin kecil, pulse semakin lembut.")]
    [SerializeField, Range(0.1f, 2f)] private float pulseSharpness = 0.45f;
    [SerializeField, Range(0f, 0.3f)] private float pulseVariation = 0.10f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;

    private SanityLevel currentLevel = SanityLevel.Healthy;
    private Color targetOverlayColor = Color.white;
    private float targetMinAlpha;
    private float targetMaxAlpha;
    private float targetAlphaPulseSpeed;

    private float targetBlur;
    private float targetGrayscale;
    private float targetPulseStrength;
    private float targetPulseSpeed;
    private float targetDistortion;
    private float targetNoise;
    private float targetScanline;

    private float currentBlur;
    private float currentGrayscale;
    private float currentPulseStrength;
    private float currentPulseSpeed;
    private float currentDistortion;
    private float currentNoise;
    private float currentScanline;

    private Color? externalOverlayColor;

    private void Awake()
    {
        if (sanityController == null)
            sanityController = FindFirstObjectByType<SanityController>();

        if (effectImage == null)
        {
            Debug.LogError("[SanityScreenEffect] effectImage belum di-assign!", this);
            return;
        }

        ResetEffect();
    }

    private void OnEnable()
    {
        if (sanityController == null) return;
        sanityController.OnSanityLevelChanged += HandleSanityLevelChanged;
    }

    private void OnDisable()
    {
        if (sanityController != null)
            sanityController.OnSanityLevelChanged -= HandleSanityLevelChanged;
    }

    private void Start()
    {
        if (sanityController == null) return;
        HandleSanityLevelChanged(sanityController.CurrentLevel);
    }

    private void Update()
    {
        if (effectImage == null) return;
        UpdateShaderProperties();
        UpdateOverlayImage();
    }

    private void OnApplicationQuit()
    {
        ResetShaderImmediate();
    }

    private void OnDestroy()
    {
        ResetShaderImmediate();
    }

    // ============================================================
    // PUBLIC API
    // ============================================================
    public void SetOverlayColor(Color? color)
    {
        externalOverlayColor = color;
        if (color.HasValue) targetOverlayColor = color.Value;
    }

    public void SetOverlayColor(Color color)
    {
        SetOverlayColor((Color?)color);
    }

    public void ClearOverlayColor()
    {
        SetOverlayColor((Color?)null);
    }

    public void SetLevel(SanityLevel level)
    {
        HandleSanityLevelChanged(level);
    }

    // ============================================================
    // OVERLAY IMAGE
    // ============================================================
    private void UpdateOverlayImage()
    {
        if (currentLevel == SanityLevel.Healthy)
        {
            FadeOverlayAlpha(0f);
            return;
        }

        float time = Time.time * Mathf.Max(0f, targetAlphaPulseSpeed);
        float pulse = (Mathf.Sin(time * Mathf.PI * 2f) + 1f) * 0.5f;
        pulse = Mathf.Pow(pulse, Mathf.Max(0.1f, pulseSharpness));

        float variation = 1f + Mathf.Sin(Time.time * targetAlphaPulseSpeed * 0.41f) * pulseVariation;
        float desiredAlpha = Mathf.Clamp01(Mathf.Lerp(targetMinAlpha, targetMaxAlpha, pulse) * variation);

        Color effectiveColor = externalOverlayColor.HasValue ? externalOverlayColor.Value : targetOverlayColor;

        Color current = effectImage.color;
        current = Color.Lerp(current, effectiveColor, Mathf.Clamp01(colorSmoothSpeed * Time.deltaTime));
        current.a = Mathf.Lerp(current.a, desiredAlpha, Mathf.Clamp01(alphaSmoothSpeed * Time.deltaTime));

        effectImage.color = current;
    }

    private void FadeOverlayAlpha(float targetAlpha)
    {
        Color current = effectImage.color;
        current.a = Mathf.Lerp(current.a, targetAlpha, Mathf.Clamp01(alphaSmoothSpeed * Time.deltaTime));
        effectImage.color = current;
    }

    // ============================================================
    // SHADER PROPERTIES
    // ============================================================
    private void UpdateShaderProperties()
    {
        if (postProcessMaterial == null) return;

        float dt = Time.deltaTime;
        float blend = Mathf.Clamp01(shaderSmoothSpeed * dt);

        currentBlur = Mathf.Lerp(currentBlur, targetBlur, blend);
        currentGrayscale = Mathf.Lerp(currentGrayscale, targetGrayscale, blend);
        currentPulseStrength = Mathf.Lerp(currentPulseStrength, targetPulseStrength, blend);
        currentPulseSpeed = Mathf.Lerp(currentPulseSpeed, targetPulseSpeed, blend);
        currentDistortion = Mathf.Lerp(currentDistortion, targetDistortion, blend);
        currentNoise = Mathf.Lerp(currentNoise, targetNoise, blend);
        currentScanline = Mathf.Lerp(currentScanline, targetScanline, blend);

        postProcessMaterial.SetFloat("_Blur", currentBlur);
        postProcessMaterial.SetFloat("_Grayscale", currentGrayscale);
        postProcessMaterial.SetFloat("_PulseStrength", currentPulseStrength);
        postProcessMaterial.SetFloat("_PulseSpeed", currentPulseSpeed);
        postProcessMaterial.SetFloat("_Distortion", currentDistortion);
        postProcessMaterial.SetFloat("_Noise", currentNoise);
        postProcessMaterial.SetFloat("_ScanlineIntensity", currentScanline);
    }

    // ============================================================
    // LEVEL HANDLING
    // ============================================================
    private void HandleSanityLevelChanged(SanityLevel level)
    {
        currentLevel = level;

        switch (level)
        {
            case SanityLevel.Healthy:
                ResetEffect();
                break;
            case SanityLevel.Unstable:
                ApplyUnstable();
                break;
            case SanityLevel.Critical:
                ApplyCritical();
                break;
            case SanityLevel.Depleted:
                ApplyDepleted();
                break;
        }

        if (enableDebugLog)
        {
            Debug.Log($"[SanityScreenEffect] Level → {level}", this);
        }
    }

    // ============================================================
    // UNSTABLE
    // ============================================================
    private void ApplyUnstable()
    {
        if (unstableSprite != null) effectImage.sprite = unstableSprite;
        if (!externalOverlayColor.HasValue) targetOverlayColor = unstableColor;

        targetMinAlpha = unstableMinAlpha;
        targetMaxAlpha = unstableMaxAlpha;
        targetAlphaPulseSpeed = unstableAlphaPulseSpeed;
        targetBlur = unstableBlur;
        targetGrayscale = unstableGrayscale;
        targetPulseStrength = unstablePulseStrength;
        targetPulseSpeed = unstablePulseSpeed;
        targetDistortion = unstableDistortion;
        targetNoise = unstableNoise;
        targetScanline = unstableScanline;
    }

    // ============================================================
    // CRITICAL
    // ============================================================
    private void ApplyCritical()
    {
        if (criticalSprite != null) effectImage.sprite = criticalSprite;
        if (!externalOverlayColor.HasValue) targetOverlayColor = criticalColor;

        targetMinAlpha = criticalMinAlpha;
        targetMaxAlpha = criticalMaxAlpha;
        targetAlphaPulseSpeed = criticalAlphaPulseSpeed;
        targetBlur = criticalBlur;
        targetGrayscale = criticalGrayscale;
        targetPulseStrength = criticalPulseStrength;
        targetPulseSpeed = criticalPulseSpeed;
        targetDistortion = criticalDistortion;
        targetNoise = criticalNoise;
        targetScanline = criticalScanline;
    }

    // ============================================================
    // DEPLETED
    // ============================================================
    private void ApplyDepleted()
    {
        if (depletedSprite != null) effectImage.sprite = depletedSprite;
        if (!externalOverlayColor.HasValue) targetOverlayColor = depletedColor;

        targetMinAlpha = depletedMinAlpha;
        targetMaxAlpha = depletedMaxAlpha;
        targetAlphaPulseSpeed = depletedAlphaPulseSpeed;
        targetBlur = depletedBlur;
        targetGrayscale = depletedGrayscale;
        targetPulseStrength = depletedPulseStrength;
        targetPulseSpeed = depletedPulseSpeed;
        targetDistortion = depletedDistortion;
        targetNoise = depletedNoise;
        targetScanline = depletedScanline;
    }

    // ============================================================
    // RESET
    // ============================================================
    private void ResetEffect()
    {
        if (!externalOverlayColor.HasValue) targetOverlayColor = Color.white;

        targetMinAlpha = 0f;
        targetMaxAlpha = 0f;
        targetAlphaPulseSpeed = 0f;
        targetBlur = 0f;
        targetGrayscale = 0f;
        targetPulseStrength = 0f;
        targetPulseSpeed = 0f;
        targetDistortion = 0f;
        targetNoise = 0f;
        targetScanline = 0f;

        if (effectImage != null)
        {
            effectImage.sprite = null;
            effectImage.color = new Color(1f, 1f, 1f, 0f);
        }

        ResetShaderImmediate();
    }

    // ============================================================
    // RESET SHADER
    // ============================================================
    private void ResetShaderImmediate()
    {
        if (postProcessMaterial == null) return;

        currentBlur = 0f;
        currentGrayscale = 0f;
        currentPulseStrength = 0f;
        currentPulseSpeed = 0f;
        currentDistortion = 0f;
        currentNoise = 0f;
        currentScanline = 0f;

        postProcessMaterial.SetFloat("_Blur", 0f);
        postProcessMaterial.SetFloat("_Grayscale", 0f);
        postProcessMaterial.SetFloat("_PulseStrength", 0f);
        postProcessMaterial.SetFloat("_PulseSpeed", 0f);
        postProcessMaterial.SetFloat("_Distortion", 0f);
        postProcessMaterial.SetFloat("_Noise", 0f);
        postProcessMaterial.SetFloat("_ScanlineIntensity", 0f);
    }
}