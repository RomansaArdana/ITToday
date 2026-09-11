using System.Collections;
using UnityEngine;

/// <summary>
/// Root coordinator untuk boss Evil Inara.
/// Bertanggung jawab mengorkestrasi seluruh sub-sistem boss:
///   - EvilInaraHealth (HP)
///   - EvilInaraStateController (state machine)
///   - EvilInaraAttackController (attack cycle)
///   - EvilInaraPhaseController (phase progression)
///   - EvilInaraMinionSpawner (minion spawn)
///
/// TIDAK mengandung logika attack, state machine detail, atau AI.
/// Berfungsi sebagai "konduktor" yang menerima event dan mendelegasikan.
/// </summary>
public class EvilInaraBoss : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("Core References")]
    [SerializeField] private EvilInaraHealth health;
    [SerializeField] private EvilInaraStateController stateController;
    [SerializeField] private EvilInaraAttackController attackController;
    [SerializeField] private EvilInaraPhaseController phaseController;
    [SerializeField] private EvilInaraMinionSpawner minionSpawner;

    [Header("Timing")]
    [SerializeField] private float introDuration = 2f;
    [SerializeField] private float exhaustedDuration = 4f;
    [SerializeField] private float vulnerableDuration = 5f;
    [SerializeField] private float phaseTransitionDuration = 2f;
    [SerializeField] private float deathSequenceDuration = 3f;

    [Header("Story")]
    [SerializeField] private string bossDefeatedFlag = "EvilInara_Defeated";
    [SerializeField] private int nextChapter = 20;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    // =========================================================
    // LIFECYCLE
    // =========================================================

    private void Awake()
    {
        AutoAssignReferences();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnHPChanged += HandleHPChanged;
            health.OnDead += HandleBossDead;
        }

        if (attackController != null)
            attackController.OnExhaustTriggered += HandleAttackCycleExhausted;
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHPChanged -= HandleHPChanged;
            health.OnDead -= HandleBossDead;
        }

        if (attackController != null)
            attackController.OnExhaustTriggered -= HandleAttackCycleExhausted;
    }

    private void Start()
    {
        StartCoroutine(IntroSequence());
    }

    // =========================================================
    // INTRO
    // =========================================================

    private IEnumerator IntroSequence()
    {
        Log("Intro dimulai");
        stateController.SetState(EvilInaraBossState.Intro);

        yield return new WaitForSeconds(introDuration);

        EnterIdle();
    }

    // =========================================================
    // COMBAT CYCLE
    // =========================================================

    private void EnterIdle()
    {
        Log("-> Idle");
        stateController.SetState(EvilInaraBossState.Idle);

        if (attackController != null)
            attackController.StartAttackCycle();
    }

    /// <summary>
    /// Dipanggil oleh EvilInaraAttackController ketika boss telah
    /// menyelesaikan sejumlah attack dan memasuki kondisi exhausted.
    /// </summary>
    private void HandleAttackCycleExhausted()
    {
        StartCoroutine(ExhaustedSequence());
    }

    private IEnumerator ExhaustedSequence()
    {
        Log("-> Exhausted");
        stateController.SetState(EvilInaraBossState.Exhausted);

        yield return new WaitForSeconds(exhaustedDuration);

        Log("-> Vulnerable");
        stateController.SetState(EvilInaraBossState.Vulnerable);

        yield return new WaitForSeconds(vulnerableDuration);

        // Jika boss belum mati saat vulnerable window habis
        if (!stateController.IsDead)
        {
            Log("Vulnerable window habis -> kembali Idle");
            EnterIdle();
        }
    }

    // =========================================================
    // HP & PHASE
    // =========================================================

    /// <summary>
    /// Dipanggil oleh EvilInaraHealth.OnHPChanged setiap kali HP berkurang.
    /// </summary>
    private void HandleHPChanged(int newHP)
    {
        Log($"HP berubah -> {newHP}");

        if (!stateController.IsDead)
            StartCoroutine(PhaseTransitionSequence(newHP));
    }

    private IEnumerator PhaseTransitionSequence(int newHP)
    {
        Log($"-> PhaseTransition (HP={newHP})");
        stateController.SetState(EvilInaraBossState.PhaseTransition);

        // Hentikan attack cycle saat transisi
        if (attackController != null)
            attackController.StopAttackCycle();

        // Beri instruksi ke phase controller untuk spawn minion
        if (phaseController != null)
            phaseController.EvaluatePhase(newHP);

        yield return new WaitForSeconds(phaseTransitionDuration);

        if (!stateController.IsDead)
        {
            Log("Phase transition selesai -> kembali Idle");
            EnterIdle();
        }
    }

    // =========================================================
    // BOSS DEATH
    // =========================================================

    /// <summary>
    /// Dipanggil oleh EvilInaraHealth.OnDead ketika HP mencapai 0.
    /// </summary>
    private void HandleBossDead()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        Log("Boss mati -> Death sequence dimulai");

        // Hentikan seluruh aktivitas
        if (attackController != null)
            attackController.StopAttackCycle();

        if (minionSpawner != null)
            minionSpawner.StopAllSpawns();

        stateController.SetState(EvilInaraBossState.Dead);

        // Durasi untuk animasi purifikasi / VFX
        yield return new WaitForSeconds(deathSequenceDuration);

        SetStoryFlag();
    }

    private void SetStoryFlag()
    {
        if (StoryManager.Instance == null)
        {
            Debug.LogWarning("[EvilInaraBoss] StoryManager tidak ditemukan.", this);
            return;
        }

        StoryManager.Instance.SetFlag(bossDefeatedFlag);
        StoryManager.Instance.ChangeChapter(nextChapter);

        Log($"Story flag set: {bossDefeatedFlag} | Chapter -> {nextChapter}");
    }

    // =========================================================
    // SETUP
    // =========================================================

    private void AutoAssignReferences()
    {
        if (health == null) health = GetComponent<EvilInaraHealth>();
        if (stateController == null) stateController = GetComponent<EvilInaraStateController>();
        if (attackController == null) attackController = GetComponent<EvilInaraAttackController>();
        if (phaseController == null) phaseController = GetComponent<EvilInaraPhaseController>();
        if (minionSpawner == null) minionSpawner = GetComponent<EvilInaraMinionSpawner>();

        if (health == null) Debug.LogError("[EvilInaraBoss] EvilInaraHealth tidak ditemukan.", this);
        if (stateController == null) Debug.LogError("[EvilInaraBoss] EvilInaraStateController tidak ditemukan.", this);
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[EvilInaraBoss] {message}", this);
    }

    [ContextMenu("Debug: Force Vulnerable")]
    private void DebugForceVulnerable()
    {
        StopAllCoroutines();
        stateController.SetState(EvilInaraBossState.Vulnerable);
    }

    [ContextMenu("Debug: Force Take Hit")]
    private void DebugForceTakeHit()
    {
        if (health != null) health.TakeHit();
    }
}
