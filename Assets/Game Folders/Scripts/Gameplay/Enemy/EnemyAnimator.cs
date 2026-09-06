using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private static readonly int ParamTriggerDie = Animator.StringToHash("Die");
    private static readonly int ParamTriggerDodge = Animator.StringToHash("Dodge");

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyStateController stateController;
    [SerializeField] private EnemyController enemyController;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private void Awake()
    {
        stateController ??= GetComponent<EnemyStateController>();
        enemyController ??= GetComponent<EnemyController>();
        animator ??= GetComponentInChildren<Animator>(true);

        if (animator == null)
            Debug.LogError("[EnemyAnimator] Animator tidak ditemukan.", this);

        if (stateController == null)
            Debug.LogError("[EnemyAnimator] EnemyStateController tidak ditemukan.", this);

        if (enemyController == null)
            Debug.LogError("[EnemyAnimator] EnemyController tidak ditemukan.", this);

        if (!enableDebugLog || animator == null) return;

        Debug.Log(
            $"[EnemyAnimator] Initialized | State: {stateController?.CurrentState} | Animator: {animator.gameObject.name} | Controller: {animator.runtimeAnimatorController?.name}",
            this
        );
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

    private void HandleStateChanged(EnemyState previousState, EnemyState newState)
    {
        if (enableDebugLog)
            Debug.Log($"[EnemyAnimator] State Changed: {previousState} → {newState}", this);

        if (newState == EnemyState.Dead)
            TriggerDeath();
    }

    public void TriggerDeath()
    {
        if (animator == null) return;

        animator.SetTrigger(ParamTriggerDie);

        if (!enableDebugLog) return;

        string enemyType = enemyController != null &&
                           enemyController.Stats != null
            ? enemyController.Stats.Archetype.ToString()
            : gameObject.name;

        Debug.Log(
            $"[EnemyAnimator] {enemyType} → DIE | Animator: {animator.gameObject.name} | Controller: {animator.runtimeAnimatorController?.name}",
            this
        );
    }

    public void TriggerDodge()
    {
        if (animator == null) return;
        if (stateController != null && stateController.IsDead) return;

        if (enemyController != null &&
            enemyController.Stats != null &&
            enemyController.Stats.Archetype != EnemyArchetype.Cyan)
            return;

        animator.SetTrigger(ParamTriggerDodge);

        if (enableDebugLog)
            Debug.Log($"[EnemyAnimator] CYAN → DODGE | Animator: {animator.gameObject.name}", this);
    }
}