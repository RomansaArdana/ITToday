using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// State Machine utama untuk Final Boss (Evil Inara).
/// Mengatur siklus: IDLE -> ATTACKING -> EXHAUSTED -> RECOVERING -> IDLE
/// </summary>
public class EvilInaraBossController : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Attacking,
        Exhausted,
        Recovering,
        Defeated
    }

    [Header("Boss Stats")]
    [SerializeField] private int maxHP = 6;
    [SerializeField] private float idleDuration = 1.5f;
    [SerializeField] private float exhaustedDuration = 3f;
    [SerializeField] private float recoveringDuration = 2f;

    [Header("References")]
    [SerializeField] private BossAttackController attackController;
    [SerializeField] private SpriteRenderer bossRenderer;

    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color exhaustedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color recoveringColor = new Color(0.8f, 0.4f, 0.8f, 1f);

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    // --- Public State ---
    public BossState CurrentState { get; private set; } = BossState.Idle;
    public int CurrentHP { get; private set; }
    public bool IsExhausted => CurrentState == BossState.Exhausted;
    public bool IsDefeated => CurrentState == BossState.Defeated;

    // --- Events ---
    public event Action<int> OnHPChanged;       // Dipanggil tiap HP turun, kirim HP baru
    public event Action OnBossDefeated;         // Dipanggil saat HP = 0
    public event Action<BossState> OnStateChanged;

    private Coroutine stateCoroutine;

    private void Awake()
    {
        attackController ??= GetComponent<BossAttackController>();
        bossRenderer ??= GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        CurrentHP = maxHP;
        TransitionTo(BossState.Idle);
    }

    // ─────────────────────────────────────────
    // State Machine
    // ─────────────────────────────────────────

    public void TransitionTo(BossState newState)
    {
        if (CurrentState == BossState.Defeated) return;

        if (stateCoroutine != null)
            StopCoroutine(stateCoroutine);

        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
        Log($"State: {newState}");

        UpdateVisual();

        stateCoroutine = newState switch
        {
            BossState.Idle       => StartCoroutine(StateIdle()),
            BossState.Attacking  => StartCoroutine(StateAttacking()),
            BossState.Exhausted  => StartCoroutine(StateExhausted()),
            BossState.Recovering => StartCoroutine(StateRecovering()),
            BossState.Defeated   => StartCoroutine(StateDefeated()),
            _                    => null
        };
    }

    private IEnumerator StateIdle()
    {
        Log("Idle — menunggu sebelum menyerang...");
        yield return new WaitForSeconds(idleDuration);
        TransitionTo(BossState.Attacking);
    }

    private IEnumerator StateAttacking()
    {
        Log("Attacking — melancarkan serangan...");

        if (attackController != null)
            yield return StartCoroutine(attackController.PerformAttack());
        else
            yield return new WaitForSeconds(2f);

        TransitionTo(BossState.Exhausted);
    }

    private IEnumerator StateExhausted()
    {
        Log($"Exhausted — window {exhaustedDuration}s. Lentera bisa melukai bos!");
        yield return new WaitForSeconds(exhaustedDuration);

        // Jika belum terkena hit Lentera, kembali menyerang
        if (CurrentState == BossState.Exhausted)
            TransitionTo(BossState.Idle);
    }

    private IEnumerator StateRecovering()
    {
        Log("Recovering — bos bangkit kembali...");
        yield return new WaitForSeconds(recoveringDuration);
        TransitionTo(BossState.Idle);
    }

    private IEnumerator StateDefeated()
    {
        Log("DEFEATED — memicu event kemenangan!");
        OnBossDefeated?.Invoke();
        yield return null;
    }

    // ─────────────────────────────────────────
    // Damage (Dipanggil oleh EvilInaraBossHealth)
    // ─────────────────────────────────────────

    /// <summary>
    /// Dipanggil oleh EvilInaraBossHealth setelah validasi IsExhausted.
    /// </summary>
    public void ReceivePurification()
    {
        if (CurrentHP <= 0 || CurrentState == BossState.Defeated) return;

        CurrentHP--;
        Log($"Terkena Lentera! HP: {CurrentHP + 1} -> {CurrentHP}");
        OnHPChanged?.Invoke(CurrentHP);

        if (CurrentHP <= 0)
        {
            TransitionTo(BossState.Defeated);
            return;
        }

        TransitionTo(BossState.Recovering);
    }

    // ─────────────────────────────────────────
    // Visual
    // ─────────────────────────────────────────

    private void UpdateVisual()
    {
        if (bossRenderer == null) return;

        bossRenderer.color = CurrentState switch
        {
            BossState.Exhausted  => exhaustedColor,
            BossState.Recovering => recoveringColor,
            _                    => normalColor
        };
    }

    // ─────────────────────────────────────────
    // Utility
    // ─────────────────────────────────────────

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[BossController] {message}", this);
    }
}
