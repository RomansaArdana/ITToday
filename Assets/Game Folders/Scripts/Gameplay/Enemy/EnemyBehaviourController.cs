using UnityEngine;

public class EnemyBehaviourController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStateController stateController;
    [SerializeField] private EnemyDetection detection;
    [SerializeField] private EnemyPatrol patrol;
    [SerializeField] private EnemySearch search;
    [SerializeField] private EnemyChase chase;

    private EnemyDetectionState previousState;

    private void Awake()
    {
        stateController ??= GetComponent<EnemyStateController>();
        detection ??= GetComponent<EnemyDetection>();
        patrol ??= GetComponent<EnemyPatrol>();
        search ??= GetComponent<EnemySearch>();
        chase ??= GetComponent<EnemyChase>();
    }

    private void Start()
    {
        if (detection == null) return;

        previousState = detection.CurrentState;
        UpdateBehaviour(previousState);
    }

    private void Update()
    {
        if (detection == null) return;

        EnemyDetectionState currentState = detection.CurrentState;

        if (currentState == previousState) return;

        previousState = currentState;
        UpdateBehaviour(currentState);
    }

    private void UpdateBehaviour(EnemyDetectionState state)
    {
        StopAllBehaviours();

        switch (state)
        {
            case EnemyDetectionState.Undetected:
                EnablePatrol();
                break;

            case EnemyDetectionState.Suspicious:
                EnableSearch();
                break;

            case EnemyDetectionState.Detected:
                EnableChase();
                break;
        }
    }

    private void EnablePatrol()
    {
        if (patrol == null) return;

        patrol.enabled = true;
    }

    private void EnableSearch()
    {
        if (search == null) return;

        search.enabled = true;
    }

    private void EnableChase()
    {
        if (chase == null) return;

        chase.enabled = true;
    }

    private void StopAllBehaviours()
    {
        if (patrol != null)
        {
            patrol.StopPatrol();
            patrol.enabled = false;
        }

        if (search != null)
        {
            search.StopSearch();
            search.enabled = false;
        }

        if (chase != null)
        {
            chase.StopChase();
            chase.enabled = false;
        }
    }
}