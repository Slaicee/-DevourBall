using UnityEngine;

public class CitySink : MonoBehaviour
{
    [Header("Visual sink speed")]
    public float sinkSpeed = 0.002f;

    [Header("Water rise logic")]
    public float waterRiseSpeed = 0.002f;
    private float invisibleWaterY = 0f;

    public float WaterLevelY
    {
        get => invisibleWaterY;
        set => invisibleWaterY = value;
    }

    [Header("Game time")]
    public float gameDuration = 600f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // Visual sinking
        transform.position += Vector3.down * sinkSpeed * Time.deltaTime;

        // Logic water rise
        invisibleWaterY += waterRiseSpeed * Time.deltaTime;

        // Check submerged objects
        CheckObjectsSink();

        // Time up
        if (timer >= gameDuration)
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.SetState(GameState.GameOver);
            else
                Time.timeScale = 0f;

            enabled = false;
        }
    }

    void CheckObjectsSink()
    {
        for (int i = EatableManager.All.Count - 1; i >= 0; i--)
        {
            Eatable e = EatableManager.All[i];
            if (invisibleWaterY >= e.requiredSize)
            {
                EatableManager.All.Remove(e);
                Destroy(e.gameObject);
            }
        }
    }
}
