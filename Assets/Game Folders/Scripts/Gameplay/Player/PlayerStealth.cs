using UnityEngine;

public class PlayerStealth : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStateController stateController;

    private HideSpot currentHideSpot;

    public bool IsCrouching { get; private set; }
    public bool IsHidden { get; private set; }

    public HideSpot CurrentHideSpot => currentHideSpot;

    public bool CanHideAtCurrentSpot =>
        currentHideSpot != null &&
        currentHideSpot.CanHide;

    public float MovementMultiplier =>
        IsCrouching && stats != null
            ? stats.CrouchSpeedMultiplier
            : 1f;

    public float StealthMultiplier
    {
        get
        {
            if (stats == null)
            {
                return 1f;
            }

            if (IsHidden)
            {
                return stats.HideStealthMultiplier;
            }

            if (IsCrouching)
            {
                return stats.CrouchStealthMultiplier;
            }

            return 1f;
        }
    }

    private void Awake()
    {
        if (stats == null)
        {
            PlayerController playerController =
                GetComponent<PlayerController>();

            if (playerController != null)
            {
                stats = playerController.Stats;
            }
        }

        if (inputReader == null)
        {
            inputReader = GetComponent<PlayerInputReader>();
        }

        if (stateController == null)
        {
            stateController = GetComponent<PlayerStateController>();
        }
    }

    private void Update()
    {
        if (inputReader == null || stateController == null)
        {
            return;
        }

        UpdateCrouch();
        UpdateHide();
    }

    private void UpdateCrouch()
    {
        if (IsHidden)
        {
            return;
        }

        IsCrouching = inputReader.CrouchHeld;
    }

    private void UpdateHide()
    {
        if (IsHidden)
        {
            if (!inputReader.HideHeld)
            {
                ExitHide();
            }

            return;
        }

        if (!inputReader.HideHeld)
        {
            return;
        }

        if (!IsCrouching)
        {
            return;
        }

        if (!CanHideAtCurrentSpot)
        {
            return;
        }

        if (!stateController.CanHide())
        {
            return;
        }

        EnterHide();
    }

    private void EnterHide()
    {
        if (currentHideSpot == null)
        {
            return;
        }

        Transform hidePoint = currentHideSpot.HidePoint;

        if (hidePoint == null)
        {
            return;
        }

        transform.position = hidePoint.position;

        IsHidden = true;

        stateController.SetState(
            PlayerState.Hide
        );
    }

    private void ExitHide()
    {
        IsHidden = false;

        if (stateController.CurrentState == PlayerState.Hide)
        {
            stateController.SetState(
                PlayerState.Idle
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HideSpot hideSpot =
            other.GetComponentInParent<HideSpot>();

        if (hideSpot == null)
        {
            return;
        }

        currentHideSpot = hideSpot;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        HideSpot hideSpot =
            other.GetComponentInParent<HideSpot>();

        if (hideSpot == null)
        {
            return;
        }

        if (currentHideSpot == hideSpot)
        {
            currentHideSpot = null;
        }
    }
}