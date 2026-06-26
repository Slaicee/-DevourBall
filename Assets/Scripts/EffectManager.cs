using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [Header("One-shot FX (by tag)")]
    public GameObject humanFX;
    public GameObject animalFX;
    public GameObject plantFX;
    public GameObject vehicleFX;
    public GameObject buildingFX;
    public GameObject streetFX;

    [Header("Persistent FX (on player)")]
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

    public void PlayPersistentEffect(string tag, Transform playerEffectHolder)
    {
        PlayPersistentEffect(EatableTypeExtensions.TagToType(tag), playerEffectHolder);
    }

    public void PlayPersistentEffect(Eatable.EatableType type, Transform playerEffectHolder)
    {
        if (effectCooldownTimer > 0) return;

        if (currentPersistentFX != null) Destroy(currentPersistentFX);

        GameObject prefab = GetFXByType(type);
        if (prefab == null) return;

        currentPersistentFX = Instantiate(prefab, playerEffectHolder);
        currentPersistentFX.transform.localPosition = Vector3.zero;

        effectCooldownTimer = persistentDuration;
        Destroy(currentPersistentFX, persistentDuration);
    }

    public void PlayEffect(string tag, Eatable eatable)
    {
        Bounds b = eatable.GetBounds();
        PlayEffect(EatableTypeExtensions.TagToType(tag), b.center, b);
    }

    public void PlayEffect(string tag, Vector3 position, Bounds bounds)
    {
        PlayEffect(EatableTypeExtensions.TagToType(tag), position, bounds);
    }

    private void PlayEffect(Eatable.EatableType type, Vector3 position, Bounds bounds)
    {
        GameObject prefab = GetEffectByType(type);
        if (prefab == null) return;

        float offsetY = bounds.extents.y + 0.2f;
        Vector3 spawnPos = bounds.center + Vector3.up * offsetY;
        GameObject fx = Instantiate(prefab, spawnPos, Quaternion.identity);

        float scaleFactor = Mathf.Clamp(bounds.size.magnitude * 0.2f, 0.3f, 3f);
        fx.transform.localScale = Vector3.one * scaleFactor;

        Destroy(fx, 2f);
    }

    private GameObject GetFXByType(Eatable.EatableType type)
    {
        return type switch
        {
            Eatable.EatableType.Human => fxHuman,
            Eatable.EatableType.Animal => fxAnimal,
            Eatable.EatableType.Plant => fxPlant,
            Eatable.EatableType.Vehicle => fxVehicle,
            Eatable.EatableType.Building => fxBuilding,
            Eatable.EatableType.Street => fxStreet,
            _ => null,
        };
    }

    private GameObject GetEffectByType(Eatable.EatableType type)
    {
        return type switch
        {
            Eatable.EatableType.Human => humanFX,
            Eatable.EatableType.Animal => animalFX,
            Eatable.EatableType.Plant => plantFX,
            Eatable.EatableType.Vehicle => vehicleFX,
            Eatable.EatableType.Building => buildingFX,
            Eatable.EatableType.Street => streetFX,
            _ => null,
        };
    }
}
