using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private GameState currentState = GameState.Menu;

    public GameState CurrentState => currentState;

    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;

        OnStateChanged?.Invoke(currentState);
    }

    public bool IsState(GameState state)
    {
        return currentState == state;
    }
}

public enum GameState
{
    Menu,
    Setting,
    Info,
    Level,
    Gameplay,
    Paused,
    GameOver
}