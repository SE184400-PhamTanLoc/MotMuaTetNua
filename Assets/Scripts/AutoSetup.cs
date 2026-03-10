using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// AUTO SETUP - TỰ CHẠY KHI BẤM PLAY, KHÔNG CẦN GẮN VÀO GAMEOBJECT NÀO!
/// Script này sử dụng [RuntimeInitializeOnLoadMethod] để tự động chạy
/// </summary>
public class AutoSetup : MonoBehaviour
{
    // === CÀI ĐẶT MẶC ĐỊNH ===
    private static int TIEN_BAN_DAU = 1000;
    private static int GIA_MAI_NHO = 200;
    private static int GIA_MAI_LON = 500;

    // === MÀU SẮC UI ===
    private static Color mauNenPanel = new Color(0.1f, 0.05f, 0.02f, 0.92f);
    private static Color mauChuChinh = new Color(1f, 0.95f, 0.85f, 1f);
    private static Color mauVang = new Color(1f, 0.84f, 0f, 1f);
    private static Color mauNutBinhThuong = new Color(0.6f, 0.15f, 0.1f, 1f);
    private static Color mauNutHover = new Color(0.8f, 0.25f, 0.15f, 1f);

    // === PRIVATE REFERENCES ===
    private static Canvas _canvas;
    private static GameObject _dialoguePanel;
    private static TextMeshProUGUI _tenNguoiNoiText;
    private static TextMeshProUGUI _noiDungText;
    private static GameObject _luaChonPanel;
    private static GameObject _luaChonButtonPrefab;
    private static Button _tiepTucButton;
    private static TextMeshProUGUI _tienText;
    private static GameObject _nhiemVuPanel;
    private static TextMeshProUGUI _nhiemVuText;
    private static GameObject _thongBaoPanel;
    private static TextMeshProUGUI _thongBaoText;
    private static Image _crosshairImage;
    private static GameObject _goiYTuongTacUI;
    private static GameObject _hoanThanhPanel;
    private static TextMeshProUGUI _hoanThanhText;
    private static GameObject _batDauPanel;
    private static TextMeshProUGUI _batDauText;

    /// <summary>
    /// TỰ ĐỘNG CHẠY KHI NHẤN PLAY - Không cần gắn script vào đâu cả!
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void TuDongSetup()
    {
        Debug.Log("<color=yellow>===================================</color>");
        Debug.Log("<color=yellow>[AutoSetup] 🏮 Bắt đầu tự động setup game Chợ Tết...</color>");
        Debug.Log("<color=yellow>===================================</color>");

        // Tạo 1 GameObject để chạy MonoBehaviour (cho Coroutine nếu cần)
        GameObject setupObj = new GameObject("_AutoSetup_Runner");
        DontDestroyOnLoad(setupObj);

        // Chạy từng bước với try-catch để bước này lỗi không làm các bước sau chết theo
        try { SetupGameManager(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Manager: " + e.Message); }
        try { TaoCanvas(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Canvas: " + e.Message); }
        try { TaoDialogueUI(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi DialogueUI: " + e.Message); }
        try { TaoHUD(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi HUD: " + e.Message); }
        try { TaoGoiYTuongTac(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Gợi ý: " + e.Message); }
        try { TaoCrosshair(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Crosshair: " + e.Message); }
        try { TaoLuaChonButtonPrefab(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Prefab: " + e.Message); }
        try { SetupDialogueManager(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi DialogueManager: " + e.Message); }
        try { SetupCoGaiBanMai(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi NPC: " + e.Message); }
        try { SetupPlayer(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Player: " + e.Message); }
        try { SetupGameHUD(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi GameHUD: " + e.Message); }
        // Bỏ qua tạo điểm đặt mai vì người chơi muốn hoàn thành luôn sau khi lấy
        // try { SetupDiemTraMai(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Điểm trả: " + e.Message); }
        try { SetupDiemLayMai(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Điểm lấy mai: " + e.Message); }
        // try { TaoBaoLiXi(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Lì xì: " + e.Message); }
        try { SetupBauCua(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Bầu Cua: " + e.Message); }
        try { SetupGiengNguyenUoc(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Giếng: " + e.Message); }

        // Đảm bảo có CursorStateController để khóa chuột
        if (UnityEngine.Object.FindFirstObjectByType<CursorStateController>() == null)
        {
            setupObj.AddComponent<CursorStateController>();
            Debug.Log("[AutoSetup] ✅ Thêm CursorStateController vào Runner");
        }

        // Đảm bảo GameFlow cho phép di chuyển (vì mặc định là Intro sẽ khóa movement)
        if (GameFlow.Instance != null && GameFlow.Instance.IsState(GameState.Intro))
        {
            GameFlow.Instance.ChangeState(GameState.State1_FreeOnlyChair);
        }

        Debug.Log("<color=green>===================================</color>");
        Debug.Log("<color=green>[AutoSetup] ✅ SETUP HOÀN TẤT!</color>");
        Debug.Log("<color=green>[AutoSetup] 🎮 Đi tìm cô gái bán mai và nhấn E để nói chuyện!</color>");
        Debug.Log("<color=green>===================================</color>");
    }

    // =========================================
    // BƯỚC 1: GAME MANAGER
    // =========================================
    private static void SetupGameManager()
    {
        if (FindObjectOfType<GameManager>() != null)
        {
            Debug.Log("[AutoSetup] GameManager đã tồn tại.");
            return;
        }

        GameObject gmObj = new GameObject("GameManager");
        DontDestroyOnLoad(gmObj);
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.soTienBanDau = TIEN_BAN_DAU;
        Debug.Log("[AutoSetup] ✅ Tạo GameManager (tiền: " + TIEN_BAN_DAU + "k)");
    }

    // =========================================
    // BƯỚC 2: TẠO CANVAS + UI
    // =========================================
    private static void TaoCanvas()
    {
        // Luôn tạo Canvas mới cho gameplay
        GameObject canvasObj = new GameObject("GameplayCanvas");
        _canvas = canvasObj.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Đảm bảo có EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        Debug.Log("[AutoSetup] ✅ Tạo Canvas");
    }

    private static void TaoDialogueUI()
    {
        // === Dialogue Panel ===
        _dialoguePanel = TaoPanel("DialoguePanel", _canvas.transform);
        RectTransform dpRect = _dialoguePanel.GetComponent<RectTransform>();
        dpRect.anchorMin = new Vector2(0.05f, 0.02f);
        dpRect.anchorMax = new Vector2(0.95f, 0.65f); // Tăng lên 65% màn hình để chứa được nhiều chữ hơn
        dpRect.offsetMin = Vector2.zero;
        dpRect.offsetMax = Vector2.zero;


        // === Tên người nói ===
        GameObject tenPanel = TaoPanel("TenNguoiNoiPanel", _dialoguePanel.transform);
        RectTransform tenPanelRect = tenPanel.GetComponent<RectTransform>();
        tenPanelRect.anchorMin = new Vector2(0, 1);
        tenPanelRect.anchorMax = new Vector2(0.45f, 1.12f);
        tenPanelRect.offsetMin = Vector2.zero;
        tenPanelRect.offsetMax = Vector2.zero;
        tenPanel.GetComponent<Image>().color = new Color(0.7f, 0.15f, 0.08f, 0.95f);

        _tenNguoiNoiText = TaoText("TenNguoiNoi", tenPanel.transform, "???", 36, TextAlignmentOptions.Center);
        _tenNguoiNoiText.fontStyle = FontStyles.Bold;
        _tenNguoiNoiText.color = mauVang;
        RectTransform tenRect = _tenNguoiNoiText.GetComponent<RectTransform>();
        tenRect.anchorMin = Vector2.zero;
        tenRect.anchorMax = Vector2.one;
        tenRect.offsetMin = new Vector2(10, 2);
        tenRect.offsetMax = new Vector2(-10, -2);

        // === Nội dung ===
        _noiDungText = TaoText("NoiDung", _dialoguePanel.transform, "", 36, TextAlignmentOptions.TopLeft); // Giảm nhẹ xuống 36 để cân đối không gian
        _noiDungText.color = mauChuChinh;
        _noiDungText.enableWordWrapping = true;
        _noiDungText.richText = true;
        RectTransform ndRect = _noiDungText.GetComponent<RectTransform>();
        ndRect.anchorMin = new Vector2(0.04f, 0.35f); // Dành phần dưới cho lựa chọn
        ndRect.anchorMax = new Vector2(0.96f, 0.94f);
        ndRect.offsetMin = Vector2.zero;
        ndRect.offsetMax = Vector2.zero;

        // === Lựa chọn Panel ===
        _luaChonPanel = new GameObject("LuaChonPanel");
        _luaChonPanel.transform.SetParent(_dialoguePanel.transform, false);
        RectTransform lcRect = _luaChonPanel.AddComponent<RectTransform>();
        lcRect.anchorMin = new Vector2(0.04f, 0.02f);
        lcRect.anchorMax = new Vector2(0.96f, 0.33f); // Dành 31% chiều cao panel cho các nút
        lcRect.offsetMin = Vector2.zero;
        lcRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = _luaChonPanel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10; // Tăng khoảng cách giữa các nút
        vlg.childAlignment = TextAnchor.LowerCenter;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.padding = new RectOffset(5, 5, 3, 3);

        // === Nút Tiếp Tục ===
        GameObject tiepTucObj = TaoNut("TiepTucButton", _dialoguePanel.transform, "Tiếp tục (Space)", 32);
        _tiepTucButton = tiepTucObj.GetComponent<Button>();
        RectTransform ttRect = tiepTucObj.GetComponent<RectTransform>();
        ttRect.anchorMin = new Vector2(0.60f, 0.03f);
        ttRect.anchorMax = new Vector2(0.97f, 0.14f);
        ttRect.offsetMin = Vector2.zero;
        ttRect.offsetMax = Vector2.zero;

        _dialoguePanel.SetActive(false);
        Debug.Log("[AutoSetup] ✅ Tạo Dialogue UI");
    }

    private static void TaoHUD()
    {
        // === Tiền ===
        GameObject tienBg = TaoPanel("TienBg", _canvas.transform);
        RectTransform tienBgRect = tienBg.GetComponent<RectTransform>();
        tienBgRect.anchorMin = new Vector2(0.72f, 0.935f);
        tienBgRect.anchorMax = new Vector2(0.99f, 0.985f);
        tienBgRect.offsetMin = Vector2.zero;
        tienBgRect.offsetMax = Vector2.zero;
        tienBg.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);

        _tienText = TaoText("TienText", tienBg.transform, "Tiền: 1,000,000đ", 38, TextAlignmentOptions.Right);
        _tienText.fontStyle = FontStyles.Bold;
        _tienText.color = mauVang;
        RectTransform tienInnerRect = _tienText.GetComponent<RectTransform>();
        tienInnerRect.anchorMin = Vector2.zero;
        tienInnerRect.anchorMax = Vector2.one;
        tienInnerRect.offsetMin = new Vector2(8, 0);
        tienInnerRect.offsetMax = new Vector2(-8, 0);

        // === Nhiệm vụ ===
        _nhiemVuPanel = TaoPanel("NhiemVuPanel", _canvas.transform);
        RectTransform nvRect = _nhiemVuPanel.GetComponent<RectTransform>();
        nvRect.anchorMin = new Vector2(0.01f, 0.90f);
        nvRect.anchorMax = new Vector2(0.35f, 0.985f);
        nvRect.offsetMin = Vector2.zero;
        nvRect.offsetMax = Vector2.zero;
        _nhiemVuPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);

        TextMeshProUGUI nvTitle = TaoText("NhiemVuTitle", _nhiemVuPanel.transform, "NHIỆM VỤ", 18, TextAlignmentOptions.TopLeft);
        nvTitle.fontStyle = FontStyles.Bold;
        nvTitle.color = mauVang;
        RectTransform nvTitleRect = nvTitle.GetComponent<RectTransform>();
        nvTitleRect.anchorMin = new Vector2(0.03f, 0.55f);
        nvTitleRect.anchorMax = new Vector2(0.97f, 0.95f);
        nvTitleRect.offsetMin = Vector2.zero;
        nvTitleRect.offsetMax = Vector2.zero;

        _nhiemVuText = TaoText("NhiemVuText", _nhiemVuPanel.transform, "- Mua mai về cho mẹ", 24, TextAlignmentOptions.TopLeft);
        _nhiemVuText.color = mauChuChinh;
        RectTransform nvTextRect = _nhiemVuText.GetComponent<RectTransform>();
        nvTextRect.anchorMin = new Vector2(0.03f, 0.05f);
        nvTextRect.anchorMax = new Vector2(0.97f, 0.55f);
        nvTextRect.offsetMin = Vector2.zero;
        nvTextRect.offsetMax = Vector2.zero;

        // === Thông báo ===
        _thongBaoPanel = TaoPanel("ThongBaoPanel", _canvas.transform);
        RectTransform tbRect = _thongBaoPanel.GetComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0.25f, 0.82f);
        tbRect.anchorMax = new Vector2(0.75f, 0.89f);
        tbRect.offsetMin = Vector2.zero;
        tbRect.offsetMax = Vector2.zero;
        _thongBaoPanel.GetComponent<Image>().color = new Color(0.1f, 0.5f, 0.1f, 0.85f);
        _thongBaoPanel.AddComponent<CanvasGroup>();

        _thongBaoText = TaoText("ThongBaoText", _thongBaoPanel.transform, "", 24, TextAlignmentOptions.Center);
        _thongBaoText.color = Color.white;
        RectTransform tbTextRect = _thongBaoText.GetComponent<RectTransform>();
        tbTextRect.anchorMin = Vector2.zero;
        tbTextRect.anchorMax = Vector2.one;
        tbTextRect.offsetMin = new Vector2(10, 2);
        tbTextRect.offsetMax = new Vector2(-10, -2);
        _thongBaoPanel.SetActive(false);

        // === Hoàn thành ===
        _hoanThanhPanel = TaoPanel("HoanThanhPanel", _canvas.transform);
        RectTransform htRect = _hoanThanhPanel.GetComponent<RectTransform>();
        htRect.anchorMin = new Vector2(0.2f, 0.2f);
        htRect.anchorMax = new Vector2(0.8f, 0.8f);
        htRect.offsetMin = Vector2.zero;
        htRect.offsetMax = Vector2.zero;
        _hoanThanhPanel.GetComponent<Image>().color = new Color(0.08f, 0.03f, 0.01f, 0.95f);

        _hoanThanhText = TaoText("HoanThanhText", _hoanThanhPanel.transform, "", 36, TextAlignmentOptions.Center);
        _hoanThanhText.color = mauVang;
        RectTransform htTextRect = _hoanThanhText.GetComponent<RectTransform>();
        htTextRect.anchorMin = new Vector2(0.05f, 0.05f);
        htTextRect.anchorMax = new Vector2(0.95f, 0.95f);
        htTextRect.offsetMin = Vector2.zero;
        htTextRect.offsetMax = Vector2.zero;
        _hoanThanhPanel.SetActive(false);

        // === Bắt đầu (Intro Mission) ===
        _batDauPanel = TaoPanel("BatDauPanel", _canvas.transform);
        RectTransform bdRect = _batDauPanel.GetComponent<RectTransform>();
        bdRect.anchorMin = new Vector2(0.2f, 0.2f);
        bdRect.anchorMax = new Vector2(0.8f, 0.8f);
        bdRect.offsetMin = Vector2.zero;
        bdRect.offsetMax = Vector2.zero;
        _batDauPanel.GetComponent<Image>().color = new Color(0.08f, 0.03f, 0.01f, 0.95f);

        _batDauText = TaoText("BatDauText", _batDauPanel.transform, "", 24, TextAlignmentOptions.Center);
        _batDauText.color = mauChuChinh;
        RectTransform bdTextRect = _batDauText.GetComponent<RectTransform>();
        bdTextRect.anchorMin = new Vector2(0.05f, 0.15f);
        bdTextRect.anchorMax = new Vector2(0.95f, 0.95f);
        bdTextRect.offsetMin = Vector2.zero;
        bdTextRect.offsetMax = Vector2.zero;

        // Nút Đóng intro
        GameObject dongBtnObj = TaoNut("DongIntroButton", _batDauPanel.transform, "BẮT ĐẦU NGAY (Space)");
        RectTransform dongBtnRect = dongBtnObj.GetComponent<RectTransform>();
        dongBtnRect.anchorMin = new Vector2(0.35f, 0.05f);
        dongBtnRect.anchorMax = new Vector2(0.65f, 0.15f);
        dongBtnRect.offsetMin = Vector2.zero;
        dongBtnRect.offsetMax = Vector2.zero;
        
        _batDauPanel.SetActive(false);
    }

    private static void TaoGoiYTuongTac()
    {
        _goiYTuongTacUI = TaoPanel("GoiYTuongTac", _canvas.transform);
        RectTransform gyRect = _goiYTuongTacUI.GetComponent<RectTransform>();
        gyRect.anchorMin = new Vector2(0.3f, 0.42f);
        gyRect.anchorMax = new Vector2(0.7f, 0.48f);
        gyRect.offsetMin = Vector2.zero;
        gyRect.offsetMax = Vector2.zero;
        _goiYTuongTacUI.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);

        TextMeshProUGUI gyText = TaoText("GoiYText", _goiYTuongTacUI.transform,
            "Nhấn <color=#FFD700><b>E</b></color> để nói chuyện", 28, TextAlignmentOptions.Center);
        gyText.color = Color.white;
        RectTransform gyTextRect = gyText.GetComponent<RectTransform>();
        gyTextRect.anchorMin = Vector2.zero;
        gyTextRect.anchorMax = Vector2.one;
        gyTextRect.offsetMin = new Vector2(5, 2);
        gyTextRect.offsetMax = new Vector2(-5, -2);

        _goiYTuongTacUI.SetActive(false);
        Debug.Log("[AutoSetup] ✅ Tạo Gợi ý tương tác");
    }

    private static void TaoCrosshair()
    {
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(_canvas.transform, false);
        _crosshairImage = crosshairObj.AddComponent<Image>();
        _crosshairImage.color = new Color(1, 1, 1, 0.7f);
        RectTransform crRect = crosshairObj.GetComponent<RectTransform>();
        crRect.anchorMin = new Vector2(0.5f, 0.5f);
        crRect.anchorMax = new Vector2(0.5f, 0.5f);
        crRect.sizeDelta = new Vector2(6, 6);
        crRect.anchoredPosition = Vector2.zero;
    }

    private static void TaoLuaChonButtonPrefab()
    {
        _luaChonButtonPrefab = TaoNut("LuaChonTemplate", _canvas.transform, "Lựa chọn", 32); // Tăng cỡ chữ nút lựa chọn
        LayoutElement le = _luaChonButtonPrefab.AddComponent<LayoutElement>();
        le.preferredHeight = 60; // Tăng chiều cao nút
        le.flexibleWidth = 1;
        _luaChonButtonPrefab.SetActive(false);
        Debug.Log("[AutoSetup] ✅ Tạo Button Template");
    }

    // =========================================
    // BƯỚC 3: DIALOGUE MANAGER
    // =========================================
    private static void SetupDialogueManager()
    {
        DialogueManager dm = _canvas.gameObject.AddComponent<DialogueManager>();
        dm.dialoguePanel = _dialoguePanel;
        dm.tenNguoiNoiText = _tenNguoiNoiText;
        dm.noiDungText = _noiDungText;
        dm.luaChonPanel = _luaChonPanel;
        dm.luaChonButtonPrefab = _luaChonButtonPrefab;
        dm.tiepTucButton = _tiepTucButton;
        dm.tocDoHienChu = 0.025f;
        dm.hienUngDanhChu = true;
        Debug.Log("[AutoSetup] ✅ Setup DialogueManager");
    }

    // =========================================
    // BƯỚC 4: NPC CÔ GÁI BÁN MAI
    // =========================================
    private static void SetupCoGaiBanMai()
    {
        // Tìm tất cả objects và lọc tên
        GameObject npcObj = null;

        // Cách 1: Tìm bằng tên chính xác
        npcObj = GameObject.Find("co_gai_ban_mai");

        // Cách 2: Tìm trong tất cả objects   
        if (npcObj == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allObjects)
            {
                if (go.scene.isLoaded && go.name.ToLower().Contains("co_gai"))
                {
                    npcObj = go;
                    Debug.Log($"[AutoSetup] Tìm thấy NPC: {go.name}");
                    break;
                }
            }
        }

        // Cách 3: Tìm bằng Transform root objects
        if (npcObj == null)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var rootObj in scene.GetRootGameObjects())
            {
                if (rootObj.name.ToLower().Contains("co_gai"))
                {
                    npcObj = rootObj;
                    break;
                }
                // Tìm trong con
                var found = TimTheoTenTrongCon(rootObj.transform, "co_gai");
                if (found != null)
                {
                    npcObj = found;
                    break;
                }
            }
        }

        if (npcObj == null)
        {
            Debug.LogError("[AutoSetup] ❌ KHÔNG TÌM THẤY co_gai_ban_mai! Liệt kê tất cả root objects:");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var rootObj in scene.GetRootGameObjects())
            {
                Debug.Log($"   → Root: {rootObj.name}");
            }
            return;
        }

        Debug.Log($"[AutoSetup] 🎯 Tìm thấy NPC: {npcObj.name} tại {npcObj.transform.position}");

        // Thêm Collider cho NPC nếu chưa có
        Collider existingCol = npcObj.GetComponentInChildren<Collider>();
        if (existingCol == null)
        {
            // Tính bounds từ renderers
            Renderer[] renderers = npcObj.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                BoxCollider box = npcObj.AddComponent<BoxCollider>();
                box.center = npcObj.transform.InverseTransformPoint(bounds.center);
                box.size = npcObj.transform.InverseTransformVector(bounds.size);
                // Đảm bảo size không quá nhỏ
                box.size = new Vector3(
                    Mathf.Max(box.size.x, 1f),
                    Mathf.Max(box.size.y, 2f),
                    Mathf.Max(box.size.z, 1f)
                );
                Debug.Log($"[AutoSetup] ✅ Thêm BoxCollider cho NPC: center={box.center}, size={box.size}");
            }
            else
            {
                CapsuleCollider capsule = npcObj.AddComponent<CapsuleCollider>();
                capsule.center = new Vector3(0, 1f, 0);
                capsule.radius = 1f;
                capsule.height = 2f;
                Debug.Log("[AutoSetup] ✅ Thêm CapsuleCollider mặc định cho NPC");
            }
        }
        else
        {
            Debug.Log($"[AutoSetup] NPC đã có Collider: {existingCol.GetType().Name}");
        }

        // Gắn script CoGaiBanMai
        CoGaiBanMai npc = npcObj.GetComponent<CoGaiBanMai>();
        if (npc == null)
            npc = npcObj.AddComponent<CoGaiBanMai>();

        npc.tenNPC = "Cô gái bán mai";
        npc.giaMaiNho = GIA_MAI_NHO;
        npc.giaMaiLon = GIA_MAI_LON;

        Rigidbody rb = npcObj.GetComponent<Rigidbody>();
        if (rb == null) rb = npcObj.AddComponent<Rigidbody>();
        
        rb.isKinematic = true; // Khóa hoàn toàn vật lý để không bao giờ bị ngã
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // Set layer cho NPC và tất cả con
        SetLayerRecursive(npcObj, 6);
        Debug.Log($"[AutoSetup] ✅ Setup NPC hoàn tất (layer=6)");
    }

    // =========================================
    // BƯỚC 5: PLAYER
    // =========================================
    private static void SetupPlayer()
    {
        GameObject playerObj = null;

        // Tìm FPS Controller
        var fpsController = FindObjectOfType<StarterAssets.FirstPersonController>();
        if (fpsController != null)
        {
            playerObj = fpsController.gameObject;
            Debug.Log($"[AutoSetup] Tìm thấy Player qua FirstPersonController: {playerObj.name}");
        }

        // Tìm PlayerMovement cũ
        if (playerObj == null)
        {
            var pm = FindObjectOfType<PlayerMovement>();
            if (pm != null)
            {
                playerObj = pm.gameObject;
                Debug.Log($"[AutoSetup] Tìm thấy Player qua PlayerMovement: {playerObj.name}");
            }
        }

        // Tìm bằng tag
        if (playerObj == null)
        {
            try { playerObj = GameObject.FindGameObjectWithTag("Player"); } catch { }
        }

        // Tìm bằng tên (cả Player từ RoomScene và PlayerCapsule)
        if (playerObj == null)
        {
            playerObj = GameObject.Find("Player");
        }
        if (playerObj == null)
        {
            playerObj = GameObject.Find("PlayerCapsule");
        }

        // Tìm bằng CharacterController
        if (playerObj == null)
        {
            var foundCC = UnityEngine.Object.FindFirstObjectByType<CharacterController>();
            if (foundCC != null) playerObj = foundCC.gameObject;
        }

        if (playerObj == null)
        {
            Debug.LogError("[AutoSetup] ❌ KHÔNG TÌM THẤY PLAYER!");
            return;
        }

        Debug.Log($"[AutoSetup] 🎯 Player: {playerObj.name} tại {playerObj.transform.position}");

        // Thu nhỏ CharacterController radius để đi qua hẻm hẹp
        CharacterController charCtrl = playerObj.GetComponent<CharacterController>();
        if (charCtrl != null)
        {
            charCtrl.radius = 0.25f;
            Debug.Log($"[AutoSetup] ✅ Thu nhỏ CharacterController radius = {charCtrl.radius}");
        }

        // Gắn PlayerInteraction
        PlayerInteraction pi = playerObj.GetComponent<PlayerInteraction>();
        if (pi == null)
            pi = playerObj.AddComponent<PlayerInteraction>();

        pi.khoangCachTuongTac = 3f; // Khoảng cách tương tác hợp lý
        pi.npcLayer = 1 << 6;
        pi.goiYTuongTacUI = _goiYTuongTacUI;

        Debug.Log("[AutoSetup] ✅ Setup Player hoàn tất");
    }

    // =========================================
    // BƯỚC 6: GAME HUD
    // =========================================
    private static void SetupGameHUD()
    {
        GameHUD hud = _canvas.gameObject.AddComponent<GameHUD>();
        hud.tienText = _tienText;
        hud.nhiemVuPanel = _nhiemVuPanel;
        hud.nhiemVuText = _nhiemVuText;
        hud.thongBaoPanel = _thongBaoPanel;
        hud.thongBaoText = _thongBaoText;
        hud.crosshairImage = _crosshairImage;
        hud.hoanThanhPanel = _hoanThanhPanel;
        hud.hoanThanhText = _hoanThanhText;
        hud.batDauPanel = _batDauPanel;
        hud.batDauText = _batDauText;
        hud.thoiGianThongBao = 3f;
        Debug.Log("[AutoSetup] ✅ Setup GameHUD");
    }

    // =========================================
    // BƯỚC 7: ĐIỂM TRẢ MAI
    // =========================================
    private static void SetupDiemTraMai()
    {
        Vector3 viTri = new Vector3(20, 0, 20);

        // Tìm nhà Tết
        GameObject nhaTet = GameObject.Find("nha_tet");
        if (nhaTet != null)
        {
            viTri = nhaTet.transform.position + Vector3.up * 1f;
            Debug.Log($"[AutoSetup] Đặt điểm trả mai tại nha_tet: {viTri}");
        }
        else
        {
            var player = FindObjectOfType<StarterAssets.FirstPersonController>();
            if (player != null)
                viTri = player.transform.position + player.transform.forward * 30f;
        }

        GameObject diemTraObj = new GameObject("DiemTraMai_NhaMe");
        diemTraObj.transform.position = viTri;

        BoxCollider box = diemTraObj.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(8, 5, 8);
        box.center = new Vector3(0, 2.5f, 0);

        DiemTraMai dtm = diemTraObj.AddComponent<DiemTraMai>();

        // Tạo UI gợi ý cho điểm trả mai
        GameObject goiYTraMai = TaoPanel("GoiYTraMai", _canvas.transform);
        RectTransform gytmRect = goiYTraMai.GetComponent<RectTransform>();
        gytmRect.anchorMin = new Vector2(0.3f, 0.55f);
        gytmRect.anchorMax = new Vector2(0.7f, 0.61f);
        gytmRect.offsetMin = Vector2.zero;
        gytmRect.offsetMax = Vector2.zero;
        goiYTraMai.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);

        TextMeshProUGUI gytmText = TaoText("GoiYTraMaiText", goiYTraMai.transform,
            "Nhấn <color=#FFD700><b>F</b></color> để đặt mai cho mẹ 🌼", 20, TextAlignmentOptions.Center);
        gytmText.color = Color.white;
        RectTransform gytmTextRect = gytmText.GetComponent<RectTransform>();
        gytmTextRect.anchorMin = Vector2.zero;
        gytmTextRect.anchorMax = Vector2.one;
        gytmTextRect.offsetMin = new Vector2(5, 2);
        gytmTextRect.offsetMax = new Vector2(-5, -2);

        goiYTraMai.SetActive(false);
        dtm.goiYText = goiYTraMai;

        Debug.Log("[AutoSetup] ✅ Setup Điểm trả mai");
    }

    // =========================================
    // BƯỚC 9: ĐIỂM LẤY MAI
    // =========================================
    private static void SetupDiemLayMai()
    {
        // Tìm chậu mai ngoài chợ  
        GameObject maiObj = null;
        string[] tenThuongCo = { "diem_lay_mai", "chau_mai", "DiemLayMai" };
        foreach (var ten in tenThuongCo)
        {
            maiObj = GameObject.Find(ten);
            if (maiObj != null) break;
        }

        // Nếu không tìm thấy, tạo mới gần NPC
        if (maiObj == null)
        {
            GameObject npcObj = GameObject.Find("co_gai_ban_mai");
            if (npcObj == null)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                foreach (var rootObj in scene.GetRootGameObjects())
                {
                    var found = TimTheoTenTrongCon(rootObj.transform, "co_gai");
                    if (found != null) { npcObj = found; break; }
                }
            }

            Vector3 viTri = Vector3.zero;
            if (npcObj != null)
                viTri = npcObj.transform.position + npcObj.transform.right * 3f;

            maiObj = new GameObject("DiemLayMai");
            maiObj.transform.position = viTri;
        }

        BoxCollider box = maiObj.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = maiObj.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(4, 3, 4);
            box.center = new Vector3(0, 1.5f, 0);
        }
        else
        {
            box.isTrigger = true;
        }

        DiemLayMai dlm = maiObj.GetComponent<DiemLayMai>();
        if (dlm == null)
            dlm = maiObj.AddComponent<DiemLayMai>();

        // Để goiYText = null, DiemLayMai sẽ tự fall back sang PlayerInteraction
        dlm.goiYText = null;

        Debug.Log("[AutoSetup] ✅ Setup Điểm lấy mai");
    }

    // =========================================
    // BƯỚC 10: BẦU CUA
    // =========================================
    private static void SetupBauCua()
    {
        GameObject bcObj = null;
        string[] tenThuongCo = { "hoi_danh_bai", "bau_cua", "BauCua" };
        foreach (var ten in tenThuongCo)
        {
            bcObj = GameObject.Find(ten);
            if (bcObj != null) break;
        }

        if (bcObj == null)
        {
            Debug.Log("[AutoSetup] ⚠️ Không tìm thấy đối tượng Bầu Cua, bỏ qua.");
            return;
        }

        // Giữ lại bản cũ: chỉ thêm script & Rigidbody
        BauCuaMinigame bc = bcObj.GetComponent<BauCuaMinigame>();
        if (bc == null)
            bc = bcObj.AddComponent<BauCuaMinigame>();

        bc.tenNPC = "Sòng Bầu Cua";
        bc.quayVePhiaPlayer = false; // Không quay đầu

        Rigidbody rb = bcObj.GetComponent<Rigidbody>();
        if (rb == null) rb = bcObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // Thu nhỏ BoxCollider
        BoxCollider box = bcObj.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = bcObj.AddComponent<BoxCollider>();
        }
        // Tính toán lại center dựa trên renderers để tránh bị lòi ra ngoài
        Renderer[] renderers = bcObj.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            box.center = bcObj.transform.InverseTransformPoint(bounds.center);
        }
        else
        {
            box.center = new Vector3(0, 1f, 0);
        }
        box.size = new Vector3(0.5f, 2f, 0.5f);

        // Chuyển collider con thành trigger
        Collider[] childCols = bcObj.GetComponentsInChildren<Collider>();
        foreach (var col in childCols)
        {
            if (col.gameObject != bcObj)
            {
                col.isTrigger = true;
            }
        }

        SetLayerRecursive(bcObj, 6);

        // TẠO UI CHO BẦU CUA
        GameObject bcPanel = TaoPanel("BauCuaPanel", _canvas.transform);
        bcPanel.GetComponent<Image>().color = new Color(0.2f, 0.05f, 0.0f, 0.95f);
        
        RectTransform bcPanelRect = bcPanel.GetComponent<RectTransform>();
        bcPanelRect.anchorMin = new Vector2(0.05f, 0.05f);
        bcPanelRect.anchorMax = new Vector2(0.95f, 0.95f);
        bcPanelRect.offsetMin = Vector2.zero;
        bcPanelRect.offsetMax = Vector2.zero;

        // TIÊU ĐỀ
        TextMeshProUGUI txtTieuDe = TaoText("TieuDe", bcPanel.transform, "SÒNG BẦU CUA - TẾT", 50, TextAlignmentOptions.Center);
        txtTieuDe.color = mauVang;
        txtTieuDe.fontStyle = FontStyles.Bold;
        RectTransform tdRect = txtTieuDe.GetComponent<RectTransform>();
        tdRect.anchorMin = new Vector2(0, 0.92f);
        tdRect.anchorMax = new Vector2(1, 1);
        tdRect.offsetMin = Vector2.zero;
        tdRect.offsetMax = Vector2.zero;

        // XÚC XẮC
        GameObject xxContainer = new GameObject("XucXacContainer");
        xxContainer.transform.SetParent(bcPanel.transform, false);
        RectTransform xxRect = xxContainer.AddComponent<RectTransform>();
        xxRect.anchorMin = new Vector2(0.25f, 0.72f);
        xxRect.anchorMax = new Vector2(0.75f, 0.9f);
        xxRect.offsetMin = Vector2.zero;
        xxRect.offsetMax = Vector2.zero;

        HorizontalLayoutGroup hLayout = xxContainer.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 15;
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;

        bc.textXucXac = new TextMeshProUGUI[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject xxBg = TaoPanel($"XucXac_{i}", xxContainer.transform);
            xxBg.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.8f, 1f);
            
            TextMeshProUGUI xxText = TaoText($"Text_{i}", xxBg.transform, "?", 45, TextAlignmentOptions.Center);
            xxText.color = Color.black;
            xxText.fontStyle = FontStyles.Bold;
            RectTransform tRect = xxText.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            
            bc.textXucXac[i] = xxText;
        }

        // KẾT QUẢ
        bc.textKetQua = TaoText("TextKetQua", bcPanel.transform, "Bơm tiền vào con mình thích rồi Lắc nhé!", 30, TextAlignmentOptions.Center);
        bc.textKetQua.color = Color.white;
        RectTransform kqRect = bc.textKetQua.GetComponent<RectTransform>();
        kqRect.anchorMin = new Vector2(0.1f, 0.6f);
        kqRect.anchorMax = new Vector2(0.9f, 0.7f);
        kqRect.offsetMin = Vector2.zero;
        kqRect.offsetMax = Vector2.zero;

        // BÀN CƯỢC 6 Ô
        GameObject banCuoc = new GameObject("BanCuoc");
        banCuoc.transform.SetParent(bcPanel.transform, false);
        RectTransform banRect = banCuoc.AddComponent<RectTransform>();
        banRect.anchorMin = new Vector2(0.05f, 0.15f);
        banRect.anchorMax = new Vector2(0.95f, 0.58f);
        banRect.offsetMin = Vector2.zero;
        banRect.offsetMax = Vector2.zero;

        GridLayoutGroup grid = banCuoc.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(300, 180);
        grid.spacing = new Vector2(15, 15);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        string[] tenConVat = { "Bầu", "Cua", "Tôm", "Cá", "Gà", "Nai" };
        bc.oCuoc = new BauCuaMinigame.OCuoc[6];
        for (int i = 0; i < 6; i++)
        {
            GameObject oBg = TaoPanel($"OCuoc_{tenConVat[i]}", banCuoc.transform);
            oBg.GetComponent<Image>().color = new Color(0.15f, 0.35f, 0.15f, 1f);

            TextMeshProUGUI tenText = TaoText("TenCon", oBg.transform, tenConVat[i], 36, TextAlignmentOptions.Center);
            tenText.fontStyle = FontStyles.Bold;
            tenText.color = mauVang;
            RectTransform tenCRect = tenText.GetComponent<RectTransform>();
            tenCRect.anchorMin = new Vector2(0, 0.6f);
            tenCRect.anchorMax = new Vector2(1, 1);
            tenCRect.offsetMin = Vector2.zero;
            tenCRect.offsetMax = Vector2.zero;

            TextMeshProUGUI cuocText = TaoText("SoTien", oBg.transform, "0đ", 28, TextAlignmentOptions.Center);
            cuocText.color = Color.white;
            RectTransform cuocRect = cuocText.GetComponent<RectTransform>();
            cuocRect.anchorMin = new Vector2(0, 0.3f);
            cuocRect.anchorMax = new Vector2(1, 0.6f);
            cuocRect.offsetMin = Vector2.zero;
            cuocRect.offsetMax = Vector2.zero;

            // Nút 10k và 50k
            GameObject nutContainer = new GameObject("NutContainer");
            nutContainer.transform.SetParent(oBg.transform, false);
            RectTransform ncRect = nutContainer.AddComponent<RectTransform>();
            ncRect.anchorMin = new Vector2(0, 0);
            ncRect.anchorMax = new Vector2(1, 0.3f);
            ncRect.offsetMin = Vector2.zero;
            ncRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup nutLayout = nutContainer.AddComponent<HorizontalLayoutGroup>();
            nutLayout.spacing = 5;
            nutLayout.childAlignment = TextAnchor.MiddleCenter;
            nutLayout.childControlWidth = true;
            nutLayout.childControlHeight = true;

            GameObject btn10k = TaoNut("Btn10k", nutContainer.transform, "+10k", 22);
            btn10k.GetComponent<Image>().color = new Color(0.3f, 0.5f, 0.3f, 1f);
            
            GameObject btn50k = TaoNut("Btn50k", nutContainer.transform, "+50k", 22);
            btn50k.GetComponent<Image>().color = new Color(0.5f, 0.3f, 0.1f, 1f);

            int idx = i;
            bc.oCuoc[i] = new BauCuaMinigame.OCuoc
            {
                tenCon = tenConVat[i],
                textSoTienCuoc = cuocText
            };

            btn10k.GetComponent<Button>().onClick.AddListener(() => bc.DatCuoc(idx, 10));
            btn50k.GetComponent<Button>().onClick.AddListener(() => bc.DatCuoc(idx, 50));
        }

        // NÚT LẮC
        GameObject btnLacObj = TaoNut("BtnLac", bcPanel.transform, "LẮC!", 36);
        btnLacObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.1f, 1f);
        bc.btnLac = btnLacObj.GetComponent<Button>();
        bc.btnLac.onClick.AddListener(() => bc.LacXucXac());
        RectTransform lacRect = btnLacObj.GetComponent<RectTransform>();
        lacRect.anchorMin = new Vector2(0.4f, 0.02f);
        lacRect.anchorMax = new Vector2(0.6f, 0.12f);
        lacRect.offsetMin = Vector2.zero;
        lacRect.offsetMax = Vector2.zero;
        
        // NÚT ĐẶT LẠI (HOÀN TIỀN)
        GameObject btnHuyObj = TaoNut("BtnHuy", bcPanel.transform, "ĐẶT LẠI", 24);
        btnHuyObj.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f);
        bc.btnDatLai = btnHuyObj.GetComponent<Button>();
        bc.btnDatLai.onClick.AddListener(() => bc.DatLaiCuoc());
        RectTransform huyRect = btnHuyObj.GetComponent<RectTransform>();
        huyRect.anchorMin = new Vector2(0.2f, 0.02f);
        huyRect.anchorMax = new Vector2(0.35f, 0.12f);
        huyRect.offsetMin = Vector2.zero;
        huyRect.offsetMax = Vector2.zero;

        // NÚT THOÁT
        GameObject btnThoatObj = TaoNut("BtnThoat", bcPanel.transform, "THOÁT", 24);
        btnThoatObj.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f, 1f);
        bc.btnThoat = btnThoatObj.GetComponent<Button>();
        bc.btnThoat.onClick.AddListener(() => bc.DongSoba());
        RectTransform thoatRect = btnThoatObj.GetComponent<RectTransform>();
        thoatRect.anchorMin = new Vector2(0.65f, 0.02f);
        thoatRect.anchorMax = new Vector2(0.8f, 0.12f);
        thoatRect.offsetMin = Vector2.zero;
        thoatRect.offsetMax = Vector2.zero;

        // TEXT TỔNG TIỀN
        bc.textTongTien = TaoText("TextTienHienTai", bcPanel.transform, "Tiền trong túi: ?", 28, TextAlignmentOptions.Right);
        bc.textTongTien.color = mauVang;
        RectTransform tienRect = bc.textTongTien.GetComponent<RectTransform>();
        tienRect.anchorMin = new Vector2(0.7f, 0.9f);
        tienRect.anchorMax = new Vector2(0.95f, 0.98f);
        tienRect.offsetMin = Vector2.zero;
        tienRect.offsetMax = Vector2.zero;

        bc.uiPanel = bcPanel;
        bcPanel.SetActive(false);

        Debug.Log("[AutoSetup] ✅ Setup Bầu Cua");
    }

    // =========================================
    // KHU VỰC GIẾNG NGUYỆN ƯỚC
    // =========================================
    private static void SetupGiengNguyenUoc()
    {
        // 1. Tìm cái giếng trong Scene
        GameObject giengObj = null;
        
        // Cố gắng tìm bằng tên thường thấy
        string[] tenThuongCo = { "gieng", "well", "gieng_nuoc", "water_well", "cai_gieng" };
        foreach (var ten in tenThuongCo)
        {
            giengObj = GameObject.Find(ten);
            if (giengObj != null) break;
        }

        if (giengObj == null)
        {
            Debug.Log("[AutoSetup] ⚠️ Không tìm thấy đối tượng tên 'gieng/cai_gieng', bỏ qua Giếng Nguyện Ước.");
            return;
        }

        // 2. Thêm script và Collider nếu chưa có
        GiengNguyenUoc giengScript = giengObj.GetComponent<GiengNguyenUoc>();
        if (giengScript == null)
        {
            giengScript = giengObj.AddComponent<GiengNguyenUoc>();
        }

        BoxCollider boxCol = giengObj.GetComponent<BoxCollider>();
        if (boxCol == null) 
        {
            // Nếu có collider loại khác (như MeshCollider), tắt nó đi để dùng BoxCollider cho gọn
            Collider existingCol = giengObj.GetComponent<Collider>();
            if (existingCol != null) existingCol.enabled = false;
            
            boxCol = giengObj.AddComponent<BoxCollider>();
        }

        // Luôn tính toán lại center và size cho giếng
        Renderer[] giengRends = giengObj.GetComponentsInChildren<Renderer>();
        if (giengRends.Length > 0)
        {
            Bounds bounds = giengRends[0].bounds;
            foreach (var r in giengRends) bounds.Encapsulate(r.bounds);
            boxCol.center = giengObj.transform.InverseTransformPoint(bounds.center);
        }
        else
        {
            boxCol.center = new Vector3(0, 1f, 0);
        }
        boxCol.size = new Vector3(1.0f, 2f, 1.0f);
        boxCol.isTrigger = false; // Đảm bảo là vật thể đặc để không đi xuyên qua

        SetLayerRecursive(giengObj, 6);

        // 3. TẠO UI CHO GIẾNG
        GameObject pnlNguyenUoc = TaoPanel("GiengNguyenUocPanel", _canvas.transform);
        pnlNguyenUoc.GetComponent<Image>().color = new Color(0.1f, 0.4f, 0.8f, 0.95f);
        
        RectTransform nuiRect = pnlNguyenUoc.GetComponent<RectTransform>();
        nuiRect.anchorMin = new Vector2(0.2f, 0.2f);
        nuiRect.anchorMax = new Vector2(0.8f, 0.8f);
        nuiRect.offsetMin = Vector2.zero;
        nuiRect.offsetMax = Vector2.zero;

        // Tiêu đề
        TextMeshProUGUI txtTieuDeGieng = TaoText("TxtTieuDe", pnlNguyenUoc.transform, "GIẾNG NGUYỆN ƯỚC", 40, TextAlignmentOptions.Center);
        txtTieuDeGieng.color = new Color(1f, 0.8f, 0.2f, 1f);
        txtTieuDeGieng.fontStyle = FontStyles.Bold;
        RectTransform tdGiengRect = txtTieuDeGieng.GetComponent<RectTransform>();
        tdGiengRect.anchorMin = new Vector2(0, 0.8f);
        tdGiengRect.anchorMax = new Vector2(1, 1);
        tdGiengRect.offsetMin = Vector2.zero;
        tdGiengRect.offsetMax = Vector2.zero;

        // Hướng dẫn
        TextMeshProUGUI txtHuongDan = TaoText("TxtHuongDan", pnlNguyenUoc.transform, "Thành tâm viết một điều ước và ném 10.000đ xuống giếng:", 24, TextAlignmentOptions.Center);
        RectTransform hdRect = txtHuongDan.GetComponent<RectTransform>();
        hdRect.anchorMin = new Vector2(0.1f, 0.65f);
        hdRect.anchorMax = new Vector2(0.9f, 0.75f);
        hdRect.offsetMin = Vector2.zero;
        hdRect.offsetMax = Vector2.zero;

        // Input Field cho điều ước
        GameObject inputObj = new GameObject("InputField_DieuUoc");
        inputObj.transform.SetParent(pnlNguyenUoc.transform, false);
        Image bgImg = inputObj.AddComponent<Image>();
        bgImg.color = new Color(1, 1, 1, 0.8f);
        
        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.1f, 0.35f);
        inputRect.anchorMax = new Vector2(0.9f, 0.6f);
        inputRect.offsetMin = Vector2.zero;
        inputRect.offsetMax = Vector2.zero;

        // Text area of input field
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 10);
        textAreaRect.offsetMax = new Vector2(-10, -10);

        TextMeshProUGUI inputText = TaoText("Text", textArea.transform, "", 28, TextAlignmentOptions.TopLeft);
        inputText.color = Color.black;
        RectTransform itRect = inputText.GetComponent<RectTransform>();
        itRect.anchorMin = Vector2.zero;
        itRect.anchorMax = Vector2.one;
        itRect.offsetMin = Vector2.zero;
        itRect.offsetMax = Vector2.zero;

        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        inputField.textComponent = inputText;
        inputField.targetGraphic = bgImg;

        // Placeholder
        TextMeshProUGUI phText = TaoText("Placeholder", textArea.transform, "Ví dụ: Con ước năm mới có gấu...", 28, TextAlignmentOptions.TopLeft);
        phText.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        RectTransform phRect = phText.GetComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = Vector2.zero;
        phRect.offsetMax = Vector2.zero;
        inputField.placeholder = phText;

        // Nút Ước
        GameObject btnUocObj = TaoNut("BtnUoc", pnlNguyenUoc.transform, "NÉM XU VÀ ƯỚC", 24);
        btnUocObj.GetComponent<Image>().color = new Color(0.1f, 0.6f, 0.2f, 1f);
        RectTransform uocRect = btnUocObj.GetComponent<RectTransform>();
        uocRect.anchorMin = new Vector2(0.35f, 0.15f);
        uocRect.anchorMax = new Vector2(0.65f, 0.28f);
        uocRect.offsetMin = Vector2.zero;
        uocRect.offsetMax = Vector2.zero;

        // Nút Đóng
        GameObject btnDongObj = TaoNut("BtnDong", pnlNguyenUoc.transform, "THÔI", 20);
        btnDongObj.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f, 1f);
        RectTransform dongRect = btnDongObj.GetComponent<RectTransform>();
        dongRect.anchorMin = new Vector2(0.7f, 0.15f);
        dongRect.anchorMax = new Vector2(0.9f, 0.25f);
        dongRect.offsetMin = Vector2.zero;
        dongRect.offsetMax = Vector2.zero;

        // Gắn references vào script
        giengScript.panelUocNguyen = pnlNguyenUoc;
        giengScript.inputDieuUoc = inputField;
        giengScript.btnGuiDieuUoc = btnUocObj.GetComponent<Button>();
        giengScript.btnDongPanel = btnDongObj.GetComponent<Button>();

        pnlNguyenUoc.SetActive(false);

        Debug.Log("[AutoSetup] ✅ Setup Giếng Nguyện Ước hoàn tất");
    }

    // =========================================
    // TIỆN ÍCH
    // =========================================

    private static GameObject TaoPanel(string ten, Transform parent)
    {
        GameObject panel = new GameObject(ten);
        panel.transform.SetParent(parent, false);
        Image img = panel.AddComponent<Image>();
        img.color = mauNenPanel;
        return panel;
    }

    private static TextMeshProUGUI TaoText(string ten, Transform parent, string noiDung, float size, TextAlignmentOptions alignment)
    {
        GameObject textObj = new GameObject(ten);
        textObj.transform.SetParent(parent, false);
        textObj.layer = 5;

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        
        if (tmp.font == null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }

        tmp.text = noiDung;
        tmp.fontSize = size;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;
        tmp.richText = true;
        tmp.raycastTarget = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.color = Color.white;

        tmp.ForceMeshUpdate();
        return tmp;
    }

    private static GameObject TaoNut(string ten, Transform parent, string noiDung, float fontSize = 20)
    {
        GameObject btnObj = new GameObject(ten);
        btnObj.transform.SetParent(parent, false);
        btnObj.layer = 5; // Layer UI bắt buộc để GraphicRaycaster nhận diện click

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = mauNutBinhThuong;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = mauNutBinhThuong;
        colors.highlightedColor = mauNutHover;
        colors.pressedColor = new Color(0.9f, 0.3f, 0.1f, 1f);
        colors.selectedColor = mauNutHover;
        btn.colors = colors;
        btn.targetGraphic = btnImg;

        TextMeshProUGUI btnText = TaoText("Text", btnObj.transform, noiDung, fontSize, TextAlignmentOptions.Center);
        btnText.color = mauChuChinh;
        btnText.fontStyle = FontStyles.Bold;
        RectTransform textRect = btnText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 2);
        textRect.offsetMax = new Vector2(-10, -2);

        return btnObj;
    }

    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = layer;
        }
    }

    private static GameObject TimTheoTenTrongCon(Transform parent, string keyword)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains(keyword.ToLower()))
                return child.gameObject;

            var found = TimTheoTenTrongCon(child, keyword);
            if (found != null) return found;
        }
        return null;
    }
}
