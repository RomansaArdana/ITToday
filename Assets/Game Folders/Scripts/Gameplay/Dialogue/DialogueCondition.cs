using UnityEngine;

[System.Serializable]
public class DialogueCondition
{
    public enum ConditionType
    {
        None,
        StoryFlag,
        Chapter
    }

    public enum Comparison
    {
        IsTrue,
        IsFalse
    }

    [Header("Condition")]
    [SerializeField] private ConditionType type = ConditionType.None;

    [Header("Story Flag")]
    [SerializeField] private string storyFlag;
    [SerializeField] private Comparison comparison = Comparison.IsTrue;

    [Header("Chapter")]
    [SerializeField] private int requiredChapter = 1;

    public bool IsMet()
    {
        bool result;

        switch (type)
        {
            case ConditionType.None:
                result = true;
                break;

            case ConditionType.StoryFlag:
                result = CheckStoryFlag();
                break;

            case ConditionType.Chapter:
                result = CheckChapter();
                break;

            default:
                result = false;
                break;
        }

        DialogueManager.DebugLog(
            $"[DialogueCondition] Type: {type} → Result: {result}"
        );

        return result;
    }

    private bool CheckStoryFlag()
    {
        if (StoryManager.Instance == null)
        {
            DialogueManager.DebugWarning(
                "[DialogueCondition] StoryManager tidak ditemukan."
            );

            return false;
        }

        if (string.IsNullOrWhiteSpace(storyFlag))
        {
            DialogueManager.DebugWarning(
                "[DialogueCondition] Story Flag kosong."
            );

            return false;
        }

        bool flagValue =
            StoryManager.Instance.HasFlag(storyFlag);

        bool result = comparison switch
        {
            Comparison.IsTrue => flagValue,
            Comparison.IsFalse => !flagValue,
            _ => false
        };

        DialogueManager.DebugLog(
            $"[DialogueCondition] Story Flag \"{storyFlag}\" = {flagValue} | Comparison: {comparison} | Result: {result}"
        );

        return result;
    }

    private bool CheckChapter()
    {
        if (StoryManager.Instance == null)
        {
            DialogueManager.DebugWarning(
                "[DialogueCondition] StoryManager tidak ditemukan."
            );

            return false;
        }

        int currentChapter =
            StoryManager.Instance.CurrentChapter;

        bool result =
            currentChapter >= requiredChapter;

        DialogueManager.DebugLog(
            $"[DialogueCondition] Chapter sekarang: {currentChapter} | Required: {requiredChapter} | Result: {result}"
        );

        return result;
    }
}