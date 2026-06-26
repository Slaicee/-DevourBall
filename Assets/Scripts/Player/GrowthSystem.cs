using UnityEngine;
using Cinemachine;
using TMPro;

public class GrowthSystem : MonoBehaviour
{
    public float Size { get; private set; }
    public float Radius { get; private set; }

    private float fixedGrowValue;
    private float heightScalePerSize;
    private float maxHeightScale;
    private bool keepYEqualsRadius;
    private TextMeshProUGUI sizeDebugText;
    private bool showSizeInUI;

    private CinemachineFreeLook freeLookCam;
    private float[] initialOrbitHeights;
    private Vector3 originalPos;

    private void Awake()
    {
        freeLookCam = FindObjectOfType<CinemachineFreeLook>();
        if (freeLookCam != null)
        {
            initialOrbitHeights = new float[3];
            for (int i = 0; i < 3; i++)
            {
                initialOrbitHeights[i] = freeLookCam.m_Orbits[i].m_Height;
            }
        }

        originalPos = transform.position;
    }

    public void Initialize(float growValue, float heightScale, float maxHeight,
        bool keepY, TextMeshProUGUI debugText, bool showDebug)
    {
        fixedGrowValue = growValue;
        heightScalePerSize = heightScale;
        maxHeightScale = maxHeight;
        keepYEqualsRadius = keepY;
        sizeDebugText = debugText;
        showSizeInUI = showDebug;

        Size = 1f;
        Radius = 0.5f;
        transform.localScale = Vector3.one * Size;

        if (keepYEqualsRadius)
            UpdateBallHeight();

        UpdateSizeDebugUI();
    }

    private void Update()
    {
        if (showSizeInUI)
            UpdateSizeDebugUI();
    }

    public void Grow()
    {
        Size += fixedGrowValue;
        transform.localScale = Vector3.one * Size;
        Radius = Size * 0.5f;

        if (keepYEqualsRadius)
            UpdateBallHeight();

        UpdateCameraHeight();
        EventBus.PublishPlayerSizeChanged(Size, Radius);
    }

    public void LoadState(float newSize, float newRadius, Vector3 position)
    {
        Size = newSize;
        Radius = newRadius;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = position;
            rb.velocity = Vector3.zero;
        }
        else
        {
            transform.position = position;
        }

        transform.localScale = Vector3.one * Size;
        UpdateCameraHeight();
        EventBus.PublishPlayerSizeChanged(Size, Radius);
    }

    private void UpdateBallHeight()
    {
        transform.position = new Vector3(transform.position.x, Radius, transform.position.z);
    }

    private void UpdateCameraHeight()
    {
        if (freeLookCam == null || initialOrbitHeights == null) return;

        float heightScale = Mathf.Clamp(1 + (Size - 1) * heightScalePerSize, 1f, maxHeightScale);
        for (int i = 0; i < 3; i++)
        {
            freeLookCam.m_Orbits[i].m_Height = initialOrbitHeights[i] * heightScale;
        }
    }

    private void UpdateSizeDebugUI()
    {
        if (sizeDebugText == null) return;
        sizeDebugText.text = $"size: {Size:F2}\nradius: {Radius:F2}";
    }
}
