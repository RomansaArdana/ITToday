using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DialogueSequence",
    menuName = "The Day After/Dialogue/Dialogue Sequence"
)]
public class DialogueSequenceSO : ScriptableObject
{
    [Header("Sequence")]
    [SerializeField] private string sequenceId;

    [Header("Dialogue Nodes")]
    [SerializeField] private List<DialogueNode> nodes = new();

    [Header("Completion Events")]
    [SerializeField] private List<DialogueEvent> completionEvents = new();

    public string SequenceId => sequenceId;

    public IReadOnlyList<DialogueNode> Nodes => nodes;

    public void ExecuteCompletionEvents()
    {
        if (completionEvents == null ||
            completionEvents.Count == 0)
        {
            DialogueManager.DebugLog(
                $"[DialogueSequence] Sequence \"{sequenceId}\" tidak memiliki Completion Event."
            );

            return;
        }

        DialogueManager.DebugLog(
            $"[DialogueSequence] Menjalankan {completionEvents.Count} Completion Event → \"{sequenceId}\""
        );

        foreach (DialogueEvent dialogueEvent in completionEvents)
        {
            if (dialogueEvent == null)
            {
                DialogueManager.DebugWarning(
                    $"[DialogueSequence] Ada Completion Event kosong → \"{sequenceId}\""
                );

                continue;
            }

            dialogueEvent.Execute();
        }
    }
}