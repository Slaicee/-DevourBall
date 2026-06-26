using UnityEngine;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float totalTime = 600f; // 10 min

    private float currentTime;
    private float lastKnownPlayerSize = 1f;

    void Start()
    {
        currentTime = totalTime;
    }

    void OnEnable()
    {
        EventBus.OnPlayerSizeChanged += HandlePlayerSizeChanged;
    }

    void OnDisable()
    {
        EventBus.OnPlayerSizeChanged -= HandlePlayerSizeChanged;
    }

    void Update()
    {
        if (timerText == null)
        {
            Debug.LogError("Timer Text not assigned!");
            return;
        }

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            UpdateUI();
            EventBus.PublishCountdownEnded();
            enabled = false;
            return;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void HandlePlayerSizeChanged(float size, float radius)
    {
        lastKnownPlayerSize = size;
    }

    public float GetPlayerSize()
    {
        return lastKnownPlayerSize;
    }

    public float GetRemainingTime() => currentTime;

    public void SetRemainingTime(float time)
    {
        currentTime = time;
        UpdateUI();
    }
}
