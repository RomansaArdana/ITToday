using System.Collections;
using UnityEngine;

/// <summary>
/// Attack C — Horizontal Sweep.
/// Boss menyapu seluruh area secara horizontal.
/// Player harus crouch atau berlindung di balik cover untuk menghindari.
/// Ini adalah attack yang menguji kemampuan PlayerCrouch.
/// </summary>
public class EvilInaraAttackSweep : EvilInaraAttack
{
    [Header("Sweep")]
    [SerializeField] private float sweepHeight = 1.5f;
    [SerializeField] private float sweepWidth = 20f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GameObject sweepVFX;

    [Header("Telegraph")]
    [SerializeField] private GameObject sweepIndicator;

    protected override void OnTelegraphStart()
    {
        if (sweepIndicator != null)
            sweepIndicator.SetActive(true);
    }

    protected override void OnTelegraphEnd()
    {
        if (sweepIndicator != null)
            sweepIndicator.SetActive(false);
    }

    protected override IEnumerator AttackPhase()
    {
        Log("Horizontal Sweep!");

        if (sweepVFX != null)
            sweepVFX.SetActive(true);

        ExecuteSweep();

        yield return new WaitForSeconds(attackDuration);

        if (sweepVFX != null)
            sweepVFX.SetActive(false);
    }

    private void ExecuteSweep()
    {
        // OverlapBox horizontal di ketinggian sweepHeight dari ground
        Vector2 sweepCenter = new Vector2(transform.position.x, transform.position.y + sweepHeight / 2f);
        Vector2 sweepSize = new Vector2(sweepWidth, sweepHeight);

        Collider2D[] hits = Physics2D.OverlapBoxAll(sweepCenter, sweepSize, 0f, playerLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            // Cek apakah player sedang crouch — jika ya, sweep tidak mengenai
            PlayerCrouch crouch = hit.GetComponentInParent<PlayerCrouch>();
            if (crouch != null && crouch.IsCrouching)
            {
                Log("Sweep MISS — player sedang crouch");
                continue;
            }

            Log("Sweep HIT player!");
            DealSanityDamageToPlayer();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 1f, 0.3f);
        Vector3 center = new Vector3(transform.position.x, transform.position.y + sweepHeight / 2f, 0f);
        Gizmos.DrawWireCube(center, new Vector3(sweepWidth, sweepHeight, 0f));
    }
}
