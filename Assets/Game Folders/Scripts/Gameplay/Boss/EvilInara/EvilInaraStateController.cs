using System;
using UnityEngine;

/// <summary>
/// Menyimpan dan mengelola state machine boss Evil Inara.
/// Meng-expose property boolean untuk query state saat ini.
/// Tidak mengandung logika transisi — itu tugas EvilInaraBoss.
/// </summary>
public class EvilInaraStateController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    public EvilInaraBossState CurrentState { get; private set; }

    public bool IsIntro         => CurrentState == EvilInaraBossState.Intro;
    public bool IsIdle          => CurrentState == EvilInaraBossState.Idle;
    public bool IsTelegraph     => CurrentState == EvilInaraBossState.Telegraph;
    public bool IsAttacking     => CurrentState == EvilInaraBossState.Attacking;
    public bool IsRecovery      => CurrentState == EvilInaraBossState.Recovery;
    public bool IsExhausted     => CurrentState == EvilInaraBossState.Exhausted;
    public bool IsVulnerable    => CurrentState == EvilInaraBossState.Vulnerable;
    public bool IsPhaseTransition => CurrentState == EvilInaraBossState.PhaseTransition;
    public bool IsDead          => CurrentState == EvilInaraBossState.Dead;

    /// <summary>True selama boss tidak dapat menerima damage dari Lantern.</summary>
    public bool IsInvulnerable  => !IsVulnerable && !IsDead;

    /// <summary>Dipanggil setiap kali state berubah.</summary>
    public event Action<EvilInaraBossState> OnStateChanged;

    private void Awake()
    {
        CurrentState = EvilInaraBossState.Intro;
    }

    /// <summary>
    /// Berpindah ke state baru.
    /// Tidak melakukan apapun jika state sudah sama.
    /// </summary>
    public void SetState(EvilInaraBossState newState)
    {
        if (CurrentState == newState) return;

        EvilInaraBossState previous = CurrentState;
        CurrentState = newState;

        Log($"{previous} -> {CurrentState}");
        OnStateChanged?.Invoke(CurrentState);
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[EvilInaraState] {message}", this);
    }
}
