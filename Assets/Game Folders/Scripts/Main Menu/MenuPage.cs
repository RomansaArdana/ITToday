using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPage : Page
{
    public Button b_play;
    public Button b_setting;
    public Button b_info;
    public Button b_quit;

    private void Start()
    {
        b_play.onClick.AddListener(() => GameManager.Instance.ChangeState(GameState.Level));
        b_info.onClick.AddListener(() => GameManager.Instance.ChangeState(GameState.Info));
        b_setting.onClick.AddListener(() => GameManager.Instance.ChangeState(GameState.Setting));
        b_quit.onClick.AddListener(Quit);
    }

    void Quit()
    {
        Application.Quit();
        Debug.Log("Game akan keluar...");
    }
}
