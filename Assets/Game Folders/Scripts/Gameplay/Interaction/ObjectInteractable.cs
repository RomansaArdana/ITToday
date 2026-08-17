using UnityEngine;

public class ObjectInteractable : InteractableBase
{
    [SerializeField] private string objectName = "Object";

    public override void Interact(GameObject interactor)
    {
        Debug.Log($"{objectName} interacted by {interactor.name}");

        PlayerStateController stateController =
            interactor.GetComponent<PlayerStateController>();

        stateController?.ExitInteraction();
    }
}