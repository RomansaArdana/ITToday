using System;
using UnityEngine;

public class SanityController : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;

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
    }

    private void Update()
    {
        if (stats == null)
        {
            return;
        }

        UpdateSanityOverTime();

        DebugTesting();
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

        CurrentLevel = newLevel;

        Debug.Log(
            $"Sanity Level: {CurrentLevel}",
            this
        );

        OnSanityLevelChanged?.Invoke(CurrentLevel);
    }

    public void SetDrainActive(bool active)
    {
        isDraining = active;

        if (active)
        {
            isRecovering = false;
        }
    }

    public void SetRecoveryActive(bool active)
    {
        isRecovering = active;

        if (active)
        {
            isDraining = false;
        }
    }

    public void StopSanityChange()
    {
        isDraining = false;
        isRecovering = false;
    }

    public void DrainSanity(float amount)
    {
        if (amount <= 0f)
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
    }

    public void RecoverSanity(float amount)
    {
        if (amount <= 0f)
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
    }

    public void RestoreFullSanity()
    {
        if (stats == null)
        {
            return;
        }

        float previousSanity = currentSanity;

        currentSanity = stats.MaxSanity;

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

    private void DebugTesting()
    {
        debugTimer += Time.deltaTime;

        if (debugTimer >= 1f)
        {
            debugTimer = 0f;

            Debug.Log(
                $"Sanity: {CurrentSanity:F1}/{MaxSanity:F1} | Level: {CurrentLevel}",
                this
            );
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetDrainActive(true);

            Debug.Log(
                "Sanity Drain: ON",
                this
            );
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetRecoveryActive(true);

            Debug.Log(
                "Sanity Recovery: ON",
                this
            );
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StopSanityChange();

            Debug.Log(
                "Sanity Change: STOP",
                this
            );
        }
    }
}