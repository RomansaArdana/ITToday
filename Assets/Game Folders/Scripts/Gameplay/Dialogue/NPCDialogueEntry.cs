using UnityEngine;

[System.Serializable]
public class NPCDialogueEntry
{
    [Header("Dialogue")]
    [SerializeField] private DialogueSequenceSO sequence;

    [Header("Condition")]
    [SerializeField] private DialogueCondition condition = new DialogueCondition();

    public DialogueSequenceSO Sequence => sequence;

    public bool IsAvailable()
    {
        if (sequence == null) return false;
        if (condition == null) return true;

        return condition.IsMet();
    }
}