using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TrainQuestManager : MonoBehaviour
{
    [Header("Quest Requirements")]
    [Tooltip("Daftar Story Flag yang harus terpenuhi. Contoh: npc1_talked, npc2_talked, dll")]
    [SerializeField] private List<string> requiredFlags = new List<string>
    {
        "train_npc_1",
        "train_npc_2",
        "train_npc_3",
        "train_npc_4"
    };

    [Header("Completion Action")]
    [Tooltip("Aksi yang akan dipanggil saat semua NPC sudah diajak bicara (contoh: mainkan cutscene, load scene)")]
    public UnityEvent OnAllNPCsInteracted;

    private void Start()
    {
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.OnFlagSet += HandleStoryFlagSet;
            
            // Cek di awal, siapa tahu gamenya di-load saat flag sudah terpenuhi
            CheckAllFlags();
        }
    }

    private void OnDestroy()
    {
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.OnFlagSet -= HandleStoryFlagSet;
        }
    }

    private void HandleStoryFlagSet(string flag)
    {
        if (requiredFlags.Contains(flag))
        {
            Debug.Log($"[TrainQuestManager] NPC Flag terpenuhi: {flag}");
            CheckAllFlags();
        }
    }

    private void CheckAllFlags()
    {
        foreach (string flag in requiredFlags)
        {
            if (!StoryManager.Instance.HasFlag(flag))
            {
                // Masih ada NPC yang belum diajak bicara
                return;
            }
        }

        Debug.Log("[TrainQuestManager] SEMUA NPC SUDAH DIAJAK BICARA! Memicu Cutscene/Transisi...");
        
        // Lepas event agar tidak terpanggil dua kali
        StoryManager.Instance.OnFlagSet -= HandleStoryFlagSet;

        // Panggil aksi (seperti play cutscene/pindah scene)
        OnAllNPCsInteracted?.Invoke();
    }

    /// <summary>
    /// Fungsi pembantu agar bisa dipanggil lewat Unity Event di Inspector
    /// </summary>
    public void LoadNextScene(string sceneName)
    {
        Debug.Log($"[TrainQuestManager] Memuat scene: {sceneName}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
