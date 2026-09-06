using System.Collections;
using UnityEngine;

/// <summary>
/// Komponen untuk pilar/dinding pelindung di arena bos.
/// Bisa hancur saat terkena serangan bos, lalu respawn otomatis setelah beberapa detik.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BossCover : MonoBehaviour
{
    [Header("Cover Stats")]
    [Tooltip("Waktu tunggu sebelum cover respawn setelah hancur (detik).")]
    [SerializeField] private float respawnDelay = 8f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer coverRenderer;
    [Tooltip("Warna saat cover hancur / tidak aktif.")]
    [SerializeField] private Color destroyedColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
    [Tooltip("Warna normal cover.")]
    [SerializeField] private Color normalColor = Color.white;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private Collider2D col;
    private bool isDestroyed = false;
    private Coroutine respawnCoroutine;

    public bool IsDestroyed => isDestroyed;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        coverRenderer ??= GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        Restore();
    }

    // ─────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────

    /// <summary>
    /// Dipanggil oleh BossAttackController saat AoE sweep mengenai cover ini.
    /// </summary>
    public void TakeHit()
    {
        if (isDestroyed) return;

        Log($"{name} hancur terkena serangan bos!");

        isDestroyed = true;
        DisableCover();

        if (respawnCoroutine != null)
            StopCoroutine(respawnCoroutine);

        respawnCoroutine = StartCoroutine(RespawnAfterDelay());
    }

    /// <summary>
    /// Dipanggil oleh BossArenaSetup untuk restore semua cover saat fase baru dimulai.
    /// </summary>
    public void Restore()
    {
        if (respawnCoroutine != null)
            StopCoroutine(respawnCoroutine);

        isDestroyed = false;
        EnableCover();
        Log($"{name} dipulihkan.");
    }

    // ─────────────────────────────────────────
    // Internal
    // ─────────────────────────────────────────

    private IEnumerator RespawnAfterDelay()
    {
        Log($"Respawn dalam {respawnDelay}s...");
        yield return new WaitForSeconds(respawnDelay);

        isDestroyed = false;
        EnableCover();
        Log($"{name} respawn dari tanah!");
    }

    private void DisableCover()
    {
        if (col != null) col.enabled = false;
        if (coverRenderer != null) coverRenderer.color = destroyedColor;
    }

    private void EnableCover()
    {
        if (col != null) col.enabled = true;
        if (coverRenderer != null) coverRenderer.color = normalColor;
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[BossCover] {message}", this);
    }
}
