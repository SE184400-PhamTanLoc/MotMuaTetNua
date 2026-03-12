using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Tự động setup các component tương tác cho cảnh RoomVillage
/// </summary>
public class RoomVillageSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void TuDongSetup()
    {
        // Lấy tên scene hiện tại
        string sceneName = SceneManager.GetActiveScene().name;

        // Nếu không phải scene RoomVillage hoặc RoomScene thì bỏ qua
        if (sceneName != "RoomVillage" && sceneName != "RoomScene")
        {
            return;
        }

        Debug.Log("<color=cyan>===================================</color>");
        Debug.Log("<color=cyan>[RoomVillageSetup] 🏠 Bắt đầu tự động setup game RoomVillage...</color>");
        Debug.Log("<color=cyan>===================================</color>");

        GameObject setupObj = new GameObject("_RoomVillageSetup_Runner");
        DontDestroyOnLoad(setupObj);

        // Tạo Manager
        if (Object.FindFirstObjectByType<RoomVillageManager>() == null)
        {
            GameObject rmObj = new GameObject("RoomVillageManager");
            rmObj.AddComponent<RoomVillageManager>();
            Debug.Log("[RoomVillageSetup] Đã tạo RoomVillageManager.");
        }

        // Tạo AltarCleaningMinigame manager
        if (Object.FindFirstObjectByType<AltarCleaningMinigame>() == null)
        {
            GameObject minigameObj = new GameObject("AltarCleaningMinigame");
            minigameObj.AddComponent<AltarCleaningMinigame>();
            Debug.Log("[RoomVillageSetup] Đã tạo AltarCleaningMinigame.");
        }

        // Setup Flashback UI
        TaoFlashbackUI();

        // Setup các object tương tác
        setupObj.AddComponent<RoomVillageSetupRunner>();
    }

    private static void TaoFlashbackUI()
    {
        if (Object.FindFirstObjectByType<FlashbackController>() != null) return;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[RoomVillageSetup] Không tìm thấy Canvas để tạo Flashback UI");
            return;
        }

        GameObject panel = new GameObject("FlashbackPanel");
        panel.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = Color.white;
        // if (Resources.Load<Sprite>("FlashbackPhoto") != null) 
        //    img.sprite = Resources.Load<Sprite>("FlashbackPhoto");

        CanvasGroup cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0;

        FlashbackController controller = panel.AddComponent<FlashbackController>();
        controller.flashbackPanel = panel;
        controller.flashbackImage = img;
        controller.canvasGroup = cg;

        panel.SetActive(false);
        Debug.Log("[RoomVillageSetup] Đã tạo Flashback UI.");
    }
}

public class RoomVillageSetupRunner : MonoBehaviour
{
    void Start()
    {
        Invoke("SetupInteractions", 0.5f); // Đợi các object load xong
    }

    void SetupInteractions()
    {
        // Sử dụng FindObjectsByType và kiểm tra tên linh hoạt hơn (vì có thể là "BanTho (2)", v.v.)
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (var obj in allObjects)
        {
            string lowerName = obj.name.ToLower();

            // 1. Nguoi_Me
            if (lowerName.Contains("nguoi_me"))
            {
                if (obj.GetComponent<NguoiMeInteract>() == null)
                {
                    obj.AddComponent<NguoiMeInteract>();
                    Debug.Log("[RoomVillageSetup] ✅ Setup NguoiMeInteract cho " + obj.name);
                }
            }
            // 2. CaiKhan
            else if (lowerName.Contains("caikhan"))
            {
                if (obj.GetComponent<ClothInteract>() == null)
                {
                    obj.AddComponent<ClothInteract>();
                    Debug.Log("[RoomVillageSetup] ✅ Setup ClothInteract cho " + obj.name);
                }
            }
            // 3. BanTho
            else if (lowerName.Contains("bantho"))
            {
                if (obj.GetComponent<AltarInteract>() == null)
                {
                    obj.AddComponent<AltarInteract>();
                    Debug.Log("[RoomVillageSetup] ✅ Setup AltarInteract cho " + obj.name);
                }
            }
            // 4. Thoát (RaKhoiNha)
            else if (lowerName.Contains("rakhoinha"))
            {
                if (obj.GetComponent<DoorExit>() == null)
                {
                    var de = obj.AddComponent<DoorExit>();
                    de.sceneName = "Day_28_Scene";
                    Debug.Log("[RoomVillageSetup] ✅ Setup DoorExit cho " + obj.name);
                }
            }
        }

        Destroy(gameObject); // Setup xong thì huỷ runner
    }
}
