using UnityEngine;

[RequireComponent(typeof(GrowthSystem))]
public class SlimePlayerBridge : MonoBehaviour
{
    [Header("Slime material")]
    public Material slimeMaterial;

    [Header("Bounce")]
    public float bounceAmount = 0.05f;
    public float bounceSpeed = 2.5f;

    private GrowthSystem growthSystem;
    private GameObject visualChild;
    private float bouncePhase;

    void Start()
    {
        growthSystem = GetComponent<GrowthSystem>();

        // Clean up any previous PBF slime child
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name == "SlimeVisual" || child.name == "SlimeBody" ||
                child.GetComponent<Slime.Slime_PBF>() != null)
                Destroy(child.gameObject);
        }

        // Hide the ball mesh on the root
        var ballRenderer = GetComponent<MeshRenderer>();
        if (ballRenderer != null) ballRenderer.enabled = false;

        // Create a separate visual child that stays upright
        visualChild = new GameObject("SlimeBody");
        visualChild.transform.SetParent(transform);
        visualChild.transform.localPosition = Vector3.zero;

        var meshFilter = visualChild.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateSlimeMesh();

        var meshRenderer = visualChild.AddComponent<MeshRenderer>();
        meshRenderer.material = GetOrCreateSlimeMaterial();
        if (meshRenderer.material.renderQueue >= 2500)
            meshRenderer.material.renderQueue = 2000;

        bouncePhase = Random.Range(0f, Mathf.PI * 2f);
    }

    private Material GetOrCreateSlimeMaterial()
    {
        if (slimeMaterial != null) return slimeMaterial;

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var mat = new Material(shader);
        mat.name = "SlimeLit";
        mat.color = new Color(0.12f, 0.68f, 0.40f);
        mat.SetFloat("_Smoothness", 0.65f);
        mat.SetFloat("_Metallic", 0f);
        mat.EnableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
        // Strong emission so color stays vibrant regardless of lighting
        mat.SetColor("_EmissionColor", new Color(0.2f, 0.6f, 0.3f));
        mat.renderQueue = 2000;
        return mat;
    }

    void LateUpdate()
    {
        if (growthSystem == null || visualChild == null) return;

        // Cancel GrowthSystem's root scale — we control visual scale here
        transform.localScale = Vector3.one;

        float r = growthSystem.Radius;

        bouncePhase += Time.deltaTime * bounceSpeed;
        float bounce = 1f + Mathf.Sin(bouncePhase) * bounceAmount;

        visualChild.transform.localScale = Vector3.one * r * bounce;
        visualChild.transform.rotation = Quaternion.identity;
    }

    private static Mesh CreateSlimeMesh()
    {
        const int rings = 24;
        const int segments = 32;

        // Profile: (y, radius) — bottom to top, Y-axis symmetric
        // Max radius = 1.0 so visual scale = GrowthSystem.Radius matches eatable range
        float[] profile = {
            -1.00f, 0.80f,  // bottom edge
            -0.75f, 0.93f,  // bulge
            -0.40f, 1.00f,  // max width — slime "skirt"
             0.00f, 0.85f,  // equator
             0.30f, 0.60f,  // mid dome
             0.60f, 0.42f,  // upper dome
             0.80f, 0.35f,  // rounding
             1.00f, 0.30f,  // rounded dome crown
        };

        int vertCount = (rings + 1) * segments;
        var verts = new Vector3[vertCount];
        var norms = new Vector3[vertCount];
        var uvs = new Vector2[vertCount];
        var tris = new int[rings * segments * 6];

        for (int ring = 0; ring <= rings; ring++)
        {
            float t = ring / (float)rings;
            float y = InterpProfileY(profile, t);
            float r = InterpProfileR(profile, t);

            for (int seg = 0; seg < segments; seg++)
            {
                float angle = seg / (float)segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * r;
                float z = Mathf.Sin(angle) * r;

                int idx = ring * segments + seg;
                verts[idx] = new Vector3(x, y, z);

                float dt = 0.001f;
                float rUp = InterpProfileR(profile, Mathf.Clamp01(t + dt));
                float rDn = InterpProfileR(profile, Mathf.Clamp01(t - dt));
                float dr = (rUp - rDn) / (2f * dt);
                Vector3 tangent = new Vector3(Mathf.Cos(angle) * dr, 1f, Mathf.Sin(angle) * dr).normalized;
                Vector3 azimuthal = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle));
                norms[idx] = Vector3.Cross(tangent, azimuthal).normalized;

                uvs[idx] = new Vector2(seg / (float)segments, t);
            }
        }

        int tri = 0;
        for (int ring = 0; ring < rings; ring++)
        {
            for (int seg = 0; seg < segments; seg++)
            {
                int a = ring * segments + seg;
                int b = a + segments;
                int c = (seg + 1) % segments + ring * segments;
                int d = c + segments;

                tris[tri++] = a; tris[tri++] = b; tris[tri++] = c;
                tris[tri++] = c; tris[tri++] = b; tris[tri++] = d;
            }
        }

        var mesh = new Mesh { name = "SlimeBlob" };
        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        return mesh;
    }

    private static float InterpProfileY(float[] profile, float t)
    {
        int count = profile.Length / 2;
        float pos = t * (count - 1);
        int i = Mathf.Clamp((int)pos, 0, count - 2);
        float frac = pos - i;
        return Mathf.Lerp(profile[i * 2], profile[(i + 1) * 2], frac);
    }

    private static float InterpProfileR(float[] profile, float t)
    {
        int count = profile.Length / 2;
        float pos = t * (count - 1);
        int i = Mathf.Clamp((int)pos, 0, count - 2);
        float frac = pos - i;
        return Mathf.Lerp(profile[i * 2 + 1], profile[(i + 1) * 2 + 1], frac);
    }

    private void AutoFillMaterial()
    {
#if UNITY_EDITOR
        if (slimeMaterial == null)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("Slime t:Material");
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/Slime/"))
                {
                    slimeMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
                    break;
                }
            }
        }
#endif
    }
}
