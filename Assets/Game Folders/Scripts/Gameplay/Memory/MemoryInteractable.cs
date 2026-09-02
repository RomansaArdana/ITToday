using UnityEngine;

/// <summary>
/// Integration layer antara Memory Object (world) dan Dialogue + Story system.
///
/// Flow:
///   Player interact → MemoryInteractable.CanInteract() → cek prerequisiteFlag
///       → MemoryInteractable.Interact() → DialogueManager.StartDialogue()
///       → Dialogue selesai (via DialogueSequenceSO.completionEvents atau callback)
///       → StoryManager.SetFlag(completionFlag)
///       → isCompleted = true (jika IsOneShot)
///
/// TIDAK membuat DialogueManager baru.
/// TIDAK membuat StoryManager baru.
/// TIDAK menyimpan state story sendiri.
/// Semua condition dan flag dikelola oleh StoryManager existing.
///
/// Cara pakai:
///   1. Tambahkan komponen ini ke GameObject memory object di scene.
///   2. Assign MemorySO di Inspector.
///   3. Assign layer Interactable agar PlayerInteraction bisa mendeteksinya.
///   4. Pastikan DialogueManager dan StoryManager ada di scene.
/// </summary>
public class MemoryInteractable : InteractableBase
{
    [Header("Memory Data")]
    [Tooltip("ScriptableObject yang mendefinisikan ID, prerequisite, dialogue, dan completion flag memory ini.")]
    [SerializeField] private MemorySO memorySO;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    // ─────────────────────────────────────────────────────────────────────────
    // State
    // ─────────────────────────────────────────────────────────────────────────

    private bool isCompleted;

    /// <summary>True jika memory ini sudah pernah dipicu (dan IsOneShot = true).</summary>
    public bool IsCompleted => isCompleted;

    /// <summary>ID memory ini, untuk referensi dari sistem lain tanpa string literal.</summary>
    public MemoryId MemoryId => memorySO != null ? memorySO.MemoryId : MemoryId.Other;

    // ─────────────────────────────────────────────────────────────────────────
    // InteractableBase — Overrides
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Menentukan apakah memory ini bisa diinteraksi saat ini.
    ///
    /// Blocked jika:
    /// - memorySO null (konfigurasi tidak lengkap)
    /// - IsCompleted dan IsOneShot (sudah selesai, tidak boleh diulang)
    /// - DialogueManager sedang aktif (hindari duplicate dialogue)
    /// - prerequisiteFlag belum di-set di StoryManager
    /// </summary>
    public override bool CanInteract(GameObject interactor)
    {
        if (!base.CanInteract(interactor)) return false;

        if (memorySO == null)
        {
            Log("[MemoryInteractable] CanInteract = false → MemorySO belum di-assign.");
            return false;
        }

        if (memorySO.IsOneShot && isCompleted)
        {
            Log($"[MemoryInteractable] CanInteract = false → Memory [{memorySO.MemoryId}] sudah selesai (one-shot).");
            return false;
        }

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            Log("[MemoryInteractable] CanInteract = false → Dialogue sedang aktif.");
            return false;
        }

        if (!IsPrerequisiteMet())
        {
            Log($"[MemoryInteractable] CanInteract = false → Prerequisite flag \"{memorySO.PrerequisiteFlag}\" belum terpenuhi.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Memicu memory: menjalankan DialogueSequenceSO via DialogueManager existing.
    /// Memory completion (set flag + isCompleted) ditangani oleh callback OnDialogueCompleted
    /// yang didaftarkan sebelum dialogue dimulai.
    /// </summary>
    public override void Interact(GameObject interactor)
    {
        if (memorySO == null)
        {
            Debug.LogWarning("[MemoryInteractable] Interact dipanggil tapi MemorySO null.", this);
            return;
        }

        if (memorySO.DialogueSequence == null)
        {
            Debug.LogWarning($"[MemoryInteractable] Memory [{memorySO.MemoryId}] tidak memiliki DialogueSequence.", this);
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[MemoryInteractable] DialogueManager.Instance tidak ditemukan.", this);
            return;
        }

        Log($"[MemoryInteractable] Memicu memory [{memorySO.MemoryId}].");

        // Daftar callback untuk dipanggil setelah dialogue selesai
        // Menggunakan event dari DialogueSequenceSO completion events yang sudah ada,
        // ditambah callback lokal untuk menangani isCompleted dan StoryFlag
        RegisterCompletionCallback();

        DialogueManager.Instance.StartDialogue(memorySO.DialogueSequence, interactor);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Prerequisite Check
    // ─────────────────────────────────────────────────────────────────────────

    private bool IsPrerequisiteMet()
    {
        // Jika tidak ada prerequisite → selalu bisa diinteraksi
        if (memorySO == null || !memorySO.HasPrerequisite) return true;

        if (StoryManager.Instance == null)
        {
            Debug.LogWarning("[MemoryInteractable] StoryManager tidak ditemukan. Prerequisite check gagal.", this);
            return false;
        }

        return StoryManager.Instance.HasFlag(memorySO.PrerequisiteFlag);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Completion Callback
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Mendaftarkan callback OnDialogueEnded ke DialogueSequenceSO.
    /// Karena DialogueSequenceSO menggunakan completionEvents (bukan C# event),
    /// kita monitor DialogueManager.IsDialogueActive via coroutine ringan.
    /// </summary>
    private void RegisterCompletionCallback()
    {
        // Hentikan coroutine lama jika ada (edge case)
        StopAllCoroutines();
        StartCoroutine(WaitForDialogueCompletion());
    }

    private System.Collections.IEnumerator WaitForDialogueCompletion()
    {
        // Tunggu 1 frame agar DialogueManager.StartDialogue sempat set isDialogueActive = true
        yield return null;

        // Tunggu sampai dialogue selesai
        while (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            yield return null;
        }

        OnMemoryCompleted();
    }

    private void OnMemoryCompleted()
    {
        if (memorySO == null) return;

        Log($"[MemoryInteractable] Memory [{memorySO.MemoryId}] selesai.");

        // Set completion flag di StoryManager (single source of truth untuk story flags)
        if (memorySO.HasCompletionFlag && StoryManager.Instance != null)
        {
            StoryManager.Instance.SetFlag(memorySO.CompletionFlag);
            Log($"[MemoryInteractable] Story flag set: \"{memorySO.CompletionFlag}\".");
        }

        // Tandai sebagai selesai (hanya jika one-shot)
        if (memorySO.IsOneShot)
        {
            isCompleted = true;
            Log($"[MemoryInteractable] Memory [{memorySO.MemoryId}] ditandai selesai (one-shot). Interaksi tidak bisa diulang.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Debug
    // ─────────────────────────────────────────────────────────────────────────

    private void Log(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log(message, this);
    }

    private void OnDrawGizmosSelected()
    {
        if (memorySO == null) return;

        // Tampilkan warna berbeda berdasarkan state
        Gizmos.color = isCompleted ? Color.gray :
                       !IsPrerequisiteMet() ? Color.red :
                       Color.yellow;

        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.3f);
    }
}
