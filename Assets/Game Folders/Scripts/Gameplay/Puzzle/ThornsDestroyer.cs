using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Pasang script ini di GameObject sulur berduri.
/// - Memberikan damage (drain sanity) ke player saat menyentuh sulur
/// - Subscribe ke ShadowRaycastChecker.OnShadowConfirmed → jalankan animasi hancur
///
/// Setup:
/// 1. Assign shadowChecker di Inspector
/// 2. BoxCollider2D → Is Trigger: ✅
/// 3. Tag player harus "Player"
/// </summary>
public class ThornsDestroyer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShadowRaycastChecker shadowChecker;

    [Header("Damage Settings")]
    [Tooltip("Sanity yang dikurangi per detik saat player menyentuh sulur")]
    [SerializeField] private float sanityDamagePerSecond = 20f;

    [Header("Destroy Settings")]
    [Tooltip("Durasi animasi hancur dalam detik")]
    [SerializeField] private float destroyDuration = 1.2f;
    [Tooltip("Nama trigger Animator (kosongkan jika tidak pakai Animator)")]
    [SerializeField] private string animatorTriggerName = "Destroy";

    // ── Events ───────────────────────────────────────────────────────────────
    public event Action OnThornsDestroyed;

    // ── Components ───────────────────────────────────────────────────────────
    private Collider2D[] colliders;
    private SpriteRenderer[] renderers;
    private Animator animator;
    private bool isDestroying = false;

    // ── Damage State ─────────────────────────────────────────────────────────
    private SanityController playerSanity;
    private bool isPlayerTouching = false;

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (shadowChecker != null)
            shadowChecker.OnShadowConfirmed += HandleShadowConfirmed;
    }

    private void OnDisable()
    {
        if (shadowChecker != null)
            shadowChecker.OnShadowConfirmed -= HandleShadowConfirmed;
    }

    private void Update()
    {
        // Drain sanity terus-menerus selama player menyentuh sulur
        if (isPlayerTouching && playerSanity != null && !isDestroying)
        {
            playerSanity.DrainSanity(sanityDamagePerSecond * Time.deltaTime);
        }
    }

    // ── Trigger Damage ───────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroying) return;
        if (!other.CompareTag("Player")) return;

        playerSanity = other.GetComponentInParent<SanityController>();
        isPlayerTouching = true;

        Debug.Log("[ThornsDestroyer] Player menyentuh sulur! Sanity drain dimulai.");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerTouching = false;
        playerSanity = null;

        Debug.Log("[ThornsDestroyer] Player keluar dari sulur. Sanity drain berhenti.");
    }

    // ── Shadow Puzzle ────────────────────────────────────────────────────────

    private void HandleShadowConfirmed()
    {
        if (isDestroying) return;

        Debug.Log("[ThornsDestroyer] Sulur berduri mulai hancur!");
        StartCoroutine(DestroySequence());
    }

    private IEnumerator DestroySequence()
    {
        isDestroying = true;
        isPlayerTouching = false;

        // Nonaktifkan collider segera agar tidak berbahaya lagi
        foreach (Collider2D col in colliders)
            col.enabled = false;

        if (animator != null && !string.IsNullOrEmpty(animatorTriggerName))
        {
            animator.SetTrigger(animatorTriggerName);
            yield return new WaitForSeconds(destroyDuration);
        }
        else
        {
            yield return StartCoroutine(ScaleDownAndFade());
        }

        OnThornsDestroyed?.Invoke();
        Debug.Log("[ThornsDestroyer] Sulur hancur sepenuhnya.");
        gameObject.SetActive(false);
    }

    private IEnumerator ScaleDownAndFade()
    {
        Vector3 originalScale = transform.localScale;
        Color[] originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;

        float elapsed = 0f;

        while (elapsed < destroyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / destroyDuration;

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            for (int i = 0; i < renderers.Length; i++)
            {
                Color c = originalColors[i];
                c.a = Mathf.Lerp(1f, 0f, t);
                renderers[i].color = c;
            }

            yield return null;
        }
    }

    public void ResetThorns()
    {
        StopAllCoroutines();
        isDestroying = false;
        isPlayerTouching = false;
        playerSanity = null;
        gameObject.SetActive(true);
        transform.localScale = Vector3.one;

        foreach (Collider2D col in colliders)
            col.enabled = true;

        foreach (SpriteRenderer sr in renderers)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }
}
