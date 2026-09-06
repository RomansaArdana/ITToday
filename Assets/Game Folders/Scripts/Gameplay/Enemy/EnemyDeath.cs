using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDeath : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth health;
    [SerializeField] private EnemyStateController stateController;
    [SerializeField] private EnemyBehaviourController behaviourController;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    [Header("Death")]
    [SerializeField] private bool disableCollidersOnDeath = true;
    [SerializeField] private bool makeRigidbodyKinematicOnDeath = true;

    [Header("Fallback")]
    [Tooltip("Digunakan hanya jika Animator tidak ditemukan atau state Die tidak berhasil dimainkan.")]
    [SerializeField, Min(0.1f)] private float fallbackDestroyDelay = 1.5f;

    private Collider2D[] colliders;
    private Coroutine deathCoroutine;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        health ??= GetComponent<EnemyHealth>();
        stateController ??= GetComponent<EnemyStateController>();
        behaviourController ??= GetComponent<EnemyBehaviourController>();
        rb ??= GetComponent<Rigidbody2D>();
        animator ??= GetComponentInChildren<Animator>(true);

        colliders = GetComponentsInChildren<Collider2D>(true);

        if (animator == null)
            Debug.LogWarning("[EnemyDeath] Animator tidak ditemukan.", this);
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (IsDead) return;

        IsDead = true;

        SetDeadState();
        StopBehaviour();
        StopMovement();

        if (disableCollidersOnDeath)
            DisableColliders();

        if (makeRigidbodyKinematicOnDeath)
            DisablePhysics();

        if (deathCoroutine != null)
            StopCoroutine(deathCoroutine);

        deathCoroutine = StartCoroutine(WaitForDeathAnimation());

        Debug.Log($"[EnemyDeath] {gameObject.name} entered Dead state.", this);
    }

    private IEnumerator WaitForDeathAnimation()
    {
        if (animator == null)
        {
            yield return new WaitForSeconds(fallbackDestroyDelay);
            Destroy(gameObject);
            yield break;
        }

        float timeout = fallbackDestroyDelay;
        float elapsed = 0f;
        bool dieStateFound = false;

        while (elapsed < timeout)
        {
            elapsed += Time.deltaTime;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName("Red_Die") ||
                stateInfo.IsName("Blue_Die") ||
                stateInfo.IsName("Cyan_Die") ||
                stateInfo.IsName("Purple_Die"))
            {
                dieStateFound = true;

                if (!animator.IsInTransition(0) && stateInfo.normalizedTime >= 1f)
                    break;
            }

            yield return null;
        }

        if (!dieStateFound)
            Debug.LogWarning("[EnemyDeath] Die animation state tidak ditemukan. Menggunakan fallback destroy delay.", this);

        Destroy(gameObject);
    }

    private void SetDeadState()
    {
        if (stateController == null) return;

        stateController.SetState(EnemyState.Dead);
    }

    private void StopBehaviour()
    {
        if (behaviourController == null) return;

        behaviourController.StopAllBehaviours();
    }

    private void StopMovement()
    {
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void DisablePhysics()
    {
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void DisableColliders()
    {
        if (colliders == null) return;

        foreach (Collider2D col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }
    }
}