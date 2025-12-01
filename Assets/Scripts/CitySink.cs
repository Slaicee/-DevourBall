using UnityEngine;
using System.Collections.Generic;

public class CitySink : MonoBehaviour
{
    [Header("视觉效果：城市下降")]
    public float sinkSpeed = 0.002f;         // 城市每秒下降多少（视觉用）

    [Header("逻辑判定：海平面上升")]
    public float waterRiseSpeed = 0.002f;   // 隐形海平面每秒上升多少
    private float invisibleWaterY = 0f;    // 逻辑海平面（真正判定用）

    [Header("游戏时长")]
    public float gameDuration = 600f;      // 10分钟
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // 1. 视觉城市下降（只是视觉）
        transform.position += Vector3.down * sinkSpeed * Time.deltaTime;

        // 2. 逻辑海平面上升（用于判断）
        invisibleWaterY += waterRiseSpeed * Time.deltaTime;

        // 3. 检查物体和玩家是否被淹
        CheckObjectsSink();

        // 4. 时间到了
        if (timer >= gameDuration)
        {
            GameOver("时间到！");
        }
    }

    // 物体淹没判定
    void CheckObjectsSink()
    {
        foreach (var e in EatableManager.All.ToArray())
        {
            // 世界高度不重要，只看 requiredSize
            if (invisibleWaterY >= e.requiredSize)
            {
                Debug.Log($"物体 {e.name} 被淹没（海平面={invisibleWaterY:F2} requiredSize={e.requiredSize})");

                EatableManager.All.Remove(e);
                Destroy(e.gameObject);
            }
        }
    }

    // 游戏结束
    void GameOver(string reason)
    {
        Debug.Log($"游戏结束：{reason}");

        // TODO：显示得分 = player.size
        Time.timeScale = 0f;
    }
}
