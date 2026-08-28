using UnityEngine;

[System.Serializable]
public class DialogueEvent
{
    public enum EventType
    {
        SetStoryFlag,
        RemoveStoryFlag,
        ChangeChapter
    }

    [Header("Event")]
    [SerializeField] private EventType type = EventType.SetStoryFlag;

    [Header("Story Flag")]
    [SerializeField] private string storyFlag;

    [Header("Chapter")]
    [SerializeField] private int chapter = 1;

    public void Execute()
    {
        DialogueManager.DebugLog($"[DialogueEvent] Execute → Type: {type}");

        if (StoryManager.Instance == null)
        {
            DialogueManager.DebugWarning("[DialogueEvent] Gagal dijalankan → StoryManager tidak ditemukan.");
            return;
        }

        switch (type)
        {
            case EventType.SetStoryFlag:
                StoryManager.Instance.SetFlag(storyFlag);
                DialogueManager.DebugLog($"[DialogueEvent] Set Story Flag → \"{storyFlag}\"");
                break;

            case EventType.RemoveStoryFlag:
                StoryManager.Instance.RemoveFlag(storyFlag);
                DialogueManager.DebugLog($"[DialogueEvent] Remove Story Flag → \"{storyFlag}\"");
                break;

            case EventType.ChangeChapter:
                StoryManager.Instance.ChangeChapter(chapter);
                DialogueManager.DebugLog($"[DialogueEvent] Change Chapter → {chapter}");
                break;
        }
    }
}