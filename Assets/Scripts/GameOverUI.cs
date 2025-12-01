using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI 组件")]
    public GameObject gameOverPanel; // 整个UI Panel
    public TextMeshProUGUI scoreText; // 显示得分
    public Button restartButton;
    public Button quitButton;

    [Header("得分倍率")]
    public float scoreMultiplier = 100f; // 吞噬球尺寸 * 倍率 = 最终得分

    void Start()
    {
        // 初始隐藏
        gameOverPanel.SetActive(false);

        // 按钮绑定事件
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    /// <summary>
    /// 游戏结束调用
    /// </summary>
    /// <param name="playerSize">吞噬球体积</param>
    public void ShowGameOver(float playerSize)
    {
        int finalScore = Mathf.RoundToInt(playerSize * scoreMultiplier);
        scoreText.text = $"得分：{finalScore}";

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // 暂停游戏

        // 显示鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
