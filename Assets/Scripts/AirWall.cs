using UnityEngine;
using TMPro;
using System.Collections;

public class AirWall : MonoBehaviour
{
    [Header("边界彩蛋配置")]
    public Transform player; // 拖拽玩家吞噬球
    public Vector3 playerStartPos; // 玩家初始起点
    public TextMeshProUGUI tipText; // 提示文本UI
    public float tipShowDuration = 2f; // 提示显示时长
    public float moveLockTime = 0.5f; // 禁止移动时长
    public float triggerDistance = 1f; // 触发阈值
    public string[] tipTexts = new string[] { // 随机提示文本
        "前面的区域以后再来探索吧",
        "这里还没开放哦",
        "别跑太远啦，快回来"
    };

    private bool isPlayerMoveLocked; // 锁定玩家移动
    private float moveLockTimer;
    private static bool isEggTriggered;

    void Start()
    {
        // 自动读取玩家初始位置
        if (player != null && playerStartPos == Vector3.zero)
        {
            playerStartPos = player.position;
        }

        // 初始化提示文本（隐藏）
        if (tipText != null)
        {
            tipText.gameObject.SetActive(false);
        }
        isEggTriggered = false;
    }

    void Update()
    {
        // 玩家移动锁定计时
        if (isPlayerMoveLocked)
        {
            moveLockTimer += Time.deltaTime;
            if (moveLockTimer >= moveLockTime)
            {
                isPlayerMoveLocked = false;
                moveLockTimer = 0;
                isEggTriggered = false; // 重置标记以便再次触发
            }
        }
    }

    // Trigger检测：玩家碰空气墙
    void OnTriggerStay(Collider other)
    {
        // 确保只有玩家进入触发器时才会触发事件
        if (other.CompareTag("Player") && !isPlayerMoveLocked && !isEggTriggered)
        {
            // 直接触发彩蛋逻辑
            TriggerBoundaryEasterEgg();
            Debug.Log("触发边界彩蛋！");
        }
    }


    void TriggerBoundaryEasterEgg()
    {
        if (player == null) return;

        isPlayerMoveLocked = true;
        isEggTriggered = true; // 全局标记，避免多墙重复触发

        // 瞬移回起点（保留Y轴高度）
        player.position = new Vector3(
            playerStartPos.x,
            player.position.y,
            playerStartPos.z
        );

        // 调试输出玩家的新位置
        Debug.Log("玩家瞬移至新位置: " + player.position);

        // 显示提示文本
        if (tipText != null)
        {
            StartCoroutine(ShowRandomTipText());
        }

        Debug.Log("碰到边界彩蛋！回到起点");
    }


    // 随机提示文本渐变
    IEnumerator ShowRandomTipText()
    {
        tipText.gameObject.SetActive(true);  // 确保文本显示
        tipText.text = tipTexts[Random.Range(0, tipTexts.Length)];

        // 渐变出现
        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            tipText.alpha = Mathf.Lerp(0, 1, t / 0.5f);  // 透明度渐变
            yield return null;
        }

        // 保持显示
        yield return new WaitForSeconds(tipShowDuration);

        // 渐变消失
        t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            tipText.alpha = Mathf.Lerp(1, 0, t / 0.5f);  // 透明度渐变
            yield return null;
        }

        tipText.gameObject.SetActive(false);  // 隐藏文本
    }

    // 供玩家移动脚本判断是否锁定
    public bool IsPlayerMoveLocked()
    {
        return isPlayerMoveLocked;
    }

    // 场景切换时重置标记
    void OnDestroy()
    {
        isEggTriggered = false;
    }
}
