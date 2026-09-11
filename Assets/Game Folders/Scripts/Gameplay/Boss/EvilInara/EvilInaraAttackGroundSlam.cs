using System.Collections;
using UnityEngine;

/// <summary>
/// Attack A — Ground Slam.
/// Boss mengangkat tangan, telegraph dengan indikator area di lantai,
/// lalu membanting ke bawah menghasilkan shockwave horizontal.
/// Player harus keluar dari area shockwave atau berlindung di cover.
/// </summary>
public class EvilInaraAttackGroundSlam : EvilInaraAttack
{
    [Header("Ground Slam")]
    [SerializeField] private GameObject telegraphIndicator;
    [SerializeField] private GameObject shockwavePrefab;
    [SerializeField] private Transform shockwaveSpawnPoint;
    [SerializeField] private float shockwaveRadius = 5f;
    [SerializeField] private LayerMask playerLayer;

    protected override void OnTelegraphStart()
    {
        if (telegraphIndicator != null)
            telegraphIndicator.SetActive(true);
    }

    protected override void OnTelegraphEnd()
    {
        if (telegraphIndicator != null)
            telegraphIndicator.SetActive(false);
    }

    protected override IEnumerator AttackPhase()
    {
        Log("Ground Slam — impact!");

        SpawnShockwave();
        CheckDirectHit();

        yield return new WaitForSeconds(attackDuration);
    }

    private void SpawnShockwave()
    {
        if (shockwavePrefab == null) return;

        Vector3 spawnPos = shockwaveSpawnPoint != null
            ? shockwaveSpawnPoint.position
            : transform.position;

        Instantiate(shockwavePrefab, spawnPos, Quaternion.identity);
    }

    private void CheckDirectHit()
    {
        // Jika player dalam radius dekat saat slam — sanity damage langsung
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= shockwaveRadius)
            DealSanityDamageToPlayer();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.4f);
        Vector3 center = shockwaveSpawnPoint != null ? shockwaveSpawnPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, shockwaveRadius);
    }
}
