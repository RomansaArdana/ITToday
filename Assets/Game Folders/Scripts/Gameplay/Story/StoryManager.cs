using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [Header("Chapter")]
    [SerializeField] private int currentChapter = 1;

    private readonly HashSet<string> storyFlags = new();

    public int CurrentChapter => currentChapter;

    public event System.Action<string> OnFlagSet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool HasFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return false;
        return storyFlags.Contains(flag);
    }

    public void SetFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;
        storyFlags.Add(flag);
        OnFlagSet?.Invoke(flag);
    }

    public void RemoveFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;
        storyFlags.Remove(flag);
    }

    public void ChangeChapter(int chapter)
    {
        if (chapter < 1) return;
        currentChapter = chapter;
    }

    public void ResetStory()
    {
        currentChapter = 1;
        storyFlags.Clear();
    }

    [ContextMenu("Test Story Flag")]
    private void TestStoryFlag()
    {
        SetFlag("Met_Garnet");
        Debug.Log($"Met_Garnet: {HasFlag("Met_Garnet")}");
    }
}