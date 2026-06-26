using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public CanvasGroup pauseGroup;
    public GameObject pausePanel;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);

        StartCoroutine(FadeCanvas(pauseGroup, 0f, 1f, 0.2f));

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.Paused);
        else
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;

        StartCoroutine(FadeOutAndDisable());

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.Playing);
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator FadeOutAndDisable()
    {
        yield return StartCoroutine(FadeCanvas(pauseGroup, 1f, 0f, 0.2f));
        pausePanel.SetActive(false);
    }

    private IEnumerator FadeCanvas(CanvasGroup group, float from, float to, float time)
    {
        float t = 0f;
        group.alpha = from;

        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }

        group.alpha = to;
    }
}
