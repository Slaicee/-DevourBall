using UnityEngine;

public class Eatable : MonoBehaviour
{
    [Header("吞噬需求")]
    public float requiredSize = 1f; // 球半径需 > 这个值才能吃 

    private Bounds bounds;
    private Renderer rend;
    private Collider coll;

    public enum EatableType
    {
        Human,
        Animal,
        Plant,
        Vehicle,
        Building,
        Street,
        Other
    }

    public EatableType eatType;

    void Awake()
    {
        UpdateBounds(); // 立即计算 
        AutoDetectType();
    }

    void AutoDetectType()
    {
        switch (gameObject.tag)
        {
            case "Human": eatType = EatableType.Human; break;
            case "Animal": eatType = EatableType.Animal; break;
            case "Plant": eatType = EatableType.Plant; break;
            case "Vehicle": eatType = EatableType.Vehicle; break;
            case "Building": eatType = EatableType.Building; break;
            case "Street": eatType = EatableType.Street; break;
            default: eatType = EatableType.Other; break;
        }
    }

    void LateUpdate()
    {
        UpdateBounds(); // 运行时动态更新（Scale/动画变化） 
    }

    void UpdateBounds()
    {
        // 优先 Renderer.bounds（最准） 
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            bounds = rend.bounds;
            return;
        }

        // 备用 Collider.bounds（无 Renderer 的物体） 
        coll = GetComponentInChildren<Collider>();
        if (coll != null)
        {
            bounds = coll.bounds;
            return;
        }

        // 终极备用：Transform 计算（所有物体通用） 
        bounds = new Bounds(transform.position, Vector3.one * requiredSize * 2f);
    }

    public Bounds GetBounds()
    {
        return bounds;
    }

    public float GetEffectiveSize()
    {
        return bounds.size.magnitude; // 包围盒对角线长度 
    }
}