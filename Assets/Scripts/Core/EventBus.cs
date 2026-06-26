using System;
using UnityEngine;

public static class EventBus
{
    public static event Action<GameObject, Eatable.EatableType, Vector3, Bounds> OnEat;
    public static event Action<float, float> OnPlayerSizeChanged;
    public static event Action<GameState, GameState> OnGameStateChanged;
    public static event Action OnCountdownEnded;
    public static event Action<int> OnScoreChanged;

    public static void PublishEat(GameObject eatenObject, Eatable.EatableType type,
        Vector3 position, Bounds bounds)
    {
        OnEat?.Invoke(eatenObject, type, position, bounds);
    }

    public static void PublishPlayerSizeChanged(float size, float radius)
    {
        OnPlayerSizeChanged?.Invoke(size, radius);
    }

    public static void PublishGameStateChanged(GameState previous, GameState current)
    {
        OnGameStateChanged?.Invoke(previous, current);
    }

    public static void PublishCountdownEnded()
    {
        OnCountdownEnded?.Invoke();
    }

    public static void PublishScoreChanged(int score)
    {
        OnScoreChanged?.Invoke(score);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetEvents()
    {
        OnEat = null;
        OnPlayerSizeChanged = null;
        OnGameStateChanged = null;
        OnCountdownEnded = null;
        OnScoreChanged = null;
    }
}
