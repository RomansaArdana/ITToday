using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PauseButtonHUD — Tombol pause yang ada di HUD (pojok layar).
///
/// Script ini adalah bridge sederhana:
///   Klik tombol → PauseManager.TogglePause()
///
/// Juga menangani perubahan ikon tombol (▶ saat playing, ⏸ saat paused).
///
/// Setup:
/// 1. Attach ke GameObject Button pause di HUD
/// 2. Opsional: assign iconPlay dan iconPause (Sprite) untuk icon yang berubah
/// 3. Opsional: assign buttonImage (Image) untuk mengganti sprite
/// </summary>
[RequireComponent(typeof(Button))]
public class PauseButtonHUD : MonoBehaviour
{
    [Header("Icon Toggle (Opsional)")]
    [Tooltip("Sprite ikon saat game berjalan normal (ikon pause ⏸).")]
    [SerializeField] private Sprite iconPause;

    [Tooltip("Sprite ikon saat game sedang di-pause (ikon play ▶).")]
    [SerializeField] private Sprite iconPlay;

    [Tooltip("Image komponen pada button yang akan diganti sprite-nya.")]
    [SerializeField] private Image buttonImage;

    // ─── State ─────────────────────────────────────────────────────────────
    private Button button;

    // ─── Lifecycle ─────────────────────────────────────────────────────────

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(OnPauseButtonClicked);

        // Subscribe ke event untuk update ikon
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnPaused  += UpdateIconToPaused;
            PauseManager.Instance.OnResumed += UpdateIconToPlaying;
        }

        // Set ikon awal
        UpdateIconToPlaying();
    }

    private void OnDestroy()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnPaused  -= UpdateIconToPaused;
            PauseManager.Instance.OnResumed -= UpdateIconToPlaying;
        }
    }

    // ─── Callbacks ─────────────────────────────────────────────────────────

    private void OnPauseButtonClicked()
    {
        PauseManager.Instance?.TogglePause();
    }

    private void UpdateIconToPaused()
    {
        if (buttonImage != null && iconPlay != null)
            buttonImage.sprite = iconPlay;
    }

    private void UpdateIconToPlaying()
    {
        if (buttonImage != null && iconPause != null)
            buttonImage.sprite = iconPause;
    }
}
