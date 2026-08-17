using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelPage : Page
{
    [SerializeField] private Button b_home;

    [SerializeField] private Button b_level1;

    [Header("Scene Names")]
    [SerializeField] private string sceneLevel1;

    private void Start()
    {
        b_home.onClick.AddListener(() => GameManager.Instance.ChangeState(GameState.Menu));

        b_level1.onClick.AddListener(() => ChangeScene(sceneLevel1));
    }

    private void ChangeScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
