using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] private bool interactionEnabled = true;

    public bool IsInteractionEnabled => interactionEnabled;

    public virtual bool CanInteract(GameObject interactor)
    {
        return interactionEnabled && interactor != null;
    }

    public abstract void Interact(GameObject interactor);

    public virtual void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;
    }
}