using System.Collections;
using UnityEngine;

/// <summary>
/// Cover object di arena boss fight.
/// Memiliki HP sendiri dan bisa hancur akibat serangan boss.
/// Akan respawn setelah beberapa detik agar arena tidak menjadi unwinnable.
///
/// Setup:
///   - Tambahkan script ini ke object cover (pilar/dinding)
///   - Pastikan ada Collider2D untuk menerima damage dari attack boss
/// </summary>
public class BossCoverObject : MonoBehaviour
{
    [Header("Cover Stats")]
    [SerializeField] private float maxHP = 3f;
    [SerializeField] private float respawnTime = 8f;

    [Header("Visuals")]
    [SerializeField] private GameObject intactVisual;
    [SerializeField] private GameObject destroyedVisual;
    [SerializeField] private ParticleSystem destroyVFX;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private float currentHP;
    private bool isDestroyed;

    public bool IsDestroyed => isDestroyed;

    private void Awake()
    {
        currentHP = maxHP;
        SetVisual(intact: true);
    }

    // =========================================================
    // DAMAGE
    // =========================================================

    public void TakeDamage(float amount)
    {
        if (isDestroyed || amount <= 0f) return;

        currentHP = Mathf.Max(0f, currentHP - amount);

        Log($"Cover HP: {currentHP:F1}/{maxHP:F1}");

        if (currentHP <= 0f)
            DestroyCover();
    }

    private void DestroyCover()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        Log("Cover hancur -> respawn dalam " + respawnTime + "s");

        SetVisual(intact: false);

        if (destroyVFX != null)
            destroyVFX.Play();

        // Nonaktifkan collider agar player tidak bisa berlindung
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        StartCoroutine(RespawnRoutine());
    }

    // =========================================================
    // RESPAWN
    // =========================================================

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);
        Respawn();
    }

    private void Respawn()
    {
        isDestroyed = false;
        currentHP = maxHP;

        SetVisual(intact: true);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;

        Log("Cover respawn!");
    }

    // =========================================================
    // VISUAL
    // =========================================================

    private void SetVisual(bool intact)
    {
        if (intactVisual != null)
            intactVisual.SetActive(intact);

        if (destroyedVisual != null)
            destroyedVisual.SetActive(!intact);
    }

    // =========================================================
    // COLLISION
    // =========================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Terima damage dari projectile boss
        BossProjectileDamage proj = other.GetComponent<BossProjectileDamage>();
        if (proj != null)
            TakeDamage(proj.Damage);
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[BossCover] {message}", this);
    }
}
