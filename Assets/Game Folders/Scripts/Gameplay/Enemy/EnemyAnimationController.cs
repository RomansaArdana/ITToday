using UnityEngine;

/// <summary>
/// Mengontrol animasi enemy berdasarkan EnemyState dan EnemyArchetype.
///
/// SETUP DI UNITY INSPECTOR:
/// 1. Tambahkan component ini ke prefab enemy.
/// 2. Assign reference Animator ke slot "animator".
/// 3. Buat Animator Controller dengan parameter berikut:
///    - IsIdle  (bool)    ? animasi idle looping
///    - Die     (trigger) ? animasi mati (all archetypes)
///    - Dodge   (trigger) ? animasi dodge (Cyan only)
///
/// MAPPING STATE ? ANIMASI:
///   Red    : Idle, Die
///   Blue   : Idle, Die
///   Cyan   : Idle, Dodge, Die
///   Purple : Idle, Die
/// </summary>
public class EnemyAnimationController : MonoBehaviour
{
    // =========================================================
    // ANIMATOR PARAMETER NAMES
    // =========================================================

    private static readonly string PARAM_IS_IDLE = "IsIdle";
    private static readonly string PARAM_DIE = "Die";
    private static readonly string PARAM_DODGE = "Dodge";

    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyStateController stateController;
    [SerializeField] private EnemyBehaviourController behaviourController;

    [Header("Cyan Only")]
    [Tooltip("Hanya dibutuhkan untuk Enemy Cyan (animasi Dodge).")]
    [SerializeField] private EnemyDash dash;

    // =========================================================
    // INTERNAL STATE
    // =========================================================

    private bool wasDodging;

    // =========================================================
    // UNITY LIFECYCLE
    // =========================================================

    private void Awake()
    {
        animator ??= GetComponentInChildren<Animator>();
        stateController ??= GetComponent<EnemyStateController>();
        behaviourController ??= GetComponent<EnemyBehaviourController>();
        dash ??= GetComponent<EnemyDash>();
    }

    private void OnEnable()
    {
        if (stateController != null)
            stateController.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (stateController != null)
            stateController.OnStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        // Inisialisasi animasi sesuai state awal.
        if (stateController != null)
            ApplyAnimation(stateController.CurrentState);
    }

    private void Update()
    {
        // Dodge hanya relevan untuk Cyan.
        // Kita pantau EnemyDash.IsEvading karena dodge bukan sebuah EnemyState,
        // melainkan sub-action di dalam state Chase.
        if (behaviourController == null) return;
        if (behaviourController.Archetype != EnemyArchetype.Cyan) return;
        if (dash == null) return;

        bool isDodging = dash.IsEvading;

        if (isDodging && !wasDodging)
            TriggerDodge();

        wasDodging = isDodging;
    }

    // =========================================================
    // STATE HANDLER
    // =========================================================

    private void HandleStateChanged(EnemyState previousState, EnemyState newState)
    {
        ApplyAnimation(newState);
    }

    private void ApplyAnimation(EnemyState state)
    {
        if (animator == null) return;

        switch (state)
        {
            case EnemyState.Dead:
                SetIdle(false);
                TriggerDie();
                break;

            // Semua state lain ? Idle
            // (Red, Blue, Cyan, Purple tidak punya animasi walk/run yang berbeda)
            default:
                SetIdle(true);
                break;
        }
    }

    // =========================================================
    // ANIMATOR HELPERS
    // =========================================================

    private void SetIdle(bool value)
    {
        if (animator == null) return;
        animator.SetBool(PARAM_IS_IDLE, value);
    }

    private void TriggerDie()
    {
        if (animator == null)
        {
            Debug.LogError("[EnemyAnim] Animator NULL saat TriggerDie!", this);
            return;
        }

        if (!animator.enabled)
        {
            Debug.LogError("[EnemyAnim] Animator DISABLED saat TriggerDie!", this);
            return;
        }

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[EnemyAnim] TriggerDie fired! State sekarang: '{info.shortNameHash}' | IsInTransition: {animator.IsInTransition(0)}", this);

        animator.SetTrigger(PARAM_DIE);

        // Cek setelah trigger dikirim (next frame tidak bisa dicek langsung,
        // tapi ini memastikan trigger benar-benar di-set)
        Debug.Log($"[EnemyAnim] Trigger '{PARAM_DIE}' dikirim ke Animator '{animator.runtimeAnimatorController?.name}'", this);
    }


    private void TriggerDodge()
    {
        if (animator == null) return;
        animator.SetTrigger(PARAM_DODGE);
    }
}
