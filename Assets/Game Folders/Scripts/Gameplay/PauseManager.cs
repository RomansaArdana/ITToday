using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PauseManager — Sistem pause terpusat untuk "The Day After".
///
/// Fitur:
/// - Pause/Resume via tombol ESC atau tombol UI pause button
/// - Time.timeScale = 0 saat paused (semua physics dan Update berhenti)
/// - Integrasi dengan GameManager.GameState.Paused
/// - Mencegah pause saat GameOver atau game belum dimulai
/// - Button: Resume (Play), Retry (restart scene), Home (main menu)
///
/// Setup:
/// 1. Taruh script ini di GameObject "PauseManager" di scene gameplay
/// 2. Assign pausePanel (GameObject panel UI pause)
/// 3. Assign scene names: currentSceneName dan mainMenuSceneName
/// 4. Assign 3 button: resumeButton, retryButton, homeButton
/// 5. Assign pauseButton (tombol pause di HUD, opsional)
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Panel GameObject yang berisi semua UI pause. Akan di-toggle saat pause.")]
    [SerializeField] private GameObject pausePanel;

    [Tooltip("Tombol pause di HUD (opsional). Klik untuk toggle pause.")]
    [SerializeField] private UnityEngine.UI.Button pauseButton;

    [Header("Panel Buttons")]
    [Tooltip("Tombol Resume / Play — melanjutkan game.")]
    [SerializeField] private UnityEngine.UI.Button resumeButton;

    [Tooltip("Tombol Retry — restart scene yang sedang berjalan.")]
    [SerializeField] private UnityEngine.UI.Button retryButton;

    [Tooltip("Tombol Home — kembali ke main menu.")]
    [SerializeField] private UnityEngine.UI.Button homeButton;

    [Header("Scene Names")]
    [Tooltip("Nama scene main menu. Harus sama persis dengan nama di Build Settings.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Pause Behavior")]
    [Tooltip("Apakah ESC bisa membuka pause? Nonaktifkan saat cutscene atau dialogue.")]
    [SerializeField] private bool allowEscPause = true;

    [Tooltip("Apakah pause diperbolehkan saat game sedang di-GameOver?")]
    [SerializeField] private bool allowPauseDuringGameOver = false;

    // ─── State ─────────────────────────────────────────────────────────────
    public bool IsPaused { get; private set; }

    // ─── Events ────────────────────────────────────────────────────────────
    public event System.Action OnPaused;
    public event System.Action OnResumed;

    // ─── Lifecycle ─────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Pastikan panel tertutup di awal
        SetPanelVisible(false);
        IsPaused = false;
    }

    private void Start()
    {
        // Hubungkan buttons
        resumeButton?.onClick.AddListener(Resume);
        retryButton?.onClick.AddListener(Retry);
        homeButton?.onClick.AddListener(GoHome);
        pauseButton?.onClick.AddListener(TogglePause);

        // Pastikan timeScale normal saat scene dimulai
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        // Selalu kembalikan timeScale normal saat object destroyed
        // (mencegah game freeze jika scene diganti saat pause)
        if (IsPaused)
            Time.timeScale = 1f;

        if (Instance == this)
            Instance = null;
    }

    // ─── Input (ESC) ───────────────────────────────────────────────────────

    private void Update()
    {
        // Gunakan legacy Input.GetKeyDown untuk ESC agar tidak menambah
        // dependency ke InputSystem assembly (lebih ringan saat compile).
        // ESC bukan bagian dari gameplay input, jadi tidak perlu InputActionReference.
        if (allowEscPause && Input.GetKeyDown(KeyCode.Escape))
            HandleEscPressed();
    }

    private void HandleEscPressed()
    {
        // Jika paused → resume
        if (IsPaused)
        {
            Resume();
            return;
        }

        // Jika tidak paused dan kondisi allow → pause
        if (CanPause())
            Pause();
    }

    // ─── Core Pause Logic ──────────────────────────────────────────────────

    /// <summary>
    /// Toggle antara pause dan resume.
    /// Dipanggil oleh tombol pause button di HUD.
    /// </summary>
    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else if (CanPause())
            Pause();
    }

    /// <summary>
    /// Pause game: hentikan waktu, tampilkan panel, update GameState.
    /// </summary>
    public void Pause()
    {
        if (IsPaused) return;
        if (!CanPause()) return;

        IsPaused = true;
        Time.timeScale = 0f;

        SetPanelVisible(true);

        GameManager.Instance?.ChangeState(GameState.Paused);
        OnPaused?.Invoke();

        Debug.Log("[PauseManager] Game Paused");
    }

    /// <summary>
    /// Resume game: kembalikan waktu, sembunyikan panel, update GameState.
    /// </summary>
    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;

        SetPanelVisible(false);

        GameManager.Instance?.ChangeState(GameState.Gameplay);
        OnResumed?.Invoke();

        Debug.Log("[PauseManager] Game Resumed");
    }

    /// <summary>
    /// Restart scene yang sedang berjalan.
    /// Dipanggil oleh tombol Retry.
    /// </summary>
    public void Retry()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        GameManager.Instance?.ChangeState(GameState.Gameplay);

        // Reload scene saat ini
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);

        Debug.Log($"[PauseManager] Retry: reloading '{currentScene}'");
    }

    /// <summary>
    /// Kembali ke main menu.
    /// Dipanggil oleh tombol Home.
    /// </summary>
    public void GoHome()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        GameManager.Instance?.ChangeState(GameState.Menu);

        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogWarning("[PauseManager] mainMenuSceneName belum diisi di Inspector!", this);
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
        Debug.Log($"[PauseManager] GoHome: loading '{mainMenuSceneName}'");
    }

    // ─── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Cek apakah game boleh di-pause sekarang.
    /// Mencegah pause di state yang tidak tepat (GameOver, Menu, dll).
    /// </summary>
    private bool CanPause()
    {
        // Cek GameOver
        if (!allowPauseDuringGameOver)
        {
            if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameOver)
                return false;
        }

        // Hanya boleh pause saat state Gameplay
        if (GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.CurrentState;
            if (state != GameState.Gameplay && state != GameState.Paused)
                return false;
        }

        return true;
    }

    private void SetPanelVisible(bool visible)
    {
        if (pausePanel != null)
            pausePanel.SetActive(visible);
    }

    // ─── Public API (untuk dipanggil dari script lain) ─────────────────────

    /// <summary>
    /// Nonaktifkan pause sementara (misal: saat cutscene berjalan).
    /// Panggil SetEscPauseAllowed(true) setelah cutscene selesai.
    /// </summary>
    public void SetEscPauseAllowed(bool allowed)
    {
        allowEscPause = allowed;
    }
}
