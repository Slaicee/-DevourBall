using UnityEngine;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float totalTime = 600f; // 10 min

    private float currentTime;

    void Start()
    {
        currentTime = totalTime;
    }

    void Update()
    {
        if (timerText == null)
        {
            Debug.LogError("倒计时Text没拖！");
            return;
        }

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;

            // 获取吞噬球尺寸
            float playerSize = 1f;
            var player = FindObjectOfType<PlayerEater>();
            if (player != null) playerSize = player.size;

            // 调用GameOverUI
            var ui = FindObjectOfType<GameOverUI>();
            if (ui != null)
            {
                ui.ShowGameOver(playerSize);
            }

            enabled = false; // 停止倒计时Update()
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = $"倒计时：{minutes:00}:{seconds:00}";
    }
}
