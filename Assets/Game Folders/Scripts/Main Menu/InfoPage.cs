using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPage : Page
{
    public Button b_home;

    private void Start()
    {
        b_home.onClick.AddListener(() => GameManager.Instance.ChangeState(GameState.Menu));
    }
}
