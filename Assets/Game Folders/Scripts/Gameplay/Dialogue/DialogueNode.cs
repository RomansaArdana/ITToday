using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    [Header("Dialogue")]
    [SerializeField] private DialogueCharacterSO speaker;

    [TextArea(3, 6)]
    [SerializeField] private string dialogueText;

    [Tooltip("Sprite ekspresi untuk node ini. Kosongkan = pakai Default Portrait karakter.")]
    [SerializeField] private Sprite expressionOverride;

    [Header("Condition")]
    [SerializeField] private DialogueCondition condition = new DialogueCondition();

    [Header("Node Events")]
    [SerializeField] private List<DialogueEvent> events = new();

    public DialogueCharacterSO Speaker => speaker;
    public string DialogueText => dialogueText;
    public Sprite ExpressionOverride => expressionOverride;
    public DialogueCondition Condition => condition;
    public IReadOnlyList<DialogueEvent> Events => events;

    public bool IsConditionMet()
    {
        if (condition == null)
        {
            DialogueManager.DebugLog($"[DialogueNode] \"{dialogueText}\" → Tidak memiliki Condition → TRUE");
            return true;
        }

        bool result = condition.IsMet();
        DialogueManager.DebugLog($"[DialogueNode] Condition Check → \"{dialogueText}\" → {result}");
        return result;
    }

    public void ExecuteEvents()
    {
        if (events == null || events.Count == 0)
        {
            DialogueManager.DebugLog($"[DialogueNode] Tidak ada Node Event → \"{dialogueText}\"");
            return;
        }

        DialogueManager.DebugLog($"[DialogueNode] Execute {events.Count} Event(s) → \"{dialogueText}\"");

        foreach (DialogueEvent dialogueEvent in events)
        {
            if (dialogueEvent == null)
            {
                DialogueManager.DebugWarning($"[DialogueNode] Ada Event kosong → \"{dialogueText}\"");
                continue;
            }

            dialogueEvent.Execute();
        }
    }
}