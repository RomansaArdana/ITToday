using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemySearch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyStateController stateController;
    [SerializeField] private EnemyDetection detection;

    [Header("Investigation")]
    [SerializeField] private float investigateDelay = 1f;

    [Header("Search")]
    [SerializeField] private float searchRadius = 1.5f;
    [SerializeField] private float searchDuration = 5f;
    [SerializeField] private float arrivalDistance = 0.1f;
    [SerializeField] private float searchMoveSpeed = 1.5f;
    [SerializeField] private int searchPointCount = 4;

    private Rigidbody2D rb;

    private bool isSearching;
    private bool isInvestigating;

    private float investigateTimer;
    private float searchTimer;

    private int currentSearchPoint;

    private Vector2 searchCenter;
    private Vector2 currentSearchTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        enemyController ??= GetComponent<EnemyController>();
        stateController ??= GetComponent<EnemyStateController>();
        detection ??= GetComponent<EnemyDetection>();
    }

    private void FixedUpdate()
    {
        if (!IsValid() || detection.CurrentState != EnemyDetectionState.Suspicious)
        {
            StopSearch();
            return;
        }

        if (!isSearching) BeginSearch();

        if (isInvestigating)
            UpdateInvestigation();
        else
            UpdateSearch();
    }

    private bool IsValid()
    {
        return enemyController != null &&
               stateController != null &&
               detection != null &&
               enemyController.Stats != null;
    }

    private void BeginSearch()
    {
        isSearching = true;
        isInvestigating = true;

        investigateTimer = 0f;
        searchTimer = 0f;
        currentSearchPoint = 0;

        searchCenter = detection.LastKnownPlayerPosition;
        currentSearchTarget = searchCenter;

        StopMovement();

    }

    private void UpdateInvestigation()
    {
        StopMovement();

        investigateTimer += Time.fixedDeltaTime;

        if (investigateTimer < investigateDelay) return;

        isInvestigating = false;
        searchTimer = 0f;

        SetNextSearchPoint();
    }

    private void UpdateSearch()
    {
        searchTimer += Time.fixedDeltaTime;

        if (searchTimer >= searchDuration)
        {
            FinishSearch();
            return;
        }

        if (HasReachedTarget())
        {
            SetNextSearchPoint();
            return;
        }

        MoveTowardsTarget();
    }

    private bool HasReachedTarget()
    {
        return (rb.position - currentSearchTarget).sqrMagnitude <= arrivalDistance * arrivalDistance;
    }

    private void SetNextSearchPoint()
    {
        currentSearchPoint++;

        int pointCount = Mathf.Max(1, searchPointCount);

        if (currentSearchPoint > pointCount)
            currentSearchPoint = 1;

        float angle = 360f / pointCount * (currentSearchPoint - 1);
        float radians = angle * Mathf.Deg2Rad;

        Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * searchRadius;

        currentSearchTarget = searchCenter + offset;
    }

    private void MoveTowardsTarget()
    {
        Vector2 direction = currentSearchTarget - rb.position;

        if (direction.sqrMagnitude <= 0.001f)
        {
            StopMovement();
            return;
        }

        direction.Normalize();

        rb.linearVelocity = direction * searchMoveSpeed;

        UpdateFacingDirection(direction);
    }

    private void FinishSearch()
    {
        StopSearch();
        stateController.SetState(EnemyState.Undetected);
    }

    private void UpdateFacingDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) < 0.01f) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x);
        transform.localScale = scale;
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void StopSearch()
    {
        isSearching = false;
        isInvestigating = false;

        investigateTimer = 0f;
        searchTimer = 0f;
        currentSearchPoint = 0;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (detection == null) return;

        Vector2 center = detection.LastKnownPlayerPosition;

        Gizmos.DrawWireSphere(center, searchRadius);
        Gizmos.DrawSphere(center, 0.08f);
        Gizmos.DrawLine(transform.position, center);
        Gizmos.DrawSphere(currentSearchTarget, 0.08f);
    }
}