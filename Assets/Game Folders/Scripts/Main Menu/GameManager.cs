using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameState currentState = GameState.Menu;

    public delegate void StateDelegate(GameState state);
    public event StateDelegate OnStateChange;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        OnStateChange?.Invoke(newState);
    }
}


public enum GameState
{
    Menu,
    Setting,
    Info,
    Level,
    Car
}
