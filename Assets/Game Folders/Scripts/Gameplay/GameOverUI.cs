using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (GameOverManager.Instance == null) return;

        if (GameOverManager.Instance.IsGameOver && !gameOverPanel.activeSelf)
            Show();
    }

    private void Show()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void Retry()
    {
        if (GameOverManager.Instance == null) return;

        GameOverManager.Instance.RestartGame(gameplaySceneName);
    }

    public void MainMenu()
    {
        if (GameOverManager.Instance == null) return;

        GameOverManager.Instance.LoadMainMenu(mainMenuSceneName);
    }
}