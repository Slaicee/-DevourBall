using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;
    public Button restartButton;
    public Button quitButton;

    [Header("Score")]
    public float scoreMultiplier = 100f;

    void Start()
    {
        gameOverPanel.SetActive(false);

        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);

        EventBus.OnGameStateChanged += HandleGameStateChanged;
    }

    void OnDestroy()
    {
        EventBus.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState prev, GameState curr)
    {
        if (curr == GameState.GameOver)
        {
            float playerSize = GetPlayerSize();
            int finalScore = Mathf.RoundToInt(playerSize * scoreMultiplier);
            scoreText.text = $"Score: {finalScore}";

            gameOverPanel.SetActive(true);
            EventBus.PublishScoreChanged(finalScore);
        }
    }

    private float GetPlayerSize()
    {
        var growth = FindObjectOfType<GrowthSystem>();
        if (growth != null) return growth.Size;

        var countdown = FindObjectOfType<CountdownUI>();
        if (countdown != null) return countdown.GetPlayerSize();

        return 1f;
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
