using UnityEngine;

public class EnemyPurpleCamouflage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyPurpleAnomalyDetector anomalyDetector;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private SpriteRenderer bodyRenderer;

    [Header("Camouflage")]
    [SerializeField] private bool hideWhenNotDetected = true;
    [SerializeField, Range(0f, 1f)] private float hiddenAlpha = 0f;
    [SerializeField, Range(0f, 1f)] private float revealedAlpha = 1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private bool previousAnomalyState;
    private bool isDead;

    private void Awake()
    {
        anomalyDetector ??= GetComponent<EnemyPurpleAnomalyDetector>();
        enemyHealth ??= GetComponent<EnemyHealth>();

        if (bodyRenderer == null)
        {
            Transform body = transform.Find("Body");
            if (body != null)
                bodyRenderer = body.GetComponent<SpriteRenderer>();
        }

        previousAnomalyState = false;
        isDead = false;

        ApplyCamouflage(false);
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
            enemyHealth.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        if (isDead)
        {
            EnforceVisibilityOnDeath();
            return;
        }

        if (anomalyDetector == null || bodyRenderer == null) return;
        if (enemyHealth != null && !enemyHealth.IsAlive) return;

        bool currentAnomalyState = anomalyDetector.IsAnomalyDetected;

        if (currentAnomalyState == previousAnomalyState) return;

        previousAnomalyState = currentAnomalyState;
        ApplyCamouflage(currentAnomalyState);
    }

    private void EnforceVisibilityOnDeath()
    {
        if (bodyRenderer == null) return;

        bodyRenderer.enabled = true;

        Color color = bodyRenderer.color;
        color.a = 1f;
        bodyRenderer.color = color;
    }


    private void HandleDeath()
    {
        if (isDead) return;

        isDead = true;
        previousAnomalyState = true;

        ForceReveal();

        if (enableDebugLog)
            Debug.Log("[Purple] CAMOUFLAGE DISABLED ON DEATH", this);
    }

    private void ApplyCamouflage(bool anomalyDetected)
    {
        if (bodyRenderer == null) return;

        bodyRenderer.enabled = true;

        Color color = bodyRenderer.color;

        if (hideWhenNotDetected && !anomalyDetected)
            color.a = hiddenAlpha;
        else
            color.a = revealedAlpha;

        bodyRenderer.color = color;

        if (!enableDebugLog) return;

        Debug.Log(
            anomalyDetected
                ? "[Purple] CAMOUFLAGE REVEALED"
                : "[Purple] CAMOUFLAGE HIDDEN",
            this
        );
    }

    public void ForceReveal()
    {
        ApplyCamouflage(true);
    }

    public void ForceHide()
    {
        if (isDead) return;

        ApplyCamouflage(false);
    }
}