using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorExit : NPCBase
{
    public string sceneName;
    public GameObject pressText;

    protected override void Start()
    {
        base.Start();

        // Đảm bảo cửa luôn tương tác bằng phím E qua PlayerInteraction
        tenNPC = "ra sân";
        hanhDongTuongTac = "ra";
        quayVePhiaPlayer = false;

        // Nếu là cổng chợ (từ Làng → Chợ hoặc ngược lại) thì đổi tên cho đúng
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "VillageScene" && sceneName == "Day_28_Scene")
        {
            tenNPC = "chợ Tết";
            hanhDongTuongTac = "đi";
        }
        else if (currentScene == "Day_28_Scene" && sceneName == "VillageScene")
        {
            tenNPC = "làng";
            hanhDongTuongTac = "quay lại";
        }

        // Ẩn bất kỳ UI cũ nào kiểu "Nhấn L/O..."
        if (pressText != null)
        {
            pressText.SetActive(false);
        }

        // Tắt ChangeScene tự động (nếu còn gắn trên cửa) để không auto load khi chạm
        var autoChange = GetComponent<ChangeScene>();
        if (autoChange != null)
        {
            autoChange.enabled = false;
        }

        // Đảm bảo collider & layer đúng để PlayerInteraction bắt được
        gameObject.layer = 6; // Layer NPC mà PlayerInteraction đang dùng

        var existingCol = GetComponent<Collider>();
        bool needsTriggerBox = true;

        if (existingCol is MeshCollider meshCol)
        {
            // MeshCollider lõm không được set isTrigger, giữ nguyên cho vật lý
            meshCol.convex = meshCol.convex;
        }
        else if (existingCol != null)
        {
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
        Debug.Log($"[DoorExit] Đang chuyển sang scene: {sceneName}");
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneTransitionManager.Instance.TransitionToScene(sceneName);
        }
        KetThucTuongTac();
    }
}