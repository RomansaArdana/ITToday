using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour, IPlayerInput
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference hideAction;
    [SerializeField] private InputActionReference jumpAction;

    public Vector2 MoveInput { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool HidePressed { get; private set; }
    public bool JumpPressed { get; private set; }

    private void OnEnable()
    {
        EnableActions();
        SubscribeActions();
    }

    private void OnDisable()
    {
        UnsubscribeActions();
        DisableActions();
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
    }

    private void DisableActions()
    {
        moveAction?.action.Disable();
        interactAction?.action.Disable();
        hideAction?.action.Disable();
        jumpAction?.action.Disable();
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
        }

        if (jumpAction != null)
        {
            jumpAction.action.performed += OnJumpPerformed;
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
        }

        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJumpPerformed;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        InteractPressed = true;
    }

    private void OnHidePerformed(InputAction.CallbackContext context)
    {
        HidePressed = true;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        JumpPressed = true;
    }

    private void LateUpdate()
    {
        InteractPressed = false;
        HidePressed = false;
        JumpPressed = false;
    }
}