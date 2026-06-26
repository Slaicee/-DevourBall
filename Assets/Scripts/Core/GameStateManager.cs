using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.OpeningCutscene;
    public GameState PreviousNonPausedState { get; private set; }

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

    private void Start()
    {
        ApplyTimeAndCursor(CurrentState);
    }

    private void OnEnable()
    {
        EventBus.OnCountdownEnded += HandleCountdownEnded;
    }

    private void OnDisable()
    {
        EventBus.OnCountdownEnded -= HandleCountdownEnded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            EventBus.OnCountdownEnded -= HandleCountdownEnded;
            Instance = null;
        }
    }

    public void SetState(GameState newState)
    {
        if (!IsTransitionValid(CurrentState, newState))
        {
            Debug.LogWarning($"GameStateManager: Invalid transition {CurrentState} -> {newState}, ignored.");
            return;
        }

        GameState previous = CurrentState;
        CurrentState = newState;

        if (newState != GameState.Paused)
            PreviousNonPausedState = newState;

        ApplyTimeAndCursor(newState);
        EventBus.PublishGameStateChanged(previous, newState);
    }

    public bool IsPlaying() => CurrentState == GameState.Playing;
    public bool IsPaused() => CurrentState == GameState.Paused;

    private void ApplyTimeAndCursor(GameState state)
    {
        switch (state)
        {
            case GameState.OpeningCutscene:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }

    private bool IsTransitionValid(GameState from, GameState to)
    {
        return (from, to) switch
        {
            (GameState.OpeningCutscene, GameState.Playing) => true,
            (GameState.Playing, GameState.Paused) => true,
            (GameState.Paused, GameState.Playing) => true,
            (GameState.Playing, GameState.GameOver) => true,
            (GameState.Paused, GameState.GameOver) => true,
            _ => false,
        };
    }

    private void HandleCountdownEnded()
    {
        SetState(GameState.GameOver);
    }
}
