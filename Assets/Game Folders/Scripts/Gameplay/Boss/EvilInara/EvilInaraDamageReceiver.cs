using UnityEngine;

/// <summary>
/// Hitbox yang menerima serangan Lantern dari PlayerLanternAttack.
/// Letakkan component ini pada child object EvilInara yang memiliki Collider2D.
/// Collider2D harus berada di Layer yang sama dengan enemyLayer pada PlayerLanternAttack.
///
/// Flow:
///   PlayerLanternAttack.FireLantern()
///       -> menemukan EvilInaraDamageReceiver via GetComponentInParent
///       -> TryReceiveLanternHit(damage)
///       -> cek IsVulnerable dari EvilInaraStateController
///       -> jika valid: EvilInaraHealth.TakeHit()
/// </summary>
public class EvilInaraDamageReceiver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EvilInaraHealth health;
    [SerializeField] private EvilInaraStateController stateController;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private void Awake()
    {
        // Auto-assign dari parent jika tidak di-set manual
        if (health == null)
            health = GetComponentInParent<EvilInaraHealth>();

        if (stateController == null)
            stateController = GetComponentInParent<EvilInaraStateController>();

        if (health == null)
            Debug.LogError("[EvilInaraDamageReceiver] EvilInaraHealth tidak ditemukan.", this);

        if (stateController == null)
            Debug.LogError("[EvilInaraDamageReceiver] EvilInaraStateController tidak ditemukan.", this);
    }

    /// <summary>
    /// Dipanggil oleh PlayerLanternAttack.FireLantern().
    /// Mengembalikan true jika hit diterima (state Vulnerable).
    /// Mengembalikan false jika hit ditolak.
    /// </summary>
    public bool TryReceiveLanternHit(float damage)
    {
        if (health == null || stateController == null)
            return false;

        if (!health.IsAlive)
        {
            Log("Hit REJECTED — boss already dead");
            return false;
        }

        if (!stateController.IsVulnerable)
        {
            Log($"Hit REJECTED — state: {stateController.CurrentState} (bukan Vulnerable)");
            return false;
        }

        Log($"Hit ACCEPTED | damage={damage:F1} | HP sebelum: {health.CurrentHP}");
        health.TakeHit();
        return true;
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[EvilInaraDamageReceiver] {message}", this);
    }
}
