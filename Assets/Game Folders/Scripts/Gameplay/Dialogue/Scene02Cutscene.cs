using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Scene 02 — Perjalanan Pulang & Pemakaman
/// ─────────────────────────────────────────────────────────────────
/// Alur:
///  AKT 1: Montage — Inara meninggalkan kantor, berjalan di kota
///  AKT 2: Pemakaman — Inara di depan peti, menangis
///  AKT 3: CUT TO BLACK — Grim muncul dari kejauhan, berbicara
///  AKT 4: FADE OUT → Scene berikutnya
///
/// Setup Hierarchy:
/// ─ Scene02Cutscene (script ini)
///   ├── [Visual Montage]    GameObject aktif saat akt 1
///   ├── [Visual Cemetery]   GameObject aktif saat akt 2
///   ├── [Visual Grim]       GameObject aktif saat akt 3 (wide shot)
///   ├── Canvas
///   │     └── FadePanel     Image hitam untuk fade in/out
///   ├── BGMSource           AudioSource musik latar
///   └── SFXSource           AudioSource efek suara
/// </summary>
public class Scene02Cutscene : MonoBehaviour
{
    // ── Scene Transition ───────────────────────────────────────────────────────
    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "Scene03";
    [SerializeField] private float finalFadeDuration = 1.5f;

    // ── Dialogue Sequences ────────────────────────────────────────────────────
    [Header("Dialogue Sequences")]
    [Tooltip("Dialog Inara di depan peti (AKT 2)")]
    [SerializeField] private DialogueSequenceSO cemeterySequence;
    [Tooltip("Dialog Grim mengamati Inara (AKT 3)")]
    [SerializeField] private DialogueSequenceSO grimSequence;

    // ── AKT 1: Montage ────────────────────────────────────────────────────────
    [Header("AKT 1 — Montage")]
    [Tooltip("GameObject yang tampil saat montage (jalan di kota dll)")]
    [SerializeField] private GameObject[] montageVisuals;
    [Tooltip("Durasi tiap gambar montage (detik)")]
    [SerializeField] private float montageShotDuration = 2.5f;
    [Tooltip("Durasi fade antar gambar montage")]
    [SerializeField] private float montageFadeDuration = 0.4f;

    // ── AKT 2: Pemakaman ──────────────────────────────────────────────────────
    [Header("AKT 2 — Pemakaman")]
    [Tooltip("Visual scene pemakaman — Inara berdiri di depan peti")]
    [SerializeField] private GameObject cemeteryVisual;
    [Tooltip("Delay sebelum dialog Inara dimulai (detik)")]
    [SerializeField] private float cemeteryDialogueDelay = 2f;

    // ── AKT 3: Grim ───────────────────────────────────────────────────────────
    [Header("AKT 3 — Grim Muncul")]
    [Tooltip("Visual wide shot pemakaman + Grim di kejauhan")]
    [SerializeField] private GameObject grimWideVisual;
    [Tooltip("Delay setelah CUT TO BLACK sebelum Grim tampil")]
    [SerializeField] private float grimAppearDelay = 1f;

    // ── Audio ─────────────────────────────────────────────────────────────────
    [Header("Audio")]
    [SerializeField] private AudioSource bgmSource;
    [Tooltip("Clip piano lembut untuk scene pemakaman")]
    [SerializeField] private AudioClip cemeteryBGM;
    [Tooltip("Volume saat masuk (fade in BGM)")]
    [SerializeField] private float bgmFadeInDuration = 2f;

    // ── UI / Fade ─────────────────────────────────────────────────────────────
    [Header("UI — Fade")]
    [Tooltip("Image hitam full screen untuk fade in/out")]
    [SerializeField] private Image fadePanel;

    // ── Testing ───────────────────────────────────────────────────────────────
    [Header("Testing")]
    [Tooltip("Mulai dari AKT tertentu (0=Montage, 1=Cemetery, 2=Grim)")]
    [SerializeField] [Range(0, 2)] private int startFromAkt = 0;
    [Tooltip("Centang untuk skip montage dan langsung ke Cemetery")]
    [SerializeField] private bool skipMontage = false;

    // ══════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        HideAllVisuals();
        SetFadeAlpha(1f); // Mulai dari layar hitam
        StartCoroutine(RunScene());
    }

    private IEnumerator RunScene()
    {
        // ── AKT 1: MONTAGE ────────────────────────────────────────────────────
        if (startFromAkt <= 0 && !skipMontage)
        {
            yield return StartCoroutine(Akt1_Montage());
        }
        else
        {
            yield return StartCoroutine(FadeFromBlack(0.5f));
        }

        // ── AKT 2: PEMAKAMAN ─────────────────────────────────────────────────
        if (startFromAkt <= 1)
        {
            yield return StartCoroutine(Akt2_Cemetery());
        }

        // ── AKT 3: GRIM MUNCUL ───────────────────────────────────────────────
        yield return StartCoroutine(Akt3_Grim());

        // ── FADE OUT → Scene Berikutnya ───────────────────────────────────────
        yield return StartCoroutine(FadeToBlack(finalFadeDuration));

        yield return new WaitForSeconds(0.5f);
        GoToNextScene();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // AKT 1 — Montage
    // ══════════════════════════════════════════════════════════════════════════

    private IEnumerator Akt1_Montage()
    {
        if (montageVisuals == null || montageVisuals.Length == 0)
        {
            // Tidak ada visual montage → langsung fade in ke AKT 2
            yield return StartCoroutine(FadeFromBlack(0.5f));
            yield break;
        }

        // Tiap gambar montage: fade in → tahan → fade out
        foreach (GameObject visual in montageVisuals)
        {
            if (visual == null) continue;

            visual.SetActive(true);

            yield return StartCoroutine(FadeFromBlack(montageFadeDuration));
            yield return new WaitForSeconds(montageShotDuration);
            yield return StartCoroutine(FadeToBlack(montageFadeDuration));

            visual.SetActive(false);
        }

        // Jeda kecil di hitam sebelum masuk pemakaman
        yield return new WaitForSeconds(0.3f);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // AKT 2 — Pemakaman
    // ══════════════════════════════════════════════════════════════════════════

    private IEnumerator Akt2_Cemetery()
    {
        // Tampilkan visual pemakaman
        if (cemeteryVisual != null) cemeteryVisual.SetActive(true);

        // Fade in BGM piano
        if (bgmSource != null && cemeteryBGM != null)
        {
            bgmSource.clip = cemeteryBGM;
            bgmSource.volume = 0f;
            bgmSource.Play();
            StartCoroutine(FadeAudioIn(bgmSource, bgmFadeInDuration));
        }

        // Fade masuk dari hitam
        yield return StartCoroutine(FadeFromBlack(1f));

        // Hening sebentar sebelum dialog
        yield return new WaitForSeconds(cemeteryDialogueDelay);

        // Dialog Inara di depan peti
        if (cemeterySequence != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(cemeterySequence, gameObject);

            while (DialogueManager.Instance.IsDialogueActive)
                yield return null;
        }

        // Inara menunduk dan menangis — hening sejenak
        yield return new WaitForSeconds(2.5f);

        // CUT TO BLACK
        yield return StartCoroutine(FadeToBlack(0.8f));

        if (cemeteryVisual != null) cemeteryVisual.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // AKT 3 — Grim
    // ══════════════════════════════════════════════════════════════════════════

    private IEnumerator Akt3_Grim()
    {
        // Hitam sejenak setelah CUT TO BLACK
        yield return new WaitForSeconds(grimAppearDelay);

        // Wide shot — Grim di kejauhan
        if (grimWideVisual != null) grimWideVisual.SetActive(true);

        // Fade in perlahan ke wide shot
        yield return StartCoroutine(FadeFromBlack(1.2f));

        // Hening — Grim hanya memperhatikan
        yield return new WaitForSeconds(2f);

        // Dialog Grim
        if (grimSequence != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(grimSequence, gameObject);

            while (DialogueManager.Instance.IsDialogueActive)
                yield return null;
        }

        // Grim menggenggam lentera — hening sebentar sebelum fade out
        yield return new WaitForSeconds(1.5f);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Utilities
    // ══════════════════════════════════════════════════════════════════════════

    private IEnumerator FadeToBlack(float duration)
    {
        yield return StartCoroutine(FadePanelTo(1f, duration));
    }

    private IEnumerator FadeFromBlack(float duration)
    {
        yield return StartCoroutine(FadePanelTo(0f, duration));
    }

    private IEnumerator FadePanelTo(float targetAlpha, float duration)
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float startAlpha = fadePanel.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetFadeAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }

        SetFadeAlpha(targetAlpha);

        // Sembunyikan panel saat sudah transparan penuh
        if (targetAlpha <= 0f) fadePanel.gameObject.SetActive(false);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadePanel == null) return;
        Color c = fadePanel.color;
        c.a = alpha;
        fadePanel.color = c;
    }

    private IEnumerator FadeAudioIn(AudioSource source, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        source.volume = 1f;
    }

    private void HideAllVisuals()
    {
        if (montageVisuals != null)
            foreach (var v in montageVisuals)
                if (v != null) v.SetActive(false);

        if (cemeteryVisual != null)   cemeteryVisual.SetActive(false);
        if (grimWideVisual != null)   grimWideVisual.SetActive(false);
    }

    private void GoToNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[Scene02] Next Scene Name kosong!");
            return;
        }
        Debug.Log($"[Scene02] Pindah ke scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        // Visual debugging: tampilkan label akt di scene view
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2,
            "Scene02\nAKT: 0=Montage | 1=Cemetery | 2=Grim");
        #endif
    }
}
