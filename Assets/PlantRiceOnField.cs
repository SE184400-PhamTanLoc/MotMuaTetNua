using UnityEngine;

public class PlantRiceOnField : MonoBehaviour
{
    public GameObject ricePrefab;
    public int rows = 8;
    public int columns = 8;

    void Start()
    {
        Renderer r = GetComponent<Renderer>();
        Vector3 size = r.bounds.size;

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < columns; z++)
            {
                float posX = -size.x / 2 + size.x * (x + 0.5f) / rows;
                float posZ = -size.z / 2 + size.z * (z + 0.5f) / columns;

                Vector3 pos = transform.position + new Vector3(posX, 0, posZ);

                Instantiate(ricePrefab, pos, Quaternion.identity, transform);
            }
        }
    }
}
