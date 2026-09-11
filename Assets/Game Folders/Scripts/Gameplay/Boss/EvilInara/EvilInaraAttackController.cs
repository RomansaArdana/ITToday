using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mengelola siklus attack Evil Inara.
/// Memilih attack, menjalankannya secara sequential,
/// dan memberitahu EvilInaraBoss ketika boss sudah lelah (Exhausted).
///
/// Flow:
///   EvilInaraBoss.EnterIdle()
///       -> StartAttackCycle()
///       -> Pilih attack dari list
///       -> ExecuteAttack()
///       -> attackCount++
///       -> Jika attackCount >= attacksBeforeExhaust: OnExhaustTriggered
///       -> Else: pilih attack berikutnya setelah cooldown
/// </summary>
public class EvilInaraAttackController : MonoBehaviour
{
    [Header("Attack List")]
    [SerializeField] private List<EvilInaraAttack> attacks = new List<EvilInaraAttack>();

    [Header("Cycle Settings")]
    [SerializeField] private int attacksBeforeExhaust = 3;
    [SerializeField] private float delayBetweenAttacks = 1.5f;
    [SerializeField] private bool randomizeOrder = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private int attackCount;
    private int lastAttackIndex = -1;
    private Coroutine attackCoroutine;
    private bool isRunning;

    /// <summary>Dipanggil oleh EvilInaraBoss untuk memulai exhaust + vulnerable sequence.</summary>
    public event Action OnExhaustTriggered;

    // =========================================================
    // PUBLIC API
    // =========================================================

    /// <summary>
    /// Memulai attack cycle dari awal.
    /// Dipanggil oleh EvilInaraBoss saat state berubah ke Idle.
    /// </summary>
    public void StartAttackCycle()
    {
        if (attacks == null || attacks.Count == 0)
        {
            Log("Tidak ada attack yang tersedia.");
            return;
        }

        isRunning = true;
        attackCount = 0;

        attackCoroutine = StartCoroutine(AttackCycleRoutine());
    }

    /// <summary>
    /// Menghentikan attack cycle.
    /// Dipanggil saat boss memasuki PhaseTransition atau Dead.
    /// </summary>
    public void StopAttackCycle()
    {
        isRunning = false;

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        Log("Attack cycle dihentikan.");
    }

    // =========================================================
    // ATTACK CYCLE
    // =========================================================

    private IEnumerator AttackCycleRoutine()
    {
        Log($"Attack cycle dimulai | attacksBeforeExhaust={attacksBeforeExhaust}");

        while (isRunning)
        {
            EvilInaraAttack selectedAttack = SelectNextAttack();

            if (selectedAttack == null)
            {
                Log("Tidak ada attack valid — cycle berhenti.");
                yield break;
            }

            Log($"Attack ke-{attackCount + 1}: {selectedAttack.GetType().Name}");

            bool attackFinished = false;
            selectedAttack.OnAttackFinished += () => attackFinished = true;

            StartCoroutine(selectedAttack.ExecuteAttack());

            // Tunggu sampai attack selesai
            yield return new WaitUntil(() => attackFinished);

            attackCount++;

            Log($"Attack selesai | count={attackCount}/{attacksBeforeExhaust}");

            if (attackCount >= attacksBeforeExhaust)
            {
                Log("Batas attack tercapai -> Exhaust!");
                isRunning = false;
                OnExhaustTriggered?.Invoke();
                yield break;
            }

            // Delay sebelum attack berikutnya
            yield return new WaitForSeconds(delayBetweenAttacks);
        }
    }

    // =========================================================
    // ATTACK SELECTION
    // =========================================================

    private EvilInaraAttack SelectNextAttack()
    {
        if (attacks == null || attacks.Count == 0) return null;

        // Hapus null entries
        attacks.RemoveAll(a => a == null);
        if (attacks.Count == 0) return null;

        if (!randomizeOrder)
        {
            int index = attackCount % attacks.Count;
            return attacks[index];
        }

        // Random tapi hindari repeat attack yang sama
        if (attacks.Count == 1)
            return attacks[0];

        int nextIndex;
        do { nextIndex = UnityEngine.Random.Range(0, attacks.Count); }
        while (nextIndex == lastAttackIndex);

        lastAttackIndex = nextIndex;
        return attacks[nextIndex];
    }

    // =========================================================
    // DEBUG
    // =========================================================

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[EvilInaraAttackController] {message}", this);
    }
}
