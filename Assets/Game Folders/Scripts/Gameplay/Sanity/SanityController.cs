using System;
using UnityEngine;

public class SanityController : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private float debugInterval = 1f;

    private float currentSanity;
    private float debugTimer;

    private bool isDraining;
    private bool isRecovering;

    public float CurrentSanity => currentSanity;
    public float MaxSanity => stats != null ? stats.MaxSanity : 0f;

    public SanityLevel CurrentLevel { get; private set; }

    public bool IsDraining => isDraining;
    public bool IsRecovering => isRecovering;
    public bool IsDepleted => currentSanity <= 0f;

    public event Action<float, float> OnSanityChanged;
    public event Action<SanityLevel> OnSanityLevelChanged;

    private void Awake()
    {
        if (stats == null)
        {
            Debug.LogError(
                "SanityController: PlayerStatsSO belum di-assign.",
                this
            );

            return;
        }

        currentSanity = stats.MaxSanity;

        UpdateSanityLevel();

        if (enableDebugLog)
        {
            Debug.Log(
                $"[Sanity] Initialized: {currentSanity:F1}/{MaxSanity:F1} | Level: {CurrentLevel}",
                this
            );
        }
    }

    private void Update()
    {
        if (stats == null || IsDepleted)
        {
            return;
        }

        UpdateSanityOverTime();
        UpdateDebugLog();
    }

    private void UpdateSanityOverTime()
    {
        if (isDraining)
        {
            DrainSanity(
                stats.SanityDrainRate * Time.deltaTime
            );

            return;
        }

        if (isRecovering)
        {
            RecoverSanity(
                stats.SanityRecoveryRate * Time.deltaTime
            );
        }
    }

    private void UpdateDebugLog()
    {
        if (!enableDebugLog)
        {
            return;
        }

        debugTimer += Time.deltaTime;

        if (debugTimer < debugInterval)
        {
            return;
        }

        debugTimer = 0f;

        Debug.Log(
            $"[Sanity] {CurrentSanity:F1}/{MaxSanity:F1} | " +
            $"Level: {CurrentLevel} | " +
            $"Draining: {IsDraining} | " +
            $"Recovering: {IsRecovering}",
            this
        );
    }

    private void UpdateSanityLevel()
    {
        if (stats == null || stats.MaxSanity <= 0f)
        {
            return;
        }

        float sanityRatio = currentSanity / stats.MaxSanity;

        SanityLevel newLevel;

        if (currentSanity <= 0f)
        {
            newLevel = SanityLevel.Depleted;
        }
        else if (sanityRatio <= stats.CriticalSanityThreshold)
        {
            newLevel = SanityLevel.Critical;
        }
        else if (sanityRatio <= stats.UnstableSanityThreshold)
        {
            newLevel = SanityLevel.Unstable;
        }
        else
        {
            newLevel = SanityLevel.Healthy;
        }

        if (CurrentLevel == newLevel)
        {
            return;
        }

        SanityLevel previousLevel = CurrentLevel;

        CurrentLevel = newLevel;

        if (enableDebugLog)
        {
            Debug.Log(
                $"[Sanity] Level: {previousLevel} → {CurrentLevel} | " +
                $"Sanity: {CurrentSanity:F1}/{MaxSanity:F1}",
                this
            );
        }

        OnSanityLevelChanged?.Invoke(CurrentLevel);
    }

    public void SetDrainActive(bool active)
    {
        if (IsDepleted)
        {
            return;
        }

        isDraining = active;

        if (active)
        {
            isRecovering = false;
        }

        if (enableDebugLog)
        {
            Debug.Log(
                $"[Sanity] Drain: {(active ? "ON" : "OFF")}",
                this
            );
        }
    }

    public void SetRecoveryActive(bool active)
    {
        if (IsDepleted)
        {
            return;
        }

        isRecovering = active;

        if (active)
        {
            isDraining = false;
        }

        if (enableDebugLog)
        {
            Debug.Log(
                $"[Sanity] Recovery: {(active ? "ON" : "OFF")}",
                this
            );
        }
    }

    public void StopSanityChange()
    {
        isDraining = false;
        isRecovering = false;

        if (enableDebugLog)
        {
            Debug.Log(
                "[Sanity] Sanity change stopped.",
                this
            );
        }
    }

    public void DrainSanity(float amount)
    {
        if (amount <= 0f || IsDepleted)
        {
            return;
        }

        float previousSanity = currentSanity;

        currentSanity = Mathf.Max(
            currentSanity - amount,
            0f
        );

        if (Mathf.Approximately(previousSanity, currentSanity))
        {
            return;
        }

        UpdateSanityLevel();

        OnSanityChanged?.Invoke(
            currentSanity,
            MaxSanity
        );

        if (enableDebugLog)
        {
            Debug.Log(
                $"[Sanity] Drain: {previousSanity:F1} → {currentSanity:F1} " +
                $"(-{amount:F1})",
                this
            );
        }

        if (IsDepleted && enableDebugLog)
        {
            Debug.Log(
                "[Sanity] DEPLETED → PlayerDeathHandler akan menangani kematian.",
                this
            );
        }
    }

    public void RecoverSanity(float amount)
    {
        if (amount <= 0f || IsDepleted)
        {
            return;
        }

        float previousSanity = currentSanity;

        currentSanity = Mathf.Min(
            currentSanity + amount,
            stats.MaxSanity
        );

        if (Mathf.Approximately(previousSanity, currentSanity))
        {
            return;
        }

        UpdateSanityLevel();

        OnSanityChanged?.Invoke(
            currentSanity,
            MaxSanity
        );
    }

    public void SetSanity(float value)
    {
        if (stats == null)
        {
            return;
        }

        float previousSanity = currentSanity;

        currentSanity = Mathf.Clamp(
            value,
            0f,
            stats.MaxSanity
        );

        if (Mathf.Approximately(previousSanity, currentSanity))
        {
            return;
        }

        UpdateSanityLevel();

        OnSanityChanged?.Invoke(
            currentSanity,
            MaxSanity
        );

        if (enableDebugLog)
        {
            Debug.Log(
                $"[Sanity] Set: {previousSanity:F1} → {currentSanity:F1}",
                this
            );
        }
    }

    public void RestoreFullSanity()
    {
        if (stats == null)
        {
            return;
        }

        float previousSanity = currentSanity;

        currentSanity = stats.MaxSanity;

        debugTimer = 0f;

        UpdateSanityLevel();

        OnSanityChanged?.Invoke(
            currentSanity,
            MaxSanity
        );

        if (enableDebugLog)
        {
            Debug.Log(
                $"[Sanity] Restored: {previousSanity:F1} → {CurrentSanity:F1}/{MaxSanity:F1}",
                this
            );
        }
    }
}