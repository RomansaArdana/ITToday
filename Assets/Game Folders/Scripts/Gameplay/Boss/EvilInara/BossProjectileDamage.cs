using UnityEngine;

/// <summary>
/// Component pada projectile boss.
/// Membawa informasi damage dan mengenai player/cover saat kontak.
/// </summary>
public class BossProjectileDamage : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float lifetime = 5f;

    public float Damage => damage;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDamage(float amount)
    {
        damage = amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kena player -> sanity damage
        SanityController sanity = other.GetComponentInParent<SanityController>();
        if (sanity != null)
        {
            sanity.DrainSanity(damage);
            Destroy(gameObject);
            return;
        }

        // Kena cover -> cover damage (ditangani oleh BossCoverObject.OnTriggerEnter2D)
        BossCoverObject cover = other.GetComponent<BossCoverObject>();
        if (cover != null)
        {
            Destroy(gameObject);
        }
    }
}
