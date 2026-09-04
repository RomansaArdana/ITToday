using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene01IntroCutscene : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueSequenceSO introSequence;

    [Header("Scene Transition")]
    [Tooltip("Nama scene berikutnya. Harus sama persis dengan nama di Build Settings.")]
    [SerializeField] private string nextSceneName = "Scene02";
    [Tooltip("Delay sebelum pindah scene (detik)")]
    [SerializeField] private float transitionDelay = 0.5f;

    [Header("Skip (untuk testing)")]
    [Tooltip("Centang ini untuk langsung mulai dialog tanpa visual/audio. Cocok saat asset belum siap.")]
    [SerializeField] private bool skipVisualsAndAudio = false;

    [Header("Visuals (CUT TO) — bisa dikosongkan dulu")]
    [SerializeField] private GameObject sittingInara;
    [SerializeField] private GameObject standingInara;
    [SerializeField] private GameObject windowCityView;

    [Header("Audio — bisa dikosongkan dulu")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource phoneRingSource;

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        if (skipVisualsAndAudio)
        {
            // ── Mode Testing: langsung mulai dialog tanpa delay ──────────────
            yield return null; // tunggu 1 frame agar DialogueManager siap

            StartDialogueNow();

            // Tunggu sampai dialog selesai
            while (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
                yield return null;

            yield return new WaitForSeconds(transitionDelay);
            GoToNextScene();
            yield break;
        }

        // ── Mode Normal: dengan visual & audio ──────────────────────────────
        if (sittingInara != null) sittingInara.SetActive(true);
        if (standingInara != null) standingInara.SetActive(false);
        if (windowCityView != null) windowCityView.SetActive(false);

        if (bgmSource != null) bgmSource.Play();

        yield return new WaitForSeconds(2f);

        if (phoneRingSource != null) phoneRingSource.Play();

        yield return new WaitForSeconds(1.5f);

        StartDialogueNow();

        // Tunggu sampai dialog selesai
        while (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            yield return null;
        }

        // CUT TO: Inara berdiri di jendela
        if (sittingInara != null) sittingInara.SetActive(false);
        if (standingInara != null) standingInara.SetActive(true);
        if (windowCityView != null) windowCityView.SetActive(true);

        if (bgmSource != null) bgmSource.Stop();

        yield return new WaitForSeconds(transitionDelay);
        GoToNextScene();
    }

    private void StartDialogueNow()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("[Scene01] DialogueManager tidak ditemukan di scene!");
            return;
        }

        if (introSequence == null)
        {
            Debug.LogError("[Scene01] Intro Sequence belum di-assign di Inspector!");
            return;
        }

        DialogueManager.Instance.StartDialogue(introSequence, gameObject);
    }

    private void GoToNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[Scene01] Next Scene Name kosong! Isi di Inspector.");
            return;
        }

        Debug.Log($"[Scene01] Pindah ke scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }
}
