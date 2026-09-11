using System.Collections;
using UnityEngine;

/// <summary>
/// Attack B — Dark Projectile.
/// Boss menembakkan projectile ke arah posisi player saat ini.
/// Player harus menggunakan cover atau berpindah posisi untuk menghindari.
/// </summary>
public class EvilInaraAttackProjectile : EvilInaraAttack
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float spreadAngle = 15f;

    [Header("Telegraph")]
    [SerializeField] private GameObject chargingVFX;

    protected override void OnTelegraphStart()
    {
        if (chargingVFX != null)
            chargingVFX.SetActive(true);
    }

    protected override void OnTelegraphEnd()
    {
        if (chargingVFX != null)
            chargingVFX.SetActive(false);
    }

    protected override IEnumerator AttackPhase()
    {
        Log($"Dark Projectile — fire! ({projectileCount} projectile)");

        FireProjectiles();

        yield return new WaitForSeconds(attackDuration);
    }

    private void FireProjectiles()
    {
        if (projectilePrefab == null || playerTransform == null) return;

        Vector3 spawnPos = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position;

        Vector2 baseDirection = ((Vector2)(playerTransform.position - spawnPos)).normalized;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = 0f;
            if (projectileCount > 1)
                angle = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, (float)i / (projectileCount - 1));

            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * baseDirection;

            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = dir * projectileSpeed;

            // Assign sanity damage jika projectile memiliki component damage
            BossProjectileDamage damage = proj.GetComponent<BossProjectileDamage>();
            if (damage != null)
                damage.SetDamage(sanityDamage);
        }
    }
}
