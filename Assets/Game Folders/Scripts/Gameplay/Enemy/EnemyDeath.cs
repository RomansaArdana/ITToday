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

    [Header("Death")]
    [SerializeField] private bool disableCollidersOnDeath = true;
    [SerializeField] private bool makeRigidbodyKinematicOnDeath = true;

    [Header("Fade Out")]
    [SerializeField] private bool fadeOutOnDeath = true;
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private bool destroyAfterFade = true;

    private Collider2D[] colliders;
    private SpriteRenderer[] spriteRenderers;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        health ??= GetComponent<EnemyHealth>();
        stateController ??= GetComponent<EnemyStateController>();
        behaviourController ??= GetComponent<EnemyBehaviourController>();
        rb ??= GetComponent<Rigidbody2D>();

        colliders =
            GetComponentsInChildren<Collider2D>(true);

        spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        SetDeadState();
        StopBehaviour();
        StopMovement();

        if (disableCollidersOnDeath)
        {
            DisableColliders();
        }

        if (makeRigidbodyKinematicOnDeath)
        {
            DisablePhysics();
        }

        if (fadeOutOnDeath)
        {
            StartCoroutine(FadeOutAndCleanup());
        }
        else if (destroyAfterFade)
        {
            Destroy(gameObject);
        }

        Debug.Log(
            $"[EnemyDeath] {gameObject.name} entered Dead state.",
            this
        );
    }

    private void SetDeadState()
    {
        if (stateController == null)
        {
            return;
        }

        stateController.SetState(
            EnemyState.Dead
        );
    }

    private void StopBehaviour()
    {
        if (behaviourController == null)
        {
            return;
        }

        behaviourController.StopAllBehaviours();
    }

    private void StopMovement()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void DisablePhysics()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void DisableColliders()
    {
        if (colliders == null)
        {
            return;
        }

        foreach (Collider2D col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }

    private IEnumerator FadeOutAndCleanup()
    {
        float duration =
            Mathf.Max(0f, fadeDuration);

        if (duration <= 0f)
        {
            SetSpriteAlpha(0f);

            if (destroyAfterFade)
            {
                Destroy(gameObject);
            }

            yield break;
        }

        float elapsed = 0f;

        float[] startAlpha =
            new float[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                startAlpha[i] =
                    spriteRenderers[i].color.a;
            }
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / duration
                );

            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    progress
                );

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] == null)
                {
                    continue;
                }

                Color color =
                    spriteRenderers[i].color;

                color.a =
                    startAlpha[i] * alpha;

                spriteRenderers[i].color = color;
            }

            yield return null;
        }

        SetSpriteAlpha(0f);

        if (destroyAfterFade)
        {
            Destroy(gameObject);
        }
    }

    private void SetSpriteAlpha(float alpha)
    {
        if (spriteRenderers == null)
        {
            return;
        }

        alpha = Mathf.Clamp01(alpha);

        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer == null)
            {
                continue;
            }

            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }
}