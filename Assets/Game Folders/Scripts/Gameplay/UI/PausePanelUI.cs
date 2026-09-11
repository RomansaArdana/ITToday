using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// PausePanelUI — Menangani tampilan visual panel pause.
///
/// Fitur:
/// - Animasi fade-in / fade-out panel menggunakan CanvasGroup alpha
/// - Animasi scale pop-up saat panel muncul (efek premium)
/// - Highlight state button saat di-hover (via EventSystem)
/// - Disable tombol saat transisi animasi berlangsung
///
/// Setup:
/// 1. Attach ke root GameObject panel pause (yang punya CanvasGroup)
/// 2. Panel ini adalah child dari Canvas di scene
/// 3. Assign resumeButton, retryButton, homeButton
///
/// STRUKTUR UI YANG DISARANKAN:
///   Canvas
///   └── PausePanel (GameObject, CanvasGroup)          ← taruh script ini di sini
///       ├── Overlay (Image, warna gelap semi-transparan)
///       └── PanelCard (RectTransform, Image)          ← panel kartu putih/gelap
///           ├── Title (Text/TMP: "PAUSED")
///           ├── ButtonResume (Button: "▶ LANJUTKAN")
///           ├── ButtonRetry  (Button: "↺ ULANGI")
///           └── ButtonHome   (Button: "⌂ MENU UTAMA")
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class PausePanelUI : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button homeButton;

    [Header("Panel Card (untuk animasi scale)")]
    [Tooltip("RectTransform dari kartu panel dalam (bukan root). Yang akan di-pop-up.")]
    [SerializeField] private RectTransform panelCard;

    [Header("Animasi")]
    [Tooltip("Durasi fade-in panel saat pause dibuka.")]
    [SerializeField, Range(0.05f, 0.5f)] private float fadeInDuration = 0.15f;

    [Tooltip("Durasi fade-out panel saat pause ditutup.")]
    [SerializeField, Range(0.05f, 0.5f)] private float fadeOutDuration = 0.12f;

    [Tooltip("Apakah aktifkan animasi scale pop-up pada panelCard saat muncul?")]
    [SerializeField] private bool useScaleAnimation = true;

    [Tooltip("Scale awal panelCard saat mulai animasi masuk. 0.85 = kecil dulu, lalu membesar.")]
    [SerializeField, Range(0.5f, 1f)] private float popupStartScale = 0.88f;

    // ─── State ─────────────────────────────────────────────────────────────
    private CanvasGroup canvasGroup;
    private Coroutine animCoroutine;

    // ─── Lifecycle ─────────────────────────────────────────────────────────

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // Sembunyikan panel saat awake (sinkron dengan PauseManager)
        canvasGroup.alpha          = 0f;
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        // Subscribe ke event PauseManager
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnPaused  += PlayOpenAnimation;
            PauseManager.Instance.OnResumed += PlayCloseAnimation;
        }

        // Hubungkan buttons
        resumeButton?.onClick.AddListener(OnResumeClicked);
        retryButton?.onClick.AddListener(OnRetryClicked);
        homeButton?.onClick.AddListener(OnHomeClicked);
    }

    private void OnDisable()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnPaused  -= PlayOpenAnimation;
            PauseManager.Instance.OnResumed -= PlayCloseAnimation;
        }

        resumeButton?.onClick.RemoveListener(OnResumeClicked);
        retryButton?.onClick.RemoveListener(OnRetryClicked);
        homeButton?.onClick.RemoveListener(OnHomeClicked);
    }

    // ─── Button Callbacks ──────────────────────────────────────────────────

    private void OnResumeClicked()
    {
        SetButtonsInteractable(false);
        PauseManager.Instance?.Resume();
    }

    private void OnRetryClicked()
    {
        SetButtonsInteractable(false);
        PauseManager.Instance?.Retry();
    }

    private void OnHomeClicked()
    {
        SetButtonsInteractable(false);
        PauseManager.Instance?.GoHome();
    }

    // ─── Animasi ───────────────────────────────────────────────────────────

    private void PlayOpenAnimation()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateOpen());
    }

    private void PlayCloseAnimation()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateClose());
    }

    private IEnumerator AnimateOpen()
    {
        // Aktifkan interaksi di awal
        canvasGroup.blocksRaycasts = true;
        SetButtonsInteractable(false); // Nonaktif dulu selama animasi

        // Scale pop-up
        if (useScaleAnimation && panelCard != null)
            panelCard.localScale = Vector3.one * popupStartScale;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaledDeltaTime karena Time.timeScale = 0
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            float easedT = EaseOutCubic(t);

            canvasGroup.alpha = easedT;

            if (useScaleAnimation && panelCard != null)
            {
                float scale = Mathf.Lerp(popupStartScale, 1f, easedT);
                panelCard.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        canvasGroup.alpha = 1f;
        if (useScaleAnimation && panelCard != null)
            panelCard.localScale = Vector3.one;

        canvasGroup.interactable = true;
        SetButtonsInteractable(true);

        // Set focus ke tombol resume
        if (resumeButton != null)
            EventSystem.current?.SetSelectedGameObject(resumeButton.gameObject);

        animCoroutine = null;
    }

    private IEnumerator AnimateClose()
    {
        canvasGroup.interactable = false;
        SetButtonsInteractable(false);

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }

        canvasGroup.alpha          = 0f;
        canvasGroup.blocksRaycasts = false;

        animCoroutine = null;
    }

    // ─── Helpers ───────────────────────────────────────────────────────────

    private void SetButtonsInteractable(bool interactable)
    {
        if (resumeButton != null) resumeButton.interactable = interactable;
        if (retryButton  != null) retryButton.interactable  = interactable;
        if (homeButton   != null) homeButton.interactable   = homeButton != null && interactable;
    }

    /// <summary>Easing function: mulai lambat lalu cepat di akhir (pop feel).</summary>
    private static float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}
