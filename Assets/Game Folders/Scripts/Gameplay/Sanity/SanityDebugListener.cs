using UnityEngine;

public class SanityDebugListener : MonoBehaviour
{
    [SerializeField] private SanityController sanityController;

    private void Awake()
    {
        if (sanityController == null)
        {
            sanityController = GetComponent<SanityController>();
        }
    }

    private void OnEnable()
    {
        if (sanityController == null)
        {
            return;
        }

        sanityController.OnSanityChanged += HandleSanityChanged;
        sanityController.OnSanityLevelChanged += HandleSanityLevelChanged;
    }

    private void OnDisable()
    {
        if (sanityController == null)
        {
            return;
        }

        sanityController.OnSanityChanged -= HandleSanityChanged;
        sanityController.OnSanityLevelChanged -= HandleSanityLevelChanged;
    }

    private void HandleSanityChanged(float current, float max)
    {
        Debug.Log(
            $"[EVENT] Sanity Changed: {current:F1}/{max:F1}",
            this
        );
    }

    private void HandleSanityLevelChanged(SanityLevel level)
    {
        Debug.Log(
            $"[EVENT] Sanity Level Changed: {level}",
            this
        );
    }
}