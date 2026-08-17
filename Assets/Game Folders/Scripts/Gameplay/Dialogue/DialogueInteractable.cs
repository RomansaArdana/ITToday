using UnityEngine;

public class DialogueInteractable : InteractableBase
{
    [SerializeField] private DialogueSequenceSO dialogueSequence;

    public override void Interact(GameObject interactor)
    {
        if (dialogueSequence == null)
        {
            return;
        }

        DialogueManager.Instance?.StartDialogue(
            dialogueSequence,
            interactor
        );
    }
}