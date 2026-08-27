using System.Collections.Generic;
using UnityEngine;

public class NPCDialogue : InteractableBase
{
    [Header("Dialogue")]
    [SerializeField] private List<NPCDialogueEntry> dialogues = new();

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;

    public override void Interact(GameObject interactor)
    {
        if (DialogueManager.Instance == null)
        {
            LogWarning("DialogueManager tidak ditemukan.");
            return;
        }

        if (DialogueManager.Instance.IsDialogueActive)
        {
            Log("Interaction ditolak → Dialogue sedang aktif.");
            return;
        }

        Log($"Interact diterima → NPC: {gameObject.name}");

        DialogueSequenceSO sequence = GetAvailableDialogue();

        if (sequence == null)
        {
            Log($"Tidak ada dialogue yang tersedia → NPC: {gameObject.name}");
            return;
        }

        Log(
            $"Sequence terpilih → \"{sequence.SequenceId}\" | NPC → {gameObject.name}"
        );

        DialogueManager.Instance.StartDialogue(
            sequence,
            interactor
        );
    }

    private DialogueSequenceSO GetAvailableDialogue()
    {
        if (dialogues == null || dialogues.Count == 0)
        {
            Log("Dialogue list kosong.");
            return null;
        }

        Log($"Mengecek {dialogues.Count} dialogue entry...");

        for (int i = 0; i < dialogues.Count; i++)
        {
            NPCDialogueEntry entry = dialogues[i];

            if (entry == null)
            {
                Log($"Entry {i} kosong → Skip.");
                continue;
            }

            if (entry.Sequence == null)
            {
                Log($"Entry {i} tidak memiliki Sequence → Skip.");
                continue;
            }

            Log(
                $"Checking Entry {i} → \"{entry.Sequence.SequenceId}\""
            );

            if (!entry.IsAvailable())
            {
                Log(
                    $"Entry {i} tidak tersedia → \"{entry.Sequence.SequenceId}\""
                );

                continue;
            }

            Log(
                $"Entry {i} tersedia → \"{entry.Sequence.SequenceId}\""
            );

            return entry.Sequence;
        }

        return null;
    }

    private void Log(string message)
    {
        if (!enableDebugLog)
        {
            return;
        }

        Debug.Log(
            $"[NPCDialogue] {message}",
            this
        );
    }

    private void LogWarning(string message)
    {
        if (!enableDebugLog)
        {
            return;
        }

        Debug.LogWarning(
            $"[NPCDialogue] {message}",
            this
        );
    }
}