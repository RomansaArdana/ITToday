using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelPage : Page
{
    public Button b_home;

    public Button b_level1;
    public Button b_level2;
    public Button b_level3;

    [Header("Lock Icons")]
    public GameObject lockIconLevel2;
    public GameObject lockIconLevel3;

    [Header("Scene Names")]
    public string sceneLevel1;
    public string sceneLevel2;
    public string sceneLevel3;

    private void Start()
    {
        b_home.onClick.AddListener(() => GameManager.Instance.ChangeState(GameState.Menu));

        // Level 1 selalu bisa dimainkan
        b_level1.onClick.AddListener(() => ChangeScene(sceneLevel1));

        // Level 2
        bool isMap2Unlocked = PlayerPrefs.GetInt("Map2Unlocked", 0) == 1;
        b_level2.interactable = isMap2Unlocked;
        lockIconLevel2.SetActive(!isMap2Unlocked);
        if (isMap2Unlocked)
        {
            b_level2.onClick.AddListener(() => ChangeScene(sceneLevel2));
        }

        // Level 3
        bool isMap3Unlocked = PlayerPrefs.GetInt("Map3Unlocked", 0) == 1;
        b_level3.interactable = isMap3Unlocked;
        lockIconLevel3.SetActive(!isMap3Unlocked);
        if (isMap3Unlocked)
        {
            b_level3.onClick.AddListener(() => ChangeScene(sceneLevel3));
        }
    }

    private void ChangeScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
