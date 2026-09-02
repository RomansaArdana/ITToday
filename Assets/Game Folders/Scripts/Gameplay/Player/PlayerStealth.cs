using UnityEngine;

/// <summary>
/// Menangani state stealth player: Crouch multiplier, Hide spot, dan Cloak.
///
/// IsCrouching dibaca dari PlayerCrouch (ground truth) — bukan dari inputReader.
/// PlayerStealth bertanggung jawab atas: StealthMultiplier dan MovementMultiplier.
/// PlayerCrouch bertanggung jawab atas: collider resize dan ceiling check.
/// </summary>
public class PlayerStealth : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStateController stateController;
    [SerializeField] private PlayerCloakOfInvisibility cloak;
    [SerializeField] private PlayerCrouch playerCrouch;

    private HideSpot currentHideSpot;

    /// <summary>
    /// True jika Inara sedang dalam posisi crouch (collider mengecil).
    /// Dibaca dari PlayerCrouch agar state ini sinkron dengan collider fisik.
    /// </summary>
    public bool IsCrouching => playerCrouch != null && playerCrouch.IsCrouching;

    public bool IsHidden { get; private set; }

    public bool IsCloaked => cloak != null && cloak.IsCloaked;
    public HideSpot CurrentHideSpot => currentHideSpot;
    public bool CanHideAtCurrentSpot => currentHideSpot != null && currentHideSpot.CanHide;

    public float MovementMultiplier => IsCrouching && stats != null ? stats.CrouchSpeedMultiplier : 1f;

    public float StealthMultiplier
    {
        get
        {
            if (stats == null) return 1f;
            if (IsCloaked) return 0f;
            if (IsHidden) return stats.HideStealthMultiplier;
            if (IsCrouching) return stats.CrouchStealthMultiplier;
            return 1f;
        }
    }

    private void Awake()
    {
        if (stats == null)
        {
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController != null) stats = playerController.Stats;
        }

        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (stateController == null) stateController = GetComponent<PlayerStateController>();
        if (cloak == null) cloak = GetComponent<PlayerCloakOfInvisibility>();
        if (playerCrouch == null) playerCrouch = GetComponent<PlayerCrouch>();
    }

    private void Update()
    {
        if (inputReader == null || stateController == null) return;

        UpdateHide();
    }

    private void UpdateHide()
    {
        if (IsHidden)
        {
            if (!inputReader.HideHeld) ExitHide();
            return;
        }

        if (!inputReader.HideHeld) return;
        if (!IsCrouching) return;
        if (!CanHideAtCurrentSpot) return;
        if (!stateController.CanHide()) return;

        EnterHide();
    }

    private void EnterHide()
    {
        if (currentHideSpot == null) return;

        Transform hidePoint = currentHideSpot.HidePoint;

        if (hidePoint == null) return;

        transform.position = hidePoint.position;
        IsHidden = true;
        stateController.SetState(PlayerState.Hide);
    }

    private void ExitHide()
    {
        IsHidden = false;

        if (stateController.CurrentState == PlayerState.Hide)
            stateController.SetState(PlayerState.Idle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HideSpot hideSpot = other.GetComponentInParent<HideSpot>();
        if (hideSpot == null) return;
        currentHideSpot = hideSpot;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        HideSpot hideSpot = other.GetComponentInParent<HideSpot>();
        if (hideSpot == null) return;
        if (currentHideSpot == hideSpot) currentHideSpot = null;
    }
}