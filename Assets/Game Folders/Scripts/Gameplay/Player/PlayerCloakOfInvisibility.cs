using UnityEngine;

public class PlayerCloakOfInvisibility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStealth playerStealth;

    [Header("Cloak")]
    [SerializeField] private float duration = 4f;
    [SerializeField] private float cooldown = 8f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    private float durationTimer;
    private float cooldownTimer;

    public bool IsCloaked { get; private set; }
    public bool IsOnCooldown => cooldownTimer > 0f;
    public float DurationRemaining => durationTimer;
    public float CooldownRemaining => cooldownTimer;

    private void Awake()
    {
        inputReader ??= GetComponent<PlayerInputReader>();
        playerStealth ??= GetComponent<PlayerStealth>();

        IsCloaked = false;
        durationTimer = 0f;
        cooldownTimer = 0f;
    }

    private void Update()
    {
        UpdateTimers();

        if (IsCloaked)
        {
            if (durationTimer <= 0f) DeactivateCloak();
            return;
        }

        TryActivate();
    }

    private void UpdateTimers()
    {
        if (durationTimer > 0f)
        {
            durationTimer -= Time.deltaTime;
            if (durationTimer < 0f) durationTimer = 0f;
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f) cooldownTimer = 0f;
        }
    }

    private void TryActivate()
    {
        if (inputReader == null) return;
        if (!inputReader.CloakPressed) return;

        ActivateCloak();
    }

    private void ActivateCloak()
    {
        if (IsCloaked) return;

        if (cooldownTimer > 0f)
        {
            if (enableDebugLog) Debug.Log($"[Cloak] Cooldown {cooldownTimer:F1}s", this);
            return;
        }

        IsCloaked = true;
        durationTimer = Mathf.Max(0f, duration);

        if (enableDebugLog) Debug.Log("[Cloak] ON", this);

        if (durationTimer <= 0f) DeactivateCloak();
    }

    private void DeactivateCloak()
    {
        IsCloaked = false;
        durationTimer = 0f;
        cooldownTimer = Mathf.Max(0f, cooldown);

        if (enableDebugLog) Debug.Log("[Cloak] OFF", this);
    }

    public void ForceDeactivate()
    {
        if (!IsCloaked) return;
        DeactivateCloak();
    }

    public void ResetCooldown()
    {
        cooldownTimer = 0f;
    }
}