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

    [Header("=== 跳过提示 ===")]
    public TextMeshProUGUI skipText;    // 按SPACE跳过

    [Header("=== 游戏主UI ===")]
    public GameObject gameUI;
    public CanvasGroup gameUIFade;

    [Header("=== 玩家脚本（拖入禁用）===")]
    public MonoBehaviour[] playerScripts;

    private Sequence seq;

    void Start()
    {
        // 黑屏 & 文本隐藏
        blackFadeGroup.alpha = 1f;
        if (text1) text1.alpha = 0;
        if (text2) text2.alpha = 0;
        if (text3) text3.alpha = 0;
        if (skipText) skipText.alpha = 1f; // 从一开始就显示

        gameUI.SetActive(false);

        // 禁用玩家脚本
        foreach (var p in playerScripts)
            if (p) p.enabled = false;

        StartCutscene();
    }

    void StartCutscene()
    {
        seq = DOTween.Sequence();

        // --- 文字1（出现→停顿→消失） ---
        seq.Append(text1.DOFade(1f, 1f));
        seq.AppendInterval(4f);
        seq.Append(text1.DOFade(0f, 1f));

        // --- 文字2 ---
        seq.Append(text2.DOFade(1f, 1f));
        seq.AppendInterval(4f);
        seq.Append(text2.DOFade(0f, 1f));

        // --- 文字3 ---
        seq.Append(text3.DOFade(1f, 1f));
        seq.AppendInterval(4f);
        seq.Append(text3.DOFade(0f, 1f));

        // --- 黑屏淡出并进入游戏 ---
        seq.Append(blackFadeGroup.DOFade(0f, 1.5f));
        seq.AppendCallback(EnterGameplay);
    }

    void Update()
    {
        // 按 Space 跳过
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EnterGameplay();
        }
    }

    void EnterGameplay()
    {
        // 防止重复调用
        if (seq != null)
        {
            seq.Kill();
            seq = null;
        }

        // 隐藏黑屏
        blackFadeGroup.DOFade(0f, 0.5f);

        // 隐藏跳过提示
        if (skipText) skipText.alpha = 0f;

        // 显示游戏 UI
        gameUI.SetActive(true);
        if (gameUIFade) gameUIFade.DOFade(1f, 1f);

        // 恢复玩家脚本
        foreach (var p in playerScripts)
            if (p) p.enabled = true;

        Destroy(gameObject, 1f);
    }
}
