using UnityEngine;

/// <summary>
/// ScriptableObject yang mendefinisikan data sebuah Memory Object di dunia game.
///
/// Satu MemorySO = satu memory object (misal: Arloji, Tiket, Ponsel, Kunci Rumah).
/// Gunakan Asset di Inspector untuk mengkonfigurasi per-memory tanpa menyentuh kode.
///
/// Memory ID digunakan sebagai identifier unik — tidak ada hardcoded string di kode lain.
/// </summary>
[CreateAssetMenu(fileName = "Memory_New", menuName = "The Day After/Memory/Memory SO")]
public class MemorySO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("ID unik memory ini. Digunakan oleh sistem lain untuk membedakan memory tanpa string literal.")]
    [SerializeField] private MemoryId memoryId = MemoryId.Watch;

    [Header("Prerequisite — Gameplay Condition")]
    [Tooltip("Story flag yang HARUS sudah di-set sebelum memory ini bisa diinteraksi. " +
             "Kosongkan jika memory tidak punya prerequisite (langsung bisa diinteraksi).")]
    [SerializeField] private string prerequisiteFlag = "";

    [Header("Dialogue")]
    [Tooltip("DialogueSequenceSO yang dijalankan saat memory ini dipicu. Wajib diisi.")]
    [SerializeField] private DialogueSequenceSO dialogueSequence;

    [Header("Completion")]
    [Tooltip("Story flag yang di-set ke StoryManager saat memory selesai (dialogue berakhir). " +
             "Kosongkan jika tidak perlu flag.")]
    [SerializeField] private string completionFlag = "";

    [Tooltip("Jika true, memory ini hanya bisa dipicu SATU kali. " +
             "Jika false, bisa dipicu berulang kali (misalnya untuk cutscene yang bisa diulang).")]
    [SerializeField] private bool isOneShot = true;

    // ─────────────────────────────────────────────────────────────────────────
    // Properties
    // ─────────────────────────────────────────────────────────────────────────

    public MemoryId MemoryId => memoryId;
    public string PrerequisiteFlag => prerequisiteFlag;
    public DialogueSequenceSO DialogueSequence => dialogueSequence;
    public string CompletionFlag => completionFlag;
    public bool IsOneShot => isOneShot;

    /// <summary>
    /// True jika memory ini memiliki prerequisite flag yang harus dipenuhi.
    /// </summary>
    public bool HasPrerequisite => !string.IsNullOrWhiteSpace(prerequisiteFlag);

    /// <summary>
    /// True jika memory ini akan men-set completion flag setelah selesai.
    /// </summary>
    public bool HasCompletionFlag => !string.IsNullOrWhiteSpace(completionFlag);
}

/// <summary>
/// Identifier unik untuk setiap memory object di game.
/// Gunakan enum ini — JANGAN gunakan string literal untuk membedakan memory.
/// </summary>
public enum MemoryId
{
    Watch,          // Chapter 1 — Arloji Kakek
    TrainTicket,    // Chapter 2 — Tiket Kereta
    Phone,          // Chapter 3 — Ponsel
    HouseKey,       // Chapter 4 — Kunci Rumah Kakek (bridge antara Memory dan Progression)
    PhotoAlbum,     // Final Stage — Album Foto (trigger Boss Battle)
    Other           // Untuk memory tambahan di masa depan
}
