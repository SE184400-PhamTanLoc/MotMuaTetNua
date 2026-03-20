using UnityEngine;

/// <summary>
/// Script helper: Tự động thêm Box Collider cho các vật phẩm không có collider.
/// Gắn vào object cha chứa nhiều vật phẩm, hoặc chạy từ menu để thêm collider cho tất cả vật phẩm trong scene.
/// </summary>
public class AutoAddColliders : MonoBehaviour
{
    [ContextMenu("Thêm Collider cho tất cả vật phẩm con")]
    void AddCollidersToChildren()
    {
        AddCollidersRecursive(transform);
        Debug.Log($"Đã thêm collider cho vật phẩm dưới {gameObject.name}");
    }

    void AddCollidersRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Bỏ qua Canvas, UI, và các object đặc biệt
            if (child.GetComponent<Canvas>() != null || 
                child.name.Contains("Canvas") ||
                child.name.Contains("Hint") ||
                child.name.Contains("SitPosition"))
            {
                continue;
            }

            // Nếu object có MeshRenderer hoặc MeshFilter → có thể là vật phẩm 3D
            bool hasMesh = child.GetComponent<MeshRenderer>() != null || 
                          child.GetComponent<MeshFilter>() != null;

            if (hasMesh)
            {
                // Kiểm tra đã có collider chưa
                Collider existingCollider = child.GetComponent<Collider>();
                if (existingCollider == null)
                {
                    // Thêm Box Collider
                    BoxCollider collider = child.gameObject.AddComponent<BoxCollider>();
                    collider.isTrigger = false; // Không phải trigger để player đứng được
                    
                    #if UNITY_EDITOR
                    UnityEditor.Undo.RegisterCreatedObjectUndo(collider, "Thêm Box Collider");
                    #endif
                    
                    Debug.Log($"Đã thêm Box Collider cho: {child.name}");
                }
            }

            // Đệ quy vào con
            AddCollidersRecursive(child);
        }
    }

#if UNITY_EDITOR
    // Static method để gọi từ menu Editor
    [UnityEditor.MenuItem("Tools/Thêm Collider cho vật phẩm trong Scene")]
    static void AddCollidersToSceneObjects()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            // Bỏ qua Player, Canvas, UI
            if (obj.name.Contains("Player") || 
                obj.name.Contains("Canvas") ||
                obj.name.Contains("Hint") ||
                obj.name.Contains("SitPosition") ||
                obj.GetComponent<Canvas>() != null)
            {
                continue;
            }

            // Nếu có MeshRenderer/MeshFilter và chưa có Collider
            bool hasMesh = obj.GetComponent<MeshRenderer>() != null || 
                          obj.GetComponent<MeshFilter>() != null;
            bool hasCollider = obj.GetComponent<Collider>() != null;

            if (hasMesh && !hasCollider)
            {
                BoxCollider collider = obj.AddComponent<BoxCollider>();
                collider.isTrigger = false;
                count++;
            }
        }

        Debug.Log($"Đã thêm Box Collider cho {count} vật phẩm trong scene!");
    }
#endif
}
