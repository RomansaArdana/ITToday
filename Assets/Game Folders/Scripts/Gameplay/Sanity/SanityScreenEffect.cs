using UnityEngine;
using UnityEngine.UI;

public class SanityScreenEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SanityController sanityController;
    [SerializeField] private Image vignette;

    [Header("Effect")]
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Intensity")]
    [SerializeField] private float unstableAlpha = 0.15f;
    [SerializeField] private float criticalAlpha = 0.3f;
    [SerializeField] private float depletedAlpha = 0.5f;

    [Header("Colors")]
    [SerializeField] private Color unstableColor = new Color(1f, 0.65f, 0.1f);
    [SerializeField] private Color criticalColor = new Color(1f, 0.1f, 0.1f);
    [SerializeField] private Color depletedColor = new Color(0.2f, 0f, 0f);

    private float targetAlpha;
    private Color targetColor;

    private void Awake()
    {
        if (sanityController == null)
            sanityController = FindFirstObjectByType<SanityController>();

        if (vignette == null)
        {
            Debug.LogError("SanityScreenEffect: Vignette belum di-assign.", this);
            return;
        }

        targetAlpha = 0f;
        targetColor = Color.clear;

        Color startColor = vignette.color;
        startColor.a = 0f;
        vignette.color = startColor;
    }

    private void OnEnable()
    {
        if (sanityController == null) return;
        sanityController.OnSanityLevelChanged += HandleSanityLevelChanged;
    }

    private void OnDisable()
    {
        if (sanityController == null) return;
        sanityController.OnSanityLevelChanged -= HandleSanityLevelChanged;
    }

    private void Start()
    {
        if (sanityController == null) return;
        HandleSanityLevelChanged(sanityController.CurrentLevel);
    }

    private void Update()
    {
        if (vignette == null) return;

        Color currentColor = vignette.color;

        currentColor = Color.Lerp(currentColor, targetColor, smoothSpeed * Time.deltaTime);
        currentColor.a = Mathf.Lerp(currentColor.a, targetAlpha, smoothSpeed * Time.deltaTime);

        vignette.color = currentColor;
    }

    private void HandleSanityLevelChanged(SanityLevel level)
    {
        switch (level)
        {
            case SanityLevel.Healthy:
                targetAlpha = 0f;
                targetColor = Color.clear;
                break;

            case SanityLevel.Unstable:
                targetAlpha = unstableAlpha;
                targetColor = unstableColor;
                break;

            case SanityLevel.Critical:
                targetAlpha = criticalAlpha;
                targetColor = criticalColor;
                break;

            case SanityLevel.Depleted:
                targetAlpha = depletedAlpha;
                targetColor = depletedColor;
                break;
        }
    }
}