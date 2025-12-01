using UnityEngine;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using static Eatable;

public class PlayerEater : MonoBehaviour
{
    public float size = 1f;       // 玩家当前体积
    public float radius = 0.5f;   // 玩家初始吞噬半径

    [Header("体积增长配置")]
    public float fixedGrowValue = 0.02f; // 吞噬物体后固定增加的体积（常量）

    [Header("调试配置")]
    public bool showSizeInUI = true; // 是否在屏幕显示体积
    public TextMeshProUGUI sizeDebugText; // 拖拽显示体积的UI文本
    public bool logEatEvent = true; // 是否打印吞噬日志

    [Header("相机高度配置")]
    public float heightScalePerSize = 0.8f; // 每增大1体积，轨道高度缩放倍数
    public float maxHeightScale = 30f; // 最大缩放倍数
    private CinemachineFreeLook freeLookCam;
    private float[] initialOrbitHeights; // 存储轨道初始高度比例

    [Header("球体抬高配置")]
    public bool keepYEqualsRadius = true;
    private Vector3 originalPos; // 记录初始XZ坐标（只改Y轴）

    public ParticleSystem effectHuman;
    public ParticleSystem effectAnimal;
    public ParticleSystem effectPlant;
    public ParticleSystem effectVehicle;
    public ParticleSystem effectBuilding;
    public ParticleSystem effectStreet;

    void Start()
    {
        // 1. 初始化相机
        freeLookCam = FindObjectOfType<CinemachineFreeLook>();
        if (freeLookCam != null)
        {
            initialOrbitHeights = new float[3];
            for (int i = 0; i < 3; i++)
            {
                initialOrbitHeights[i] = freeLookCam.m_Orbits[i].m_Height;
            }
        }
        else
        {
            Debug.LogError("场景中未找到CinemachineFreeLook相机！");
        }

        // 2. 记录初始XZ坐标
        originalPos = transform.position;
        // 把Y轴设为半径
        if (keepYEqualsRadius)
        {
            transform.position = new Vector3(originalPos.x, radius, originalPos.z);
        }

        // 3. 初始化调试UI（初始显示体积）
        UpdateSizeDebugUI();
    }

    void Update()
    {
        CheckEat();
        // 实时更新体积显示（每一帧刷新）
        if (showSizeInUI)
        {
            UpdateSizeDebugUI();
        }
    }

    void CheckEat()
    {
        List<Eatable> eatList = EatableManager.All;

        for (int i = eatList.Count - 1; i >= 0; i--)
        {
            Eatable e = eatList[i];
            Bounds b = e.GetBounds();

            float dist = Vector3.Distance(transform.position, b.center);

            if (dist <= radius && size >= e.requiredSize)
            {
                Eat(e);
            }
        }
    }

    void Eat(Eatable e)
    {
        float preEatSize = size;

        // 1. 增大
        size += fixedGrowValue;
        transform.localScale = Vector3.one * size;
        radius = size * 0.5f;

        // 2. 更新球体位置
        if (keepYEqualsRadius)
            UpdateBallHeight();

        // 3. 抬高相机
        UpdateCameraHeight();

        // 4. 播放特效
        EffectManager.Instance.PlayPersistentEffect(
            e.tag,
            transform.Find("EffectHolder")
        );

        EffectManager.Instance.PlayEffect(e.tag, e);
        SoundManager.Instance.PlayEatSound(e.tag);

        // 5. 日志
        if (logEatEvent)
        {
            Debug.Log($"【吞噬日志】吞噬物体：{e.gameObject.name} | 物体requiredSize：{e.requiredSize} | 吞噬前体积：{preEatSize:F2} | 吞噬后体积：{size:F2} | 当前半径：{radius:F2}", this);
        }

        // 6. 删除
        EatableManager.All.Remove(e);
        Destroy(e.gameObject);
    }

    void PlayEffect(EatableType type)
    {
        switch (type)
        {
            case EatableType.Human:
                if (effectHuman) effectHuman.Play();
                break;

            case EatableType.Animal:
                if (effectAnimal) effectAnimal.Play();
                break;

            case EatableType.Plant:
                if (effectPlant) effectPlant.Play();
                break;

            case EatableType.Vehicle:
                if (effectVehicle) effectVehicle.Play();
                break;

            case EatableType.Building:
                if (effectBuilding) effectBuilding.Play();
                break;

            case EatableType.Street:
                if (effectStreet) effectStreet.Play();
                break;
        }
    }

    // 更新球体Y轴高度
    void UpdateBallHeight()
    {
        transform.position = new Vector3(
            transform.position.x, // 保留当前X
            radius,               // Y轴严格等于半径
            transform.position.z  // 保留当前Z
        );
    }

    // 按初始比例抬高相机轨道
    void UpdateCameraHeight()
    {
        if (freeLookCam == null || initialOrbitHeights == null) return;

        float heightScale = Mathf.Clamp(1 + (size - 1) * heightScalePerSize, 1f, maxHeightScale);
        for (int i = 0; i < 3; i++)
        {
            freeLookCam.m_Orbits[i].m_Height = initialOrbitHeights[i] * heightScale;
        }
    }

    // 新增：更新体积调试UI
    void UpdateSizeDebugUI()
    {
        if (sizeDebugText == null) return;
        // 显示体积
        sizeDebugText.text = $"当前体积：{size:F2}\n当前半径：{radius:F2}";
    }
}