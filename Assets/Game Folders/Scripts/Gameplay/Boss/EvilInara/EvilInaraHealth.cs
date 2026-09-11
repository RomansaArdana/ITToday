using System;
using UnityEngine;

/// <summary>
/// Mengelola HP boss Evil Inara (MaxHP = 6, integer).
/// Meng-invoke event ketika HP berkurang atau boss mati.
/// Hanya menerima damage dari EvilInaraDamageReceiver.
/// </summary>
public class EvilInaraHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 6;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    public int MaxHP => maxHP;
    public int CurrentHP { get; private set; }
    public bool IsAlive => CurrentHP > 0;

    /// <summary>Dipanggil setiap kali HP berubah dengan nilai HP terbaru.</summary>
    public event Action<int> OnHPChanged;

    /// <summary>Dipanggil satu kali ketika HP mencapai 0.</summary>
    public event Action OnDead;

    private bool deathInvoked;

    private void Awake()
    {
        CurrentHP = maxHP;
        deathInvoked = false;
        Log($"Initialized: {CurrentHP}/{MaxHP}");
    }

    /// <summary>
    /// Mengurangi HP sebesar 1.
    /// Hanya dipanggil dari EvilInaraDamageReceiver.
    /// </summary>
    public void TakeHit()
    {
        if (!IsAlive) return;

        CurrentHP = Mathf.Max(0, CurrentHP - 1);

        Log($"HP: {CurrentHP + 1} -> {CurrentHP}");

        OnHPChanged?.Invoke(CurrentHP);

        if (CurrentHP <= 0)
            HandleDeath();
    }

    private void HandleDeath()
    {
        if (deathInvoked) return;

        deathInvoked = true;

        Log("HP = 0 -> OnDead invoked");
        OnDead?.Invoke();
    }

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[EvilInaraHealth] {message}", this);
    }
}
