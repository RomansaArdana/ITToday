using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    public void TriggerGameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        Time.timeScale = 0f;
    }

    public void RestartGame(string sceneName)
    {
        Time.timeScale = 1f;
        IsGameOver = false;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadMainMenu(string sceneName)
    {
        Time.timeScale = 1f;
        IsGameOver = false;
        SceneManager.LoadScene(sceneName);
    }
}