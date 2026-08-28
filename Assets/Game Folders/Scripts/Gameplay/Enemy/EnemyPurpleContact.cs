using UnityEngine;

public class EnemyPurpleContact : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private EnemyPurpleJumpscare jumpscare;

    [Header("Contact Damage")]
    [SerializeField] private float sanityDamage = 30f;
    [SerializeField] private float contactCooldown = 1.5f;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private float cooldownTimer;

    private void Awake()
    {
        enemyController ??=
            GetComponent<EnemyController>();

        enemyHealth ??=
            GetComponent<EnemyHealth>();

        jumpscare ??=
            GetComponent<EnemyPurpleJumpscare>();
    }

    private void Update()
    {
        if (cooldownTimer <= 0f)
        {
            cooldownTimer = 0f;
            return;
        }

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer < 0f)
        {
            cooldownTimer = 0f;
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        TryDamagePlayer(
            other.gameObject
        );
    }

    private void TryDamagePlayer(
        GameObject player)
    {
        if (cooldownTimer > 0f)
        {
            return;
        }

        if (enemyHealth != null &&
            !enemyHealth.IsAlive)
        {
            return;
        }

        SanityController sanity =
            player.GetComponent<SanityController>();

        if (sanity == null)
        {
            sanity =
                player.GetComponentInParent<SanityController>();
        }

        if (sanity == null)
        {
            if (enableDebugLog)
            {
                Debug.LogWarning(
                    "[Purple] SanityController tidak ditemukan pada Player.",
                    this
                );
            }

            return;
        }

        if (sanity.IsDepleted)
        {
            return;
        }

        float previousSanity =
            sanity.CurrentSanity;

        sanity.DrainSanity(
            sanityDamage
        );

        cooldownTimer =
            Mathf.Max(
                0f,
                contactCooldown
            );

        if (jumpscare != null)
        {
            jumpscare.PlayJumpscare();
        }

        if (enableDebugLog)
        {
            Debug.Log(
                $"[Purple] JUMPSCARE → " +
                $"Sanity {previousSanity:F1} → " +
                $"{sanity.CurrentSanity:F1} " +
                $"(-{sanityDamage:F1})",
                this
            );
        }
    }

    public void ResetCooldown()
    {
        cooldownTimer = 0f;
    }
}