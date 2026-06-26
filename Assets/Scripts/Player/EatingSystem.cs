using UnityEngine;

[RequireComponent(typeof(GrowthSystem))]
public class EatingSystem : MonoBehaviour
{
    private GrowthSystem growthSystem;
    private bool logEatEvent;

    private void Awake()
    {
        growthSystem = GetComponent<GrowthSystem>();
    }

    public void Initialize(bool logEvents)
    {
        logEatEvent = logEvents;
    }

    private void Update()
    {
        CheckEat();
    }

    private void CheckEat()
    {
        var eatList = EatableManager.All;

        for (int i = eatList.Count - 1; i >= 0; i--)
        {
            Eatable e = eatList[i];
            Bounds b = e.GetBounds();

            float dist = Vector3.Distance(transform.position, b.center);

            if (dist <= growthSystem.Radius && growthSystem.Size >= e.requiredSize)
            {
                Eat(e);
            }
        }
    }

    private void Eat(Eatable e)
    {
        float preEatSize = growthSystem.Size;

        growthSystem.Grow();

        if (logEatEvent)
        {
            Debug.Log($"Ate: {e.gameObject.name} | requiredSize: {e.requiredSize} | " +
                $"preSize: {preEatSize:F2} | postSize: {growthSystem.Size:F2} | " +
                $"radius: {growthSystem.Radius:F2}", this);
        }

        EventBus.PublishEat(e.gameObject, e.eatType, transform.position, e.GetBounds());

        EatableManager.All.Remove(e);
        Destroy(e.gameObject);
    }
}
