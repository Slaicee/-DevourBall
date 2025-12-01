using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [Header("特效预制体（按Tag对应拖入）")]
    public GameObject humanFX;
    public GameObject animalFX;
    public GameObject plantFX;
    public GameObject vehicleFX;
    public GameObject buildingFX;
    public GameObject streetFX;

    [Header("外观特效（挂在玩家身上）")]
    public GameObject fxHuman;
    public GameObject fxAnimal;
    public GameObject fxPlant;
    public GameObject fxVehicle;
    public GameObject fxBuilding;
    public GameObject fxStreet;

    private GameObject currentPersistentFX;
    private float effectCooldownTimer = 0f;
    private float persistentDuration = 2f;

    private void Awake()
    {
        Instance = this;
    }
    void Update()
    {
        if (effectCooldownTimer > 0)
            effectCooldownTimer -= Time.deltaTime;
    }

    // 播放吞噬球外观特效
    public void PlayPersistentEffect(string tag, Transform playerEffectHolder)
    {
        // 如果冷却中，不允许替换特效
        if (effectCooldownTimer > 0) return;

        // 清理旧特效
        if (currentPersistentFX != null) Destroy(currentPersistentFX);

        GameObject prefab = GetFXByTag(tag);
        if (prefab == null) return;

        // 实例化并挂在玩家的EffectHolder下
        currentPersistentFX = Instantiate(prefab, playerEffectHolder);
        currentPersistentFX.transform.localPosition = Vector3.zero;

        // 开始冷却计时
        effectCooldownTimer = persistentDuration;

        // 自动销毁特效
        Destroy(currentPersistentFX, persistentDuration);
    }

    GameObject GetFXByTag(string tag)
    {
        return tag switch
        {
            "Human" => fxHuman,
            "Animal" => fxAnimal,
            "Plant" => fxPlant,
            "Vehicle" => fxVehicle,
            "Building" => fxBuilding,
            "Street" => fxStreet,
            _ => null,
        };
    }

    public void PlayEffect(string tag, Eatable eatable)
    {
        GameObject prefab = GetEffectByTag(tag);
        if (prefab == null) return;

        Bounds b = eatable.GetBounds();

        // 高度 = 模型真实包围盒高度的一半 + 偏移
        float offsetY = b.extents.y + 0.2f;

        Vector3 spawnPos = b.center + Vector3.up * offsetY;
        GameObject fx = Instantiate(prefab, spawnPos, Quaternion.identity);

        // 缩放 = 包围盒对角线 * 系数
        float scaleFactor = Mathf.Clamp(b.size.magnitude * 0.2f, 0.3f, 3f);
        fx.transform.localScale = Vector3.one * scaleFactor;

        Destroy(fx, 2f);
    }

    GameObject GetEffectByTag(string tag)
    {
        switch (tag)
        {
            case "Human": return humanFX;
            case "Animal": return animalFX;
            case "Plant": return plantFX;
            case "Vehicle": return vehicleFX;
            case "Building": return buildingFX;
            case "Street": return streetFX;
        }
        return null;
    }
}
