using UnityEngine;
using TMPro;
using DG.Tweening;

public class OpeningCutscene : MonoBehaviour
{
    [Header("=== UI ===")]
    public CanvasGroup blackFadeGroup;
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;

    [Header("=== Skip hint ===")]
    public TextMeshProUGUI skipText;

    [Header("=== In-game UI ===")]
    public GameObject gameUI;
    public CanvasGroup gameUIFade;

    [Header("=== Player scripts to enable ===")]
    public MonoBehaviour[] playerScripts;

    private Sequence seq;

    void Awake()
    {
        // Ensure text starts invisible, even before Start()
        if (text1) text1.alpha = 0;
        if (text2) text2.alpha = 0;
        if (text3) text3.alpha = 0;
        if (skipText) skipText.alpha = 0;
        if (blackFadeGroup) blackFadeGroup.alpha = 1f;
    }

    void Start()
    {
        if (HasSaveFile())
        {
            Debug.Log("Save found, skipping cutscene.");
            EnterGameplay();
            return;
        }

        // No save: play opening cutscene
        if (skipText) skipText.alpha = 1f;
        gameUI.SetActive(false);

        foreach (var p in playerScripts)
            if (p) p.enabled = false;

        StartCutscene();
    }

    void StartCutscene()
    {
        seq = DOTween.Sequence();

        seq.Append(text1.DOFade(1f, 1f));
        seq.AppendInterval(4f);
        seq.Append(text1.DOFade(0f, 1f));

        seq.Append(text2.DOFade(1f, 1f));
        seq.AppendInterval(4f);
        seq.Append(text2.DOFade(0f, 1f));

        seq.Append(text3.DOFade(1f, 1f));
        seq.AppendInterval(4f);
        seq.Append(text3.DOFade(0f, 1f));

        seq.Append(blackFadeGroup.DOFade(0f, 1.5f));
        seq.AppendCallback(EnterGameplay);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EnterGameplay();
        }
    }

    public void ForceSkip()
    {
        EnterGameplay();
    }

    void EnterGameplay()
    {
        if (seq != null)
        {
            seq.Kill();
            seq = null;
        }

        blackFadeGroup.DOFade(0f, 0.5f);
        if (skipText) skipText.alpha = 0f;

        gameUI.SetActive(true);
        if (gameUIFade) gameUIFade.DOFade(1f, 1f);

        foreach (var p in playerScripts)
            if (p) p.enabled = true;

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.Playing);

        Destroy(gameObject, 1f);
    }

    private static bool HasSaveFile()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "devour_save.json");
        if (!System.IO.File.Exists(path)) return false;
        string json = System.IO.File.ReadAllText(path);
        return json.Contains("\"playerPosX\"") && json.Contains("\"playerSize\"");
    }
}
