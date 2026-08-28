using UnityEngine;
using UnityEngine.UI;

public class SanityBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SanityController sanityController;
    [SerializeField] private Image mainFill;

    [Header("Smooth")]
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Sanity Colors")]
    [SerializeField] private Color healthyColor = new Color(0.25f, 0.9f, 0.35f);
    [SerializeField] private Color unstableColor = new Color(1f, 0.75f, 0.2f);
    [SerializeField] private Color criticalColor = new Color(1f, 0.25f, 0.2f);
    [SerializeField] private Color depletedColor = new Color(0.25f, 0.1f, 0.1f);

    [Header("Color Smooth")]
    [SerializeField] private float colorSmoothSpeed = 8f;

    private float targetFillAmount;
    private float currentFillAmount;

    private Color targetColor;

    private void Awake()
    {
        if (sanityController == null)
            sanityController = FindFirstObjectByType<SanityController>();

        if (mainFill == null)
            Debug.LogError("SanityBarUI: Main Fill belum di-assign.", this);
    }

    private void Start()
    {
        InitializeBar();
    }

    private void OnEnable()
    {
        if (sanityController == null) return;

        sanityController.OnSanityChanged += HandleSanityChanged;
        sanityController.OnSanityLevelChanged += HandleSanityLevelChanged;
    }

    private void OnDisable()
    {
        if (sanityController == null) return;

        sanityController.OnSanityChanged -= HandleSanityChanged;
        sanityController.OnSanityLevelChanged -= HandleSanityLevelChanged;
    }

    private void InitializeBar()
    {
        if (sanityController == null)
        {
            Debug.LogError("SanityBarUI: SanityController tidak ditemukan.", this);
            return;
        }

        if (mainFill == null) return;

        float maxSanity = sanityController.MaxSanity;

        if (maxSanity <= 0f)
        {
            Debug.LogError("SanityBarUI: Max Sanity harus lebih dari 0.", this);
            return;
        }

        targetFillAmount = sanityController.CurrentSanity / maxSanity;
        currentFillAmount = targetFillAmount;
        mainFill.fillAmount = currentFillAmount;

        targetColor = GetColorFromLevel(sanityController.CurrentLevel);
        mainFill.color = targetColor;
    }

    private void Update()
    {
        if (mainFill == null) return;

        UpdateFill();
        UpdateColor();
    }

    private void UpdateFill()
    {
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, smoothSpeed * Time.deltaTime);
        mainFill.fillAmount = currentFillAmount;
    }

    private void UpdateColor()
    {
        mainFill.color = Color.Lerp(mainFill.color, targetColor, colorSmoothSpeed * Time.deltaTime);
    }

    private void HandleSanityChanged(float currentSanity, float maxSanity)
    {
        if (maxSanity <= 0f) return;
        targetFillAmount = Mathf.Clamp01(currentSanity / maxSanity);
    }

    private void HandleSanityLevelChanged(SanityLevel newLevel)
    {
        targetColor = GetColorFromLevel(newLevel);
    }

    private Color GetColorFromLevel(SanityLevel level)
    {
        switch (level)
        {
            case SanityLevel.Healthy: return healthyColor;
            case SanityLevel.Unstable: return unstableColor;
            case SanityLevel.Critical: return criticalColor;
            case SanityLevel.Depleted: return depletedColor;
            default: return healthyColor;
        }
    }
}