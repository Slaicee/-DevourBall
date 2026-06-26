using UnityEngine;
using Cinemachine;
using TMPro;

public class PlayerEater : MonoBehaviour
{
    [Header("Player size")]
    public float size = 1f;
    public float radius = 0.5f;

    [Header("Growth config")]
    public float fixedGrowValue = 0.02f;

    [Header("Debug")]
    public bool showSizeInUI = true;
    public TextMeshProUGUI sizeDebugText;
    public bool logEatEvent = true;

    [Header("Camera height")]
    public float heightScalePerSize = 0.8f;
    public float maxHeightScale = 30f;

    [Header("Ball height")]
    public bool keepYEqualsRadius = true;

    [Header("Effects")]
    public ParticleSystem effectHuman;
    public ParticleSystem effectAnimal;
    public ParticleSystem effectPlant;
    public ParticleSystem effectVehicle;
    public ParticleSystem effectBuilding;
    public ParticleSystem effectStreet;

    [Header("Slime")]
    public bool useSlimeVisuals = true;

    private GrowthSystem growthSystem;
    private EatingSystem eatingSystem;

    public float Size => growthSystem != null ? growthSystem.Size : size;

    void Start()
    {
        growthSystem = GetComponent<GrowthSystem>();
        if (growthSystem == null) growthSystem = gameObject.AddComponent<GrowthSystem>();

        eatingSystem = GetComponent<EatingSystem>();
        if (eatingSystem == null) eatingSystem = gameObject.AddComponent<EatingSystem>();

        growthSystem.Initialize(
            fixedGrowValue,
            heightScalePerSize,
            maxHeightScale,
            keepYEqualsRadius,
            sizeDebugText,
            showSizeInUI
        );
        eatingSystem.Initialize(logEatEvent);

        EventBus.OnEat += HandleEatEffects;

        if (useSlimeVisuals)
        {
            var bridge = GetComponent<SlimePlayerBridge>();
            if (bridge == null) bridge = gameObject.AddComponent<SlimePlayerBridge>();
        }
    }

    void OnDestroy()
    {
        EventBus.OnEat -= HandleEatEffects;
    }

    private void HandleEatEffects(GameObject obj, Eatable.EatableType type,
        Vector3 pos, Bounds bounds)
    {
        string tag = EatableTypeExtensions.ToTag(type);

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayPersistentEffect(tag, transform.Find("EffectHolder"));
            EffectManager.Instance.PlayEffect(tag, pos, bounds);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEatSound(tag);
        }
    }
}
