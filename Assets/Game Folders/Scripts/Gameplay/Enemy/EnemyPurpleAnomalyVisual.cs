using UnityEngine;

public class EnemyPurpleAnomalyVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyPurpleAnomalyDetector anomalyDetector;
    [SerializeField] private SpriteRenderer anomalyRenderer;

    [Header("Visual")]
    [SerializeField] private bool visibleWhenDetected = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private bool previousState;

    private void Awake()
    {
        anomalyDetector ??= GetComponent<EnemyPurpleAnomalyDetector>();

        if (anomalyRenderer == null)
        {
            Transform visual = transform.Find("AnomalyVisual");
            if (visual != null) anomalyRenderer = visual.GetComponent<SpriteRenderer>();
        }

        SetVisual(false);
        previousState = false;
    }

    private void Update()
    {
        if (anomalyDetector == null || anomalyRenderer == null) return;

        bool currentState = anomalyDetector.IsAnomalyDetected;

        if (currentState == previousState) return;

        previousState = currentState;

        SetVisual(currentState && visibleWhenDetected);

        if (!enableDebugLog) return;

        Debug.Log(currentState ? "[Purple] ANOMALY VISUAL ON" : "[Purple] ANOMALY VISUAL OFF", this);
    }

    private void SetVisual(bool visible)
    {
        if (anomalyRenderer == null) return;
        anomalyRenderer.enabled = visible;
    }
}