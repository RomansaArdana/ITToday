using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private DialogueUI dialogueUI;

    private DialogueSequenceSO currentSequence;
    private GameObject currentInteractor;
    private int currentLineIndex;
    private bool isDialogueActive;

    public bool IsDialogueActive => isDialogueActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartDialogue(
        DialogueSequenceSO sequence,
        GameObject interactor
    )
    {
        if (isDialogueActive)
        {
            return;
        }

        if (sequence == null)
        {
            return;
        }

        if (sequence.Lines == null || sequence.Lines.Count == 0)
        {
            return;
        }

        currentSequence = sequence;
        currentInteractor = interactor;
        currentLineIndex = 0;
        isDialogueActive = true;

        ShowCurrentLine();
    }

    public void ContinueDialogue()
    {
        if (!isDialogueActive)
        {
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= currentSequence.Lines.Count)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (dialogueUI == null)
        {
            return;
        }

        DialogueLine line = currentSequence.Lines[currentLineIndex];

        dialogueUI.ShowLine(line);
    }

    private void EndDialogue()
    {
        isDialogueActive = false;

        if (dialogueUI != null)
        {
            dialogueUI.Hide();
        }

        if (currentInteractor != null)
        {
            PlayerStateController stateController =
                currentInteractor.GetComponent<PlayerStateController>();

            stateController?.ExitInteraction();
        }

        currentSequence = null;
        currentInteractor = null;
        currentLineIndex = 0;
    }
}