using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour, IPlayerInput
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference hideAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference cloakAction;

    public Vector2 MoveInput { get; private set; }

    public bool InteractPressed { get; private set; }
    public bool HidePressed { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool CloakPressed { get; private set; }

    public bool InteractHeld { get; private set; }
    public bool HideHeld { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool AttackHeld { get; private set; }

    public bool IsInteractHeld =>
        interactAction != null &&
        interactAction.action.IsPressed();

    private void OnEnable()
    {
        EnableActions();
        SubscribeActions();
    }

    private void OnDisable()
    {
        UnsubscribeActions();
        DisableActions();

        ResetInputState();
    }

    private void Update()
    {
        MoveInput =
            moveAction != null
                ? moveAction.action.ReadValue<Vector2>()
                : Vector2.zero;
    }

    private void EnableActions()
    {
        moveAction?.action.Enable();
        interactAction?.action.Enable();
        hideAction?.action.Enable();
        jumpAction?.action.Enable();
        crouchAction?.action.Enable();
        attackAction?.action.Enable();
        cloakAction?.action.Enable();
    }

    private void DisableActions()
    {
        moveAction?.action.Disable();
        interactAction?.action.Disable();
        hideAction?.action.Disable();
        jumpAction?.action.Disable();
        crouchAction?.action.Disable();
        attackAction?.action.Disable();
        cloakAction?.action.Disable();
    }

    private void SubscribeActions()
    {
        if (interactAction != null)
        {
            interactAction.action.performed += OnInteractPerformed;
            interactAction.action.canceled += OnInteractCanceled;
        }

        if (hideAction != null)
        {
            hideAction.action.performed += OnHidePerformed;
            hideAction.action.canceled += OnHideCanceled;
        }

        if (jumpAction != null)
        {
            jumpAction.action.performed += OnJumpPerformed;
        }

        if (crouchAction != null)
        {
            crouchAction.action.performed += OnCrouchPerformed;
            crouchAction.action.canceled += OnCrouchCanceled;
        }

        if (attackAction != null)
        {
            attackAction.action.performed += OnAttackPerformed;
            attackAction.action.canceled += OnAttackCanceled;
        }

        if (cloakAction != null)
        {
            cloakAction.action.performed += OnCloakPerformed;
        }
    }

    private void UnsubscribeActions()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.canceled -= OnInteractCanceled;
        }

        if (hideAction != null)
        {
            hideAction.action.performed -= OnHidePerformed;
            hideAction.action.canceled -= OnHideCanceled;
        }

        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJumpPerformed;
        }

        if (crouchAction != null)
        {
            crouchAction.action.performed -= OnCrouchPerformed;
            crouchAction.action.canceled -= OnCrouchCanceled;
        }

        if (attackAction != null)
        {
            attackAction.action.performed -= OnAttackPerformed;
            attackAction.action.canceled -= OnAttackCanceled;
        }

        if (cloakAction != null)
        {
            cloakAction.action.performed -= OnCloakPerformed;
        }
    }

    private void OnInteractPerformed(
        InputAction.CallbackContext context)
    {
        InteractPressed = true;
        InteractHeld = true;
    }

    private void OnInteractCanceled(
        InputAction.CallbackContext context)
    {
        InteractHeld = false;
    }

    private void OnHidePerformed(
        InputAction.CallbackContext context)
    {
        HidePressed = true;
        HideHeld = true;
    }

    private void OnHideCanceled(
        InputAction.CallbackContext context)
    {
        HideHeld = false;
    }

    private void OnJumpPerformed(
        InputAction.CallbackContext context)
    {
        JumpPressed = true;
    }

    private void OnCrouchPerformed(
        InputAction.CallbackContext context)
    {
        CrouchHeld = true;
    }

    private void OnCrouchCanceled(
        InputAction.CallbackContext context)
    {
        CrouchHeld = false;
    }

    private void OnAttackPerformed(
        InputAction.CallbackContext context)
    {
        AttackPressed = true;
        AttackHeld = true;
    }

    private void OnAttackCanceled(
        InputAction.CallbackContext context)
    {
        AttackHeld = false;
    }

    private void OnCloakPerformed(
        InputAction.CallbackContext context)
    {
        CloakPressed = true;
    }

    private void LateUpdate()
    {
        InteractPressed = false;
        HidePressed = false;
        JumpPressed = false;
        AttackPressed = false;
        CloakPressed = false;
    }

    private void ResetInputState()
    {
        MoveInput = Vector2.zero;

        InteractPressed = false;
        HidePressed = false;
        JumpPressed = false;
        AttackPressed = false;
        CloakPressed = false;

        InteractHeld = false;
        HideHeld = false;
        CrouchHeld = false;
        AttackHeld = false;
    }
}