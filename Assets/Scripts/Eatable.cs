using UnityEngine;

public class Eatable : MonoBehaviour
{
    [Header("Eat condition")]
    public float requiredSize = 1f;

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
        UpdateBounds();
        AutoDetectType();
    }

    void AutoDetectType()
    {
        eatType = EatableTypeExtensions.TagToType(gameObject.tag);
    }

    void LateUpdate()
    {
        UpdateBounds();
    }

    void UpdateBounds()
    {
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            bounds = rend.bounds;
            return;
        }

        coll = GetComponentInChildren<Collider>();
        if (coll != null)
        {
            bounds = coll.bounds;
            return;
        }

        bounds = new Bounds(transform.position, Vector3.one * requiredSize * 2f);
    }

    public Bounds GetBounds()
    {
        return bounds;
    }

    public float GetEffectiveSize()
    {
        return bounds.size.magnitude;
    }
}
