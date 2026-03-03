using UnityEngine;

public class RiceFieldGenerator : MonoBehaviour
{
    public GameObject ricePrefab;

    [Header("Spacing & Density")]
    public float spacing = 4.5f;
    [Range(0f, 1f)]
    public float density = 0.12f;
    public float noiseScale = 0.2f;

    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask blockLayer;

    [Header("Avoid Road")]
    public float roadClearRadius = 3f;

    [Header("Random Look")]
    public float randomScaleMin = 0.85f;
    public float randomScaleMax = 1.05f;

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (!renderer)
        {
            Debug.LogError("Ground thiếu MeshRenderer");
            return;
        }

        Bounds bounds = renderer.bounds;

        int xCount = Mathf.FloorToInt(bounds.size.x / spacing);
        int zCount = Mathf.FloorToInt(bounds.size.z / spacing);

        Vector3 start = new Vector3(
            bounds.min.x + spacing / 2,
            bounds.max.y + 3f,
            bounds.min.z + spacing / 2
        );

        for (int x = 0; x < xCount; x++)
        {
            for (int z = 0; z < zCount; z++)
            {
                float noise = Mathf.PerlinNoise(x * noiseScale, z * noiseScale);
                if (noise > density)
                    continue;

                Vector3 rayPos = start + new Vector3(x * spacing, 0, z * spacing);

                if (Physics.CheckSphere(rayPos, roadClearRadius, blockLayer))
                    continue;

                if (Physics.Raycast(rayPos, Vector3.down, out RaycastHit hit, 6f, groundLayer))
                {
                    GameObject rice = Instantiate(
                        ricePrefab,
                        hit.point + Vector3.up * 0.02f,
                        Quaternion.identity,   // ⬅ luôn spawn thẳng
                        transform
                    );

                    // 🔥 ÉP luôn đứng thẳng trục Y
                    rice.transform.up = Vector3.up;

                    // 🌾 chỉ xoay trục Y thôi
                    rice.transform.Rotate(0, Random.Range(0f, 360f), 0);

                    float scale = Random.Range(randomScaleMin, randomScaleMax);
                    rice.transform.localScale = Vector3.one * scale;
                }
            }
        }
    }
}
