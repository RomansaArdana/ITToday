using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour, IPlayerInput
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference hideAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;

    public Vector2 MoveInput { get; private set; }

    public bool InteractPressed { get; private set; }
    public bool HidePressed { get; private set; }
    public bool JumpPressed { get; private set; }

    public bool HideHeld { get; private set; }
    public bool CrouchHeld { get; private set; }

    private void OnEnable()
    {
        EnableActions();
        SubscribeActions();
    }

    private void OnDisable()
    {
        UnsubscribeActions();
        DisableActions();

        MoveInput = Vector2.zero;

        InteractPressed = false;
        HidePressed = false;
        JumpPressed = false;

        HideHeld = false;
        CrouchHeld = false;
    }

    private void Update()
    {
        MoveInput = moveAction != null
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
    }

    private void DisableActions()
    {
        moveAction?.action.Disable();
        interactAction?.action.Disable();
        hideAction?.action.Disable();
        jumpAction?.action.Disable();
        crouchAction?.action.Disable();
    }

    private void SubscribeActions()
    {
        if (interactAction != null)
        {
            interactAction.action.performed += OnInteractPerformed;
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
    }

    private void UnsubscribeActions()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
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
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        InteractPressed = true;
    }

    private void OnHidePerformed(InputAction.CallbackContext context)
    {
        HidePressed = true;
        HideHeld = true;
    }

    private void OnHideCanceled(InputAction.CallbackContext context)
    {
        HideHeld = false;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        JumpPressed = true;
    }

    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        CrouchHeld = true;
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        CrouchHeld = false;
    }

    private void LateUpdate()
    {
        InteractPressed = false;
        HidePressed = false;
        JumpPressed = false;
    }
}