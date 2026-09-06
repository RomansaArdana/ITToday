using System;
using UnityEngine;

public class EnemyStateController : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private EnemyState currentState = EnemyState.Undetected;

    public EnemyState CurrentState => currentState;

    public bool IsIdle => currentState == EnemyState.Idle;
    public bool IsPatrol => currentState == EnemyState.Patrol;
    public bool IsSuspicious => currentState == EnemyState.Suspicious;
    public bool IsSearch => currentState == EnemyState.Search;
    public bool IsChase => currentState == EnemyState.Chase;
    public bool IsFlee => currentState == EnemyState.Flee;
    public bool IsPressure => currentState == EnemyState.Pressure;
    public bool IsCornered => currentState == EnemyState.Cornered;
    public bool IsVulnerable => currentState == EnemyState.Vulnerable;
    public bool IsDetected => currentState == EnemyState.Detected;
    public bool IsUndetected => currentState == EnemyState.Undetected;
    public bool IsDead => currentState == EnemyState.Dead;
    public bool CanBehave => currentState != EnemyState.Dead;

    public event Action<EnemyState, EnemyState> OnStateChanged;

    public void SetState(EnemyState newState)
    {
        if (currentState == newState) return;
        if (currentState == EnemyState.Dead) return;

        EnemyState previousState = currentState;
        currentState = newState;

        NotifyStateChanged(previousState, newState);
    }

    private void NotifyStateChanged(EnemyState previousState, EnemyState newState)
    {
        OnStateChanged?.Invoke(previousState, newState);
    }
}