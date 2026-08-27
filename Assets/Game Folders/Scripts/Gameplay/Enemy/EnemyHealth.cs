using System;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private EnemyController enemyController;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }

    public bool IsAlive =>
        CurrentHealth > 0f;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    private bool deathEventInvoked;

    private void Awake()
    {
        enemyController ??=
            GetComponent<EnemyController>();

        if (enemyController == null)
        {
            Debug.LogError(
                "[EnemyHealth] EnemyController tidak ditemukan.",
                this
            );

            return;
        }

        if (enemyController.Stats == null)
        {
            Debug.LogError(
                "[EnemyHealth] EnemyStatsSO belum di-assign.",
                this
            );

            return;
        }

        MaxHealth = Mathf.Max(
            1f,
            enemyController.Stats.MaxHealth
        );

        CurrentHealth = MaxHealth;

        if (enableDebugLog)
        {
            Debug.Log(
                $"[EnemyHealth] Initialized: " +
                $"{CurrentHealth:F1}/{MaxHealth:F1}",
                this
            );
        }
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        float previousHealth =
            CurrentHealth;

        CurrentHealth = Mathf.Max(
            CurrentHealth - amount,
            0f
        );

        OnHealthChanged?.Invoke(
            CurrentHealth,
            MaxHealth
        );

        if (enableDebugLog)
        {
            Debug.Log(
                $"[EnemyHealth] " +
                $"{previousHealth:F1} → {CurrentHealth:F1} " +
                $"(-{amount:F1})",
                this
            );
        }

        if (CurrentHealth <= 0f)
        {
            HandleDeath();
        }
    }

    public void RestoreHealth(float amount)
    {
        if (!IsAlive)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Min(
            CurrentHealth + amount,
            MaxHealth
        );

        OnHealthChanged?.Invoke(
            CurrentHealth,
            MaxHealth
        );
    }

    private void HandleDeath()
    {
        if (deathEventInvoked)
        {
            return;
        }

        deathEventInvoked = true;

        OnDeath?.Invoke();

        if (enableDebugLog)
        {
            Debug.Log(
                "[EnemyHealth] Enemy health reached zero.",
                this
            );
        }
    }
}