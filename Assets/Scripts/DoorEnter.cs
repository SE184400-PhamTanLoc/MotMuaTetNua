using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorEnter : NPCBase
{
    public string sceneName;
    public GameObject pressText;   // UI text: "Bấm phím O..."

    protected override void Start()
    {
        base.Start();
        
        // Mặc định: cửa vào nhà
        tenNPC = "vào nhà";
        hanhDongTuongTac = "vào";
        quayVePhiaPlayer = false;

        // Nếu đây là cổng làng đi CHỢ hoặc quay lại LÀNG thì đổi câu chữ cho đúng ngữ cảnh
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "VillageScene" && sceneName == "Day_28_Scene")
        {
            // Từ Làng → Chợ
            tenNPC = "chợ Tết";
            hanhDongTuongTac = "đi";
        }
        else if (currentScene == "Day_28_Scene" && sceneName == "VillageScene")
        {
            // Từ Chợ → Làng
            tenNPC = "làng";
            hanhDongTuongTac = "quay lại";
        }

        // Ẩn UI cũ (nếu có) để chỉ dùng UI gợi ý phím E chung
        if (pressText != null)
        {
            pressText.SetActive(false);
        }

        // Tắt ChangeScene auto nếu còn gắn để không tự load khi chạm
        var autoChange = GetComponent<ChangeScene>();
        if (autoChange != null)
        {
            autoChange.enabled = false;
        }

        // Đảm bảo collider & layer đúng để PlayerInteraction bắt được
        gameObject.layer = 6;

        // Nếu collider là MeshCollider lõm, không được set isTrigger trực tiếp (Unity báo lỗi),
        // nên ta giữ MeshCollider cho va chạm, và thêm riêng một BoxCollider trigger để tương tác.
        var existingCol = GetComponent<Collider>();
        bool needsTriggerBox = true;

        if (existingCol is MeshCollider meshCol)
        {
            // Giữ mesh collider cho vật lý, không biến thành trigger
            meshCol.convex = meshCol.convex; // không đổi gì, chỉ đảm bảo không động vào isTrigger
        }
        else if (existingCol != null)
        {
            // Các loại collider còn lại có thể dùng làm trigger nếu muốn
            existingCol.isTrigger = true;
            needsTriggerBox = false;
        }

        if (needsTriggerBox)
        {
            var triggerBox = gameObject.AddComponent<BoxCollider>();
            triggerBox.isTrigger = true;
        }
    }

    protected override void OnTuongTac()
    {
        Debug.Log($"[DoorEnter] Đang chuyển sang scene: {sceneName}");
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneTransitionManager.Instance.TransitionToScene(sceneName);
        }
        KetThucTuongTac();
    }
}