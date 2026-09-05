using UnityEngine;

public class PlayerCloakOfInvisibility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStealth playerStealth;
    [SerializeField] private PlayerAnimator playerAnimator;

    [Header("Cloak")]
    [SerializeField] private float duration = 4f;
    [SerializeField] private float cooldown = 8f;

    [Header("VFX & Audio")]
    [SerializeField] private ParticleSystem activateVFX;
    [SerializeField] private ParticleSystem deactivateVFX;
    [SerializeField] private AudioSource activateAudio;
    [SerializeField] private AudioSource deactivateAudio;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private float durationTimer;
    private float cooldownTimer;
    private float baseDuration;

    public bool IsCloaked { get; private set; }
    public bool IsOnCooldown => cooldownTimer > 0f;
    public float DurationRemaining => durationTimer;
    public float CooldownRemaining => cooldownTimer;

    private void Awake()
    {
        inputReader ??= GetComponent<PlayerInputReader>();
        playerStealth ??= GetComponent<PlayerStealth>();
        playerAnimator ??= GetComponent<PlayerAnimator>();

        IsCloaked = false;
        durationTimer = 0f;
        cooldownTimer = 0f;
        baseDuration = duration;
    }

    private void Update()
    {
        UpdateTimers();

        if (IsCloaked)
        {
            if (durationTimer <= 0f)
                DeactivateCloak();

            return;
        }

        TryActivate();
    }

    private void UpdateTimers()
    {
        if (durationTimer > 0f)
        {
            durationTimer -= Time.deltaTime;

            if (enableDebugLog)
                Debug.Log($"[Cloak] Duration: {durationTimer:F2}s | IsCloaked: {IsCloaked}", this);

            if (durationTimer <= 0f)
            {
                durationTimer = 0f;

                if (enableDebugLog)
                    Debug.Log("[Cloak] Duration habis → DeactivateCloak()", this);

                DeactivateCloak();
                return;
            }
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
                cooldownTimer = 0f;
        }
    }

    private void TryActivate()
    {
        if (inputReader == null) return;
        if (!inputReader.CloakPressed) return;
        if (IsCloaked) return;
        if (cooldownTimer > 0f)
        {
            if (enableDebugLog)
                Debug.Log($"[Cloak] Cooldown {cooldownTimer:F1}s", this);

            return;
        }

        ActivateCloak();
    }

    private void ActivateCloak()
    {
        if (IsCloaked) return;

        IsCloaked = true;
        durationTimer = Mathf.Max(0f, duration);

        if (activateVFX != null)
            activateVFX.Play();

        if (activateAudio != null)
            activateAudio.Play();

        if (playerAnimator != null)
            playerAnimator.SetCloaked(true);

        if (enableDebugLog)
            Debug.Log($"[Cloak] ON | Duration: {durationTimer:F1}s", this);

        if (durationTimer <= 0f)
            DeactivateCloak();
    }

    private void DeactivateCloak()
    {
        if (!IsCloaked) return;

        IsCloaked = false;
        durationTimer = 0f;
        cooldownTimer = Mathf.Max(0f, cooldown);

        if (deactivateVFX != null)
            deactivateVFX.Play();

        if (deactivateAudio != null)
            deactivateAudio.Play();

        if (playerAnimator != null)
            playerAnimator.SetCloaked(false);

        if (enableDebugLog)
            Debug.Log($"[Cloak] OFF | Cooldown: {cooldownTimer:F1}s", this);
    }

    public void ForceDeactivate()
    {
        if (!IsCloaked) return;

        DeactivateCloak();
    }

    public void SetDurationBonus(float bonusDuration)
    {
        duration = baseDuration + Mathf.Max(0f, bonusDuration);

        if (enableDebugLog)
            Debug.Log($"[Cloak] Duration: {baseDuration:F1}s + {bonusDuration:F1}s = {duration:F1}s", this);
    }

    public void ResetCooldown()
    {
        cooldownTimer = 0f;
    }
}