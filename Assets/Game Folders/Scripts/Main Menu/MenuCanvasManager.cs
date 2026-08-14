using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCanvasManager : CanvasManager
{
    private void Start()
    {
        GameManager.Instance.OnStateChange += Instance_OnStateChange;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnStateChange -= Instance_OnStateChange;
    }

    private void Instance_OnStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                SetPage(PageName.Menu);
                break;
            case GameState.Setting:
                SetPage(PageName.Setting);
                break;
            case GameState.Info:
                SetPage(PageName.Info);
                break;
            case GameState.Level:
                SetPage(PageName.Level);
                break;
            case GameState.Car:
                SetPage(PageName.Car);
                break;
        }
    }
}
