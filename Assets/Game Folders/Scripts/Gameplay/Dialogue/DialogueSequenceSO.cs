using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DialogueSequence",
    menuName = "The Day After/Dialogue/Dialogue Sequence"
)]
public class DialogueSequenceSO : ScriptableObject
{
    [SerializeField] private string sequenceId;
    [SerializeField] private List<DialogueLine> lines = new();

    public string SequenceId => sequenceId;
    public IReadOnlyList<DialogueLine> Lines => lines;
}

[Serializable]
public class DialogueLine
{
    [SerializeField] private string speakerName;

    [TextArea(2, 5)]
    [SerializeField] private string dialogueText;

    [SerializeField] private Sprite portrait;

    public string SpeakerName => speakerName;
    public string DialogueText => dialogueText;
    public Sprite Portrait => portrait;
}