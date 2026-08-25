using UnityEngine;

public class MenuCanvasManager : CanvasManager
{
    private void Start()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnStateChanged += Instance_OnStateChange;

        Instance_OnStateChange(GameManager.Instance.CurrentState);
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnStateChanged -= Instance_OnStateChange;
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
        }
    }
}