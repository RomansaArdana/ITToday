using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Base class abstrak untuk semua attack pattern Evil Inara.
/// Setiap attack memiliki 3 phase: Telegraph -> Attack -> Recovery.
///
/// Implementasi konkret:
///   - EvilInaraAttackGroundSlam  (Attack A)
///   - EvilInaraAttackProjectile  (Attack B)
///   - EvilInaraAttackSweep       (Attack C)
/// </summary>
public abstract class EvilInaraAttack : MonoBehaviour
{
    [Header("Attack Timing")]
    [SerializeField] protected float telegraphDuration = 1.5f;
    [SerializeField] protected float attackDuration = 0.8f;
    [SerializeField] protected float recoveryDuration = 0.5f;

    [Header("Damage")]
    [SerializeField] protected float sanityDamage = 25f;

    [Header("Debug")]
    [SerializeField] protected bool enableDebugLog = true;

    /// <summary>Dipanggil setelah seluruh fase attack (telegraph+attack+recovery) selesai.</summary>
    public event Action OnAttackFinished;

    protected Transform playerTransform;

    private void Awake()
    {
        OnAwake();
        CachePlayerTransform();
    }

    /// <summary>Override untuk melakukan setup tambahan di Awake.</summary>
    protected virtual void OnAwake() { }

    private void CachePlayerTransform()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogWarning($"[{GetType().Name}] Player tidak ditemukan di scene.", this);
    }

    // =========================================================
    // EXECUTION
    // =========================================================

    /// <summary>
    /// Eksekusi seluruh attack sequence.
    /// Dipanggil oleh EvilInaraAttackController via StartCoroutine.
    /// </summary>
    public IEnumerator ExecuteAttack()
    {
        Log($"{GetType().Name} dimulai");

        yield return TelegraphPhase();
        yield return AttackPhase();
        yield return RecoveryPhase();

        Log($"{GetType().Name} selesai");
        OnAttackFinished?.Invoke();
    }

    // =========================================================
    // PHASES
    // =========================================================

    /// <summary>Fase telegraph — tunjukkan indikator visual kepada player.</summary>
    protected virtual IEnumerator TelegraphPhase()
    {
        Log("Telegraph phase");
        OnTelegraphStart();
        yield return new WaitForSeconds(telegraphDuration);
        OnTelegraphEnd();
    }

    /// <summary>Fase attack utama — implementasikan di subclass.</summary>
    protected abstract IEnumerator AttackPhase();

    /// <summary>Fase recovery setelah attack.</summary>
    protected virtual IEnumerator RecoveryPhase()
    {
        Log("Recovery phase");
        yield return new WaitForSeconds(recoveryDuration);
    }

    // =========================================================
    // HOOKS
    // =========================================================

    /// <summary>Dipanggil saat telegraph dimulai. Override untuk menampilkan VFX.</summary>
    protected virtual void OnTelegraphStart() { }

    /// <summary>Dipanggil saat telegraph selesai. Override untuk menyembunyikan VFX.</summary>
    protected virtual void OnTelegraphEnd() { }

    // =========================================================
    // HELPER
    // =========================================================

    /// <summary>Cari SanityController dari player dan drain sanity.</summary>
    protected void DealSanityDamageToPlayer()
    {
        if (playerTransform == null) return;

        SanityController sanity = playerTransform.GetComponent<SanityController>();
        if (sanity != null)
        {
            sanity.DrainSanity(sanityDamage);
            Log($"Sanity damage: -{sanityDamage:F1}");
        }
    }

    protected void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[{GetType().Name}] {message}", this);
    }
}
