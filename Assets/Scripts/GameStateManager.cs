using UnityEngine;
using System.Collections;

public enum GameState
{
    MainMenu,
    NextLevel,
    Playing,
    PlayerDied,
    Paused,
    GameOver
}

public class GameStateManager : MonoBehaviour
{
	public static GameStateManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        HandleStateChange(newState);
    }

    private void HandleStateChange(GameState newState)
    {
        switch (newState)
        {
            case GameState.NextLevel:
                // Logic to handle when the game is in playing state
                //Time.timeScale = 1;
                break;
            case GameState.Playing:
                // Logic to handle when the game is in playing state
                //Time.timeScale = 1;
                break;
            case GameState.Paused:
                // Logic to handle when the game is paused
                //Time.timeScale = 0;
                break;
            case GameState.PlayerDied:
                // Logic to handle when the game is over
                ChangeState(GameState.Paused);
                StartCoroutine(PauseForSecond());
                break;
            case GameState.GameOver:
                // Logic to handle when the game is over
                ChangeState(GameState.Paused);
                StartCoroutine(PauseForSecond());
                break;
        }
    }

    public bool isPaused()
    {
        switch (CurrentState)
        {
            case GameState.Playing:
                return false;
        }

        return true;
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            ChangeState(GameState.Playing);
        }
    }

    private IEnumerator PauseForSecond()
    {
        yield return new WaitForSecondsRealtime(1); // Wait for 1 second in real time
        // Here, trigger any game over logic, like showing the game over screen
        ChangeState(GameState.Playing);
    }
}