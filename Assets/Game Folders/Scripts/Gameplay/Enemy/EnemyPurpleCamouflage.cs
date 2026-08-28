using UnityEngine;

public class EnemyPurpleCamouflage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyPurpleAnomalyDetector anomalyDetector;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private SpriteRenderer bodyRenderer;

    [Header("Camouflage")]
    [SerializeField] private bool hideWhenNotDetected = true;
    [SerializeField] private float revealedAlpha = 1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private bool previousAnomalyState;

    private void Awake()
    {
        anomalyDetector ??= GetComponent<EnemyPurpleAnomalyDetector>();
        enemyHealth ??= GetComponent<EnemyHealth>();

        if (bodyRenderer == null)
        {
            Transform body = transform.Find("Body");
            if (body != null) bodyRenderer = body.GetComponent<SpriteRenderer>();
        }

        previousAnomalyState = false;
        ApplyCamouflage(false);
    }

    private void Update()
    {
        if (anomalyDetector == null || bodyRenderer == null) return;
        if (enemyHealth != null && !enemyHealth.IsAlive) return;

        bool currentAnomalyState = anomalyDetector.IsAnomalyDetected;

        if (currentAnomalyState == previousAnomalyState) return;

        previousAnomalyState = currentAnomalyState;

        ApplyCamouflage(currentAnomalyState);
    }

    private void ApplyCamouflage(bool anomalyDetected)
    {
        if (bodyRenderer == null) return;

        if (hideWhenNotDetected && !anomalyDetected)
        {
            bodyRenderer.enabled = false;
        }
        else
        {
            bodyRenderer.enabled = true;

            Color color = bodyRenderer.color;
            color.a = Mathf.Clamp01(revealedAlpha);
            bodyRenderer.color = color;
        }

        if (!enableDebugLog) return;

        Debug.Log(anomalyDetected ? "[Purple] CAMOUFLAGE REVEALED" : "[Purple] CAMOUFLAGE HIDDEN", this);
    }

    public void ForceReveal() => ApplyCamouflage(true);
    public void ForceHide() => ApplyCamouflage(false);
}