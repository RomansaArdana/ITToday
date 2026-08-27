using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private DialogueSequenceSO currentSequence;
    private GameObject currentInteractor;

    private int currentNodeIndex;
    private bool isDialogueActive;

    public bool IsDialogueActive => isDialogueActive;

    public static bool DebugLogsEnabled =>
        Instance != null && Instance.enableDebugLogs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DebugLog("[DialogueManager] Initialized.");
    }

    public void StartDialogue(
        DialogueSequenceSO sequence,
        GameObject interactor
    )
    {
        DebugLog("[DialogueManager] StartDialogue dipanggil.");

        if (isDialogueActive)
        {
            DebugLog(
                "[DialogueManager] Dialogue masih aktif → StartDialogue dibatalkan."
            );

            return;
        }

        if (sequence == null)
        {
            Debug.LogWarning(
                "[DialogueManager] StartDialogue gagal → Sequence null."
            );

            return;
        }

        if (sequence.Nodes == null || sequence.Nodes.Count == 0)
        {
            Debug.LogWarning(
                $"[DialogueManager] StartDialogue gagal → Sequence \"{sequence.SequenceId}\" tidak memiliki Node."
            );

            return;
        }

        currentSequence = sequence;
        currentInteractor = interactor;
        currentNodeIndex = 0;
        isDialogueActive = true;

        DebugLog(
            $"[DialogueManager] Dialogue dimulai → Sequence: \"{sequence.SequenceId}\" | Total Nodes: {sequence.Nodes.Count}"
        );

        ShowCurrentNode();
    }

    public void ContinueDialogue()
    {
        if (!isDialogueActive)
        {
            DebugLog(
                "[DialogueManager] ContinueDialogue dipanggil tetapi dialogue tidak aktif."
            );

            return;
        }

        DebugLog(
            $"[DialogueManager] Continue → Current Node Index: {currentNodeIndex}"
        );

        ExecuteCurrentNodeEvents();

        currentNodeIndex++;

        DebugLog(
            $"[DialogueManager] Pindah ke Node Index: {currentNodeIndex}"
        );

        ShowCurrentNode();
    }

    private void ShowCurrentNode()
    {
        if (!isDialogueActive)
        {
            return;
        }

        if (currentSequence == null)
        {
            DebugLog(
                "[DialogueManager] Current Sequence null → EndDialogue."
            );

            EndDialogue();
            return;
        }

        if (dialogueUI == null)
        {
            Debug.LogWarning(
                "[DialogueManager] Dialogue UI belum di-assign."
            );

            EndDialogue();
            return;
        }

        DebugLog(
            $"[DialogueManager] Mencari Node mulai dari index {currentNodeIndex}..."
        );

        while (currentNodeIndex < currentSequence.Nodes.Count)
        {
            DialogueNode node =
                currentSequence.Nodes[currentNodeIndex];

            if (node == null)
            {
                DebugLog(
                    $"[DialogueManager] Node {currentNodeIndex} null → Skip."
                );

                currentNodeIndex++;
                continue;
            }

            DebugLog(
                $"[DialogueManager] Checking Node {currentNodeIndex} → \"{node.DialogueText}\""
            );

            bool conditionMet = node.IsConditionMet();

            if (!conditionMet)
            {
                DebugLog(
                    $"[DialogueManager] Node {currentNodeIndex} Condition FALSE → Skip."
                );

                currentNodeIndex++;
                continue;
            }

            DebugLog(
                $"[DialogueManager] Node {currentNodeIndex} Condition TRUE → Show."
            );

            dialogueUI.ShowNode(node);
            return;
        }

        DebugLog(
            "[DialogueManager] Tidak ada Node berikutnya → Dialogue selesai."
        );

        EndDialogue();
    }

    private void ExecuteCurrentNodeEvents()
    {
        if (currentSequence == null)
        {
            return;
        }

        if (currentNodeIndex < 0 ||
            currentNodeIndex >= currentSequence.Nodes.Count)
        {
            return;
        }

        DialogueNode node =
            currentSequence.Nodes[currentNodeIndex];

        if (node == null)
        {
            return;
        }

        DebugLog(
            $"[DialogueManager] Execute Events → Node {currentNodeIndex}"
        );

        node.ExecuteEvents();
    }

    private void EndDialogue()
    {
        DialogueSequenceSO finishedSequence = currentSequence;
        GameObject finishedInteractor = currentInteractor;

        DebugLog(
            $"[DialogueManager] EndDialogue → Sequence: \"{finishedSequence?.SequenceId}\""
        );

        isDialogueActive = false;

        if (dialogueUI != null)
        {
            dialogueUI.Hide();
        }

        currentSequence = null;
        currentInteractor = null;
        currentNodeIndex = 0;

        if (finishedSequence != null)
        {
            DebugLog(
                $"[DialogueManager] Menjalankan Completion Events → \"{finishedSequence.SequenceId}\""
            );

            finishedSequence.ExecuteCompletionEvents();
        }

        if (finishedInteractor != null)
        {
            PlayerStateController stateController =
                finishedInteractor.GetComponent<PlayerStateController>();

            if (stateController != null)
            {
                DebugLog(
                    "[DialogueManager] ExitInteraction dipanggil."
                );

                stateController.ExitInteraction();
            }
        }

        DebugLog(
            "[DialogueManager] Dialogue benar-benar selesai."
        );
    }

    public static void DebugLog(string message)
    {
        if (!DebugLogsEnabled)
        {
            return;
        }

        Debug.Log(message);
    }

    public static void DebugWarning(string message)
    {
        if (!DebugLogsEnabled)
        {
            return;
        }

        Debug.LogWarning(message);
    }
}