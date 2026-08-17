using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerStateController stateController;
    [SerializeField] private LayerMask interactableLayer;

    private IInteractable currentInteractable;

    public IInteractable CurrentInteractable => currentInteractable;

    private void Awake()
    {
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
        DetectInteractable();

        if (inputReader == null)
        {
            return;
        }

        if (inputReader.InteractPressed)
        {
            TryInteract();
        }
    }

    private void DetectInteractable()
    {
        currentInteractable = null;

        if (stats == null)
        {
            return;
        }

        if (stateController != null && !stateController.CanInteract())
        {
            return;
        }

        Collider2D[] results = Physics2D.OverlapCircleAll(
            transform.position,
            stats.InteractionRange,
            interactableLayer
        );

        float closestDistance = float.MaxValue;

        foreach (Collider2D result in results)
        {
            IInteractable interactable = result.GetComponentInParent<IInteractable>();

            if (interactable == null)
            {
                continue;
            }

            if (!interactable.CanInteract(gameObject))
            {
                continue;
            }

            float distance = Vector2.Distance(
                transform.position,
                result.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentInteractable = interactable;
            }
        }
    }

    private void TryInteract()
    {
        if (currentInteractable == null)
        {
            return;
        }

        if (!currentInteractable.CanInteract(gameObject))
        {
            return;
        }

        stateController?.EnterInteraction();

        currentInteractable.Interact(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (stats == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(
            transform.position,
            stats.InteractionRange
        );
    }
}