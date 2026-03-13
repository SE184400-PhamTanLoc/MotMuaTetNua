using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections.Generic;

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
    private static Color mauNenPanel = new Color(0.02f, 0, 0, 1.0f); // Fully Opaque Very Dark Red
    private static Color mauChuChinh = new Color(1.0f, 1.0f, 0.9f, 1f); // Off-white
    private static Color mauVang = new Color(1.0f, 0.84f, 0.0f, 1f); // Gold
    private static Color mauNutBinhThuong = new Color(0.6f, 0.15f, 0.1f, 1f);
    private static Color mauNutHover = new Color(0.8f, 0.25f, 0.15f, 1f);
    
    private static Color mauDoTuoi = new Color(0.8f, 0.1f, 0.1f, 1f);
    private static Color mauNenGiay = new Color(0.95f, 0.9f, 0.8f, 1f); // Parchment

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

    private static GameObject _bargainPanel;
    private static TMP_InputField _priceInputField;
    private static Button _confirmBargainButton;
    private static Button _cancelBargainButton;

    private static GameObject _fruitFallingPanel;
    public static bool IsMinigameActive => _fruitFallingPanel != null && _fruitFallingPanel.activeSelf;
    
    private static RenderTexture[] _fruitTextures = new RenderTexture[4];
    private static GameObject[] _fruitPreviewModels = new GameObject[4];
    private static Camera _previewCamera;
    private static GameObject _minigameRunner;
    private static CutsceneManager _cutsceneManager;

    /// <summary>
    /// TỰ ĐỘNG CHẠY KHI NHẤN PLAY - Đã sửa để bắt sự kiện LoadScene
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Day_28_Scene" || scene.name == "VillageScene" || scene.name == "RoomVillage" )
        {
            TuDongSetup();
        }
    }

    private static void SetupNewManagers(GameObject runner)
    {
        if (NewYearsEveManager.Instance == null) runner.AddComponent<NewYearsEveManager>();
        
        // Luôn add mới để đảm bảo Instance trỏ đúng runner hiện tại sạch sẽ
        runner.AddComponent<Mung1Manager>();
        
        // Setup FamilyPhotoMinigame (Phải dán lại UI mỗi khi load scene vì Canvas bị destroy)
        FamilyPhotoMinigame photo = runner.AddComponent<FamilyPhotoMinigame>();
        Debug.Log("[AutoSetup] Created new FamilyPhotoMinigame instance.");

        // Setup UI for FamilyPhoto
        GameObject canvas = GameObject.Find("GameplayCanvas");
        if (canvas != null && photo != null)
        {
            GameObject panel = new GameObject("FamilyPhotoPanel");
            panel.transform.SetParent(canvas.transform, false);
            photo.panel = panel;
            
            GameObject flash = new GameObject("FlashOverlay");
            flash.transform.SetParent(panel.transform, false);
            Image fImg = flash.AddComponent<Image>();
            fImg.color = new Color(1, 1, 1, 0);
            RectTransform fRect = flash.GetComponent<RectTransform>();
            fRect.anchorMin = Vector2.zero;
            fRect.anchorMax = Vector2.one;
            fRect.offsetMin = Vector2.zero;
            fRect.offsetMax = Vector2.zero;
            photo.flashOverlay = fImg;

            GameObject text = new GameObject("CountdownText");
            text.transform.SetParent(panel.transform, false);
            TextMeshProUGUI tmp = text.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 100;
            tmp.alignment = TextAlignmentOptions.Center;
            photo.countdownText = tmp;
            
            panel.SetActive(false);
        }
    }

    private static void SetupCutsceneManager(GameObject runner)
    {
        if (CutsceneManager.Instance != null) return;

        GameObject canvas = GameObject.Find("GameplayCanvas");
        if (canvas == null) return;

        // Overlay cho video
        GameObject cutsceneOverlay = new GameObject("CutsceneOverlay");
        cutsceneOverlay.transform.SetParent(canvas.transform, false);
        CanvasGroup cg = cutsceneOverlay.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        
        RectTransform rect = cutsceneOverlay.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = cutsceneOverlay.AddComponent<Image>();
        bg.color = Color.black;

        // RawImage hiển thị video
        GameObject videoDisplayObj = new GameObject("VideoDisplay");
        videoDisplayObj.transform.SetParent(cutsceneOverlay.transform, false);
        RawImage ri = videoDisplayObj.AddComponent<RawImage>();
        RectTransform riRect = videoDisplayObj.GetComponent<RectTransform>();
        riRect.anchorMin = Vector2.zero;
        riRect.anchorMax = Vector2.one;
        riRect.offsetMin = Vector2.zero;
        riRect.offsetMax = Vector2.zero;

        // VideoPlayer
        VideoPlayer vp = runner.AddComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.renderMode = VideoRenderMode.APIOnly;
        
        // Cần gán texture cho RawImage khi video phát
        // Ở đây ta dùng Camera mode hoặc RenderTexture sẽ tiện hơn trong script, 
        // nhưng APIOnly + gán texture thủ công trong script manager cũng được.
        // Để đơn giản, update script CutsceneManager sau để cấp texture.
        
        _cutsceneManager = runner.AddComponent<CutsceneManager>();
        _cutsceneManager.cutsceneOverlay = cutsceneOverlay;
        _cutsceneManager.videoDisplay = ri;
        _cutsceneManager.videoPlayer = vp;
        _cutsceneManager.overlayCanvasGroup = cg;
    }

    private static void TuDongSetup()
    {
        // Lấy tên scene hiện tại
        string sceneName = SceneManager.GetActiveScene().name;

        // Nếu không phải scene Day28 hoặc các scene Room thì bỏ qua
        if (sceneName != "Day_28_Scene" && sceneName != "RoomVillage" && sceneName != "VillageScene")
        {
            Debug.Log("[AutoSetup] Skip setup vì không phải scene hợp lệ cho UI");
            return;
        }

        Debug.Log("<color=yellow>===================================</color>");
        Debug.Log("<color=yellow>[AutoSetup] 🏮 Bắt đầu tự động setup game (Scene: " + sceneName + ")...</color>");
        Debug.Log("<color=yellow>===================================</color>");

        // Dọn dẹp object cũ
        var oldRunner = GameObject.Find("_AutoSetup_Runner");
        if (oldRunner != null) Object.Destroy(oldRunner);
        
        var oldPreview = GameObject.Find("FruitPreviewStage");
        if (oldPreview != null) Object.Destroy(oldPreview);

        var oldCanvas = GameObject.Find("GameplayCanvas");
        if (oldCanvas != null) Object.Destroy(oldCanvas);

        var oldDiemTra = GameObject.Find("DiemTraMai_NhaMe");
        if (oldDiemTra != null) Object.Destroy(oldDiemTra);

        // Reset static references
        _canvas = null;
        _dialoguePanel = null;
        _tenNguoiNoiText = null;
        _noiDungText = null;
        _luaChonPanel = null;
        _luaChonButtonPrefab = null;
        _tiepTucButton = null;
        _tienText = null;
        _nhiemVuPanel = null;
        _nhiemVuText = null;
        _thongBaoPanel = null;
        _thongBaoText = null;
        _crosshairImage = null;
        _goiYTuongTacUI = null;
        _hoanThanhPanel = null;
        _hoanThanhText = null;
        _batDauPanel = null;
        _batDauText = null;
        _bargainPanel = null;
        _priceInputField = null;
        _confirmBargainButton = null;
        _cancelBargainButton = null;
        _fruitFallingPanel = null;
        _fruitTextures = new RenderTexture[4];
        _fruitPreviewModels = new GameObject[4];
        _previewCamera = null;
        _minigameRunner = null;

        GameObject setupObj = new GameObject("_AutoSetup_Runner");
        DontDestroyOnLoad(setupObj);

        // Core UI
        try { SetupGameManager(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Manager: " + e.Message); }
        try { TaoCanvas(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Canvas: " + e.Message); }
        try { TaoDialogueUI(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi DialogueUI: " + e.Message); }
        try { TaoHUD(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi HUD: " + e.Message); }
        try { TaoGoiYTuongTac(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Gợi ý: " + e.Message); }
        try { TaoCrosshair(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Crosshair: " + e.Message); }
        try { TaoLuaChonButtonPrefab(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Prefab: " + e.Message); }
        try { TaoBargainUI(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi BargainUI: " + e.Message); }
        try { SetupDialogueManager(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi DialogueManager: " + e.Message); }
        
        // Setup Player
        try { SetupPlayer(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Player: " + e.Message); }
        try { SetupGameHUD(); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi GameHUD: " + e.Message); }
        try { SetupCutsceneManager(setupObj); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi Cutscene: " + e.Message); }
        try { SetupNewManagers(setupObj); } catch (System.Exception e) { Debug.LogError("[AutoSetup] Lỗi NewManagers: " + e.Message); }

        // Setup Scene Specifics
        if (sceneName == "Day_28_Scene")
        {
            SetupMarketOnly();
            PlayDay28Bgm(setupObj);
        }
        else if (sceneName == "VillageScene")
        {
            if (GameManager.Instance != null) GameManager.Instance.isVillagePhase = true;
            SetupVillageOutsideOnly();
            PlayVillageBgm(setupObj);
        }
        else if (sceneName == "RoomVillage" || sceneName == "RoomScene")
        {
            SetupVillageOnly();
            PlayVillageBgm(setupObj);
        }

        if (UnityEngine.Object.FindFirstObjectByType<CursorStateController>() == null)
        {
            setupObj.AddComponent<CursorStateController>();
        }

        if (GameFlow.Instance != null && GameFlow.Instance.IsState(GameState.Intro))
        {
            GameFlow.Instance.ChangeState(GameState.State1_FreeOnlyChair);
        }

        Debug.Log("<color=green>===================================</color>");
        Debug.Log("<color=green>[AutoSetup] ✅ SETUP HOÀN TẤT!</color>");
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

    private static void PlayDay28Bgm(GameObject runner)
    {
        if (runner == null) return;

        AudioSource source = runner.GetComponent<AudioSource>();
        if (source == null) source = runner.AddComponent<AudioSource>();

        SimpleBgmPlayer bgmPlayer = runner.GetComponent<SimpleBgmPlayer>();
        if (bgmPlayer == null) bgmPlayer = runner.AddComponent<SimpleBgmPlayer>();

        AudioClip clip = Resources.Load<AudioClip>("music/Silent Beauty");
        if (clip != null)
        {
            bgmPlayer.Setup(clip, 0.7f);
            Debug.Log("[AutoSetup] ✅ Đã phát nhạc: Silent Beauty (Theme bài 28)");
        }
        else
        {
            Debug.LogWarning("[AutoSetup] ❌ Không tìm thấy nhạc tại Resources/music/Silent Beauty");
        }
    }

    private static void PlayVillageBgm(GameObject runner)
    {
        if (runner == null) return;

        AudioSource source = runner.GetComponent<AudioSource>();
        if (source == null) source = runner.AddComponent<AudioSource>();

        SimpleBgmPlayer bgmPlayer = runner.GetComponent<SimpleBgmPlayer>();
        if (bgmPlayer == null) bgmPlayer = runner.AddComponent<SimpleBgmPlayer>();

        AudioClip clip = Resources.Load<AudioClip>("music/Luminous");
        if (clip != null)
        {
            bgmPlayer.Setup(clip, 0.6f);
            Debug.Log("[AutoSetup] ✅ Đã phát nhạc: Luminous (Theme Village)");
        }
        else
        {
            Debug.LogWarning("[AutoSetup] ❌ Không tìm thấy nhạc tại Resources/music/Luminous");
        }
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
        dpRect.anchorMax = new Vector2(0.95f, 0.35f); // Giữ ở khoảng 1/3 màn hình để không che cảnh
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
        _tenNguoiNoiText.outlineWidth = 0.25f;
        _tenNguoiNoiText.outlineColor = Color.black;
        RectTransform tenRect = _tenNguoiNoiText.GetComponent<RectTransform>();
        tenRect.anchorMin = Vector2.zero;
        tenRect.anchorMax = Vector2.one;
        tenRect.offsetMin = new Vector2(10, 2);
        tenRect.offsetMax = new Vector2(-10, -2);

        // === Nội dung ===
        _noiDungText = TaoText("NoiDung", _dialoguePanel.transform, "", 32, TextAlignmentOptions.TopLeft); // Giảm font size xuống 32
        _noiDungText.fontStyle = FontStyles.Bold;
        _noiDungText.color = mauChuChinh;
        _noiDungText.enableWordWrapping = true;
        _noiDungText.richText = true;
        _noiDungText.outlineWidth = 0.25f;
        _noiDungText.outlineColor = Color.black;
        RectTransform ndRect = _noiDungText.GetComponent<RectTransform>();
        ndRect.anchorMin = new Vector2(0.04f, 0.55f); // Để dành hơn nửa dưới cho nút
        ndRect.anchorMax = new Vector2(0.96f, 0.94f);
        ndRect.offsetMin = Vector2.zero;
        ndRect.offsetMax = Vector2.zero;

        // === Lựa chọn Panel ===
        _luaChonPanel = new GameObject("LuaChonPanel");
        _luaChonPanel.transform.SetParent(_dialoguePanel.transform, false);
        RectTransform lcRect = _luaChonPanel.AddComponent<RectTransform>();
        lcRect.anchorMin = new Vector2(0.04f, 0.02f);
        lcRect.anchorMax = new Vector2(0.96f, 0.53f); // Mở rộng thêm nữa lên 0.53 để đủ chỗ cho 3 nút
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
        // === Tiền (Góc trên phải) ===
        GameObject tienBg = TaoPanel("TienBg", _canvas.transform);
        tienBg.GetComponent<Image>().color = mauDoTuoi;
        RectTransform tienBgRect = tienBg.GetComponent<RectTransform>();
        tienBgRect.anchorMin = new Vector2(0.75f, 0.94f);
        tienBgRect.anchorMax = new Vector2(0.99f, 0.99f);
        tienBgRect.offsetMin = Vector2.zero;
        tienBgRect.offsetMax = Vector2.zero;

        // Viền vàng cho tiền
        GameObject tienBorder = TaoPanel("TienBorder", tienBg.transform);
        tienBorder.GetComponent<Image>().color = mauVang;
        RectTransform tbRect = tienBorder.GetComponent<RectTransform>();
        tbRect.anchorMin = Vector2.zero;
        tbRect.anchorMax = new Vector2(1, 0.05f); // Chỉ ở cạnh dưới
        tbRect.offsetMin = Vector2.zero;
        tbRect.offsetMax = Vector2.zero;

        _tienText = TaoText("TienText", tienBg.transform, "Tiền: 1,000,000đ", 36, TextAlignmentOptions.Right);
        _tienText.fontStyle = FontStyles.Bold;
        _tienText.color = mauVang;
        _tienText.outlineWidth = 0.25f;
        _tienText.outlineColor = Color.black;
        RectTransform tienInnerRect = _tienText.GetComponent<RectTransform>();
        tienInnerRect.anchorMin = Vector2.zero;
        tienInnerRect.anchorMax = Vector2.one;
        tienInnerRect.offsetMin = new Vector2(0, 0);
        tienInnerRect.offsetMax = new Vector2(-15, 0);

        // === Nhiệm vụ (Góc trên trái) ===
        _nhiemVuPanel = TaoPanel("NhiemVuPanel", _canvas.transform);
        _nhiemVuPanel.GetComponent<Image>().color = mauNenGiay;
        RectTransform nvRect = _nhiemVuPanel.GetComponent<RectTransform>();
        nvRect.anchorMin = new Vector2(0.01f, 0.55f); // Chỉnh lại ngắn hơn một chút (từ 0.45 lên 0.55)
        nvRect.anchorMax = new Vector2(0.25f, 0.985f);
        nvRect.offsetMin = Vector2.zero;
        nvRect.offsetMax = Vector2.zero;

        // Thanh Header cho Nhiệm vụ
        GameObject nvHeader = TaoPanel("NhiemVuHeader", _nhiemVuPanel.transform);
        nvHeader.GetComponent<Image>().color = mauDoTuoi;
        RectTransform nvhRect = nvHeader.GetComponent<RectTransform>();
        nvhRect.anchorMin = new Vector2(0, 0.85f); // Header nhỏ lại một chút theo tỷ lệ mới
        nvhRect.anchorMax = new Vector2(1, 1);
        nvhRect.offsetMin = Vector2.zero;
        nvhRect.offsetMax = Vector2.zero;

        TextMeshProUGUI nvTitle = TaoText("NhiemVuTitle", nvHeader.transform, "NHIỆM VỤ", 18, TextAlignmentOptions.Center);
        nvTitle.fontStyle = FontStyles.Bold;
        nvTitle.color = mauVang;
        RectTransform nvTitleRect = nvTitle.GetComponent<RectTransform>();
        nvTitleRect.anchorMin = Vector2.zero;
        nvTitleRect.anchorMax = Vector2.one;
        nvTitleRect.offsetMin = Vector2.zero;
        nvTitleRect.offsetMax = Vector2.zero;

        _nhiemVuText = TaoText("NhiemVuText", _nhiemVuPanel.transform, "", 20, TextAlignmentOptions.TopLeft); // Giảm font size xuống 20
        _nhiemVuText.fontStyle = FontStyles.Bold;
        _nhiemVuText.color = Color.black;
        _nhiemVuText.outlineWidth = 0.2f;
        _nhiemVuText.outlineColor = new Color(0, 0, 0, 0.5f); // Subtle outline for dark text on light bg
        _nhiemVuText.lineSpacing = -5;
        RectTransform nvTextRect = _nhiemVuText.GetComponent<RectTransform>();
        nvTextRect.anchorMin = new Vector2(0.05f, 0.02f);
        nvTextRect.anchorMax = new Vector2(0.95f, 0.82f); // Hạ thấp trần nội dung xuống để tránh bị đè (từ 0.88 xuống 0.82)
        nvTextRect.offsetMin = Vector2.zero;
        nvTextRect.offsetMax = Vector2.zero;

        // Vẽ thêm 2 cái nút trang trí ở đầu Scroll
        GameObject decorLeft = TaoPanel("DecorL", _nhiemVuPanel.transform);
        decorLeft.GetComponent<Image>().color = mauDoTuoi;
        RectTransform dlRect = decorLeft.GetComponent<RectTransform>();
        dlRect.anchorMin = new Vector2(-0.02f, 0.78f);
        dlRect.anchorMax = new Vector2(0.02f, 1.02f);
        dlRect.offsetMin = Vector2.zero;
        dlRect.offsetMax = Vector2.zero;

        GameObject decorRight = TaoPanel("DecorR", _nhiemVuPanel.transform);
        decorRight.GetComponent<Image>().color = mauDoTuoi;
        RectTransform drRect = decorRight.GetComponent<RectTransform>();
        drRect.anchorMin = new Vector2(0.98f, 0.78f);
        drRect.anchorMax = new Vector2(1.02f, 1.02f);
        drRect.offsetMin = Vector2.zero;
        drRect.offsetMax = Vector2.zero;

        // === Thông báo ===
        _thongBaoPanel = TaoPanel("ThongBaoPanel", _canvas.transform);
        RectTransform tbpRect = _thongBaoPanel.GetComponent<RectTransform>();
        tbpRect.anchorMin = new Vector2(0.25f, 0.82f);
        tbpRect.anchorMax = new Vector2(0.75f, 0.89f);
        tbpRect.offsetMin = Vector2.zero;
        tbpRect.offsetMax = Vector2.zero;
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

        // === Bắt đầu (Cáo Thị Intro) ===
        _batDauPanel = TaoPanel("BatDauPanel", _canvas.transform);
        _batDauPanel.GetComponent<Image>().color = mauNenGiay;
        RectTransform bdRect = _batDauPanel.GetComponent<RectTransform>();
        bdRect.anchorMin = new Vector2(0.2f, 0.2f); // Mở rộng intro panel
        bdRect.anchorMax = new Vector2(0.8f, 0.8f);
        bdRect.offsetMin = Vector2.zero;
        bdRect.offsetMax = Vector2.zero;

        // Khung viền Cáo Thị
        GameObject borderOut = TaoPanel("BorderOut", _batDauPanel.transform);
        borderOut.GetComponent<Image>().color = mauDoTuoi;
        RectTransform boRect = borderOut.GetComponent<RectTransform>();
        boRect.anchorMin = Vector2.zero;
        boRect.anchorMax = Vector2.one;
        boRect.offsetMin = new Vector2(-10, -10);
        boRect.offsetMax = new Vector2(10, 10);
        borderOut.transform.SetAsFirstSibling();

        GameObject borderIn = TaoPanel("BorderIn", _batDauPanel.transform);
        borderIn.GetComponent<Image>().color = mauVang;
        RectTransform biRect = borderIn.GetComponent<RectTransform>();
        biRect.anchorMin = Vector2.zero;
        biRect.anchorMax = Vector2.one;
        biRect.offsetMin = new Vector2(5, 5);
        biRect.offsetMax = new Vector2(-5, -5);
        borderIn.transform.SetSiblingIndex(1);

        // Title Cáo Thị
        TextMeshProUGUI bdTitle = TaoText("BatDauTitle", _batDauPanel.transform, "CÁO THỊ NGÀY TẾT", 44, TextAlignmentOptions.Center);
        bdTitle.color = mauDoTuoi;
        bdTitle.fontStyle = FontStyles.Bold;
        bdTitle.outlineWidth = 0.25f;
        bdTitle.outlineColor = Color.black;
        RectTransform bdtRect = bdTitle.GetComponent<RectTransform>();
        bdtRect.anchorMin = new Vector2(0, 0.8f);
        bdtRect.anchorMax = new Vector2(1, 0.95f);
        bdtRect.offsetMin = Vector2.zero;
        bdtRect.offsetMax = Vector2.zero;

        _batDauText = TaoText("BatDauText", _batDauPanel.transform, "", 36, TextAlignmentOptions.Center);
        _batDauText.color = Color.black;
        _batDauText.fontStyle = FontStyles.Bold;
        _batDauText.outlineWidth = 0.25f;
        _batDauText.outlineColor = new Color(0, 0, 0, 0.4f);
        _batDauText.lineSpacing = 15; // Tăng khoảng cách dòng
        RectTransform bdTextRect = _batDauText.GetComponent<RectTransform>();
        bdTextRect.anchorMin = new Vector2(0.08f, 0.25f);
        bdTextRect.anchorMax = new Vector2(0.92f, 0.78f);
        bdTextRect.offsetMin = Vector2.zero;
        bdTextRect.offsetMax = Vector2.zero;

        // Nút Đóng (Seal style)
        GameObject dongBtnObj = TaoNut("DongIntroButton", _batDauPanel.transform, "BẮT ĐẦU NGAY (Space)");
        dongBtnObj.GetComponent<Image>().color = mauDoTuoi;
        TextMeshProUGUI btnText = dongBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null) btnText.color = mauVang;

        RectTransform dongBtnRect = dongBtnObj.GetComponent<RectTransform>();
        dongBtnRect.anchorMin = new Vector2(0.3f, 0.08f);
        dongBtnRect.anchorMax = new Vector2(0.7f, 0.18f);
        dongBtnRect.offsetMin = Vector2.zero;
        dongBtnRect.offsetMax = Vector2.zero;
        
        _batDauPanel.SetActive(false);
    }

    private static void TaoFruitFallingUI()
    {
        _fruitFallingPanel = TaoPanel("FruitFallingPanel", _canvas.transform);
        RectTransform rect = _fruitFallingPanel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        _fruitFallingPanel.GetComponent<Image>().color = new Color(0,0,0,0.7f);

        // Title
        TextMeshProUGUI title = TaoText("Title", _fruitFallingPanel.transform, "HỨNG QUẢ NGŨ QUẢ", 40, TextAlignmentOptions.Center);
        title.color = mauVang;
        RectTransform tRect = title.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0, 0.9f);
        tRect.anchorMax = new Vector2(1, 1f);
        
        // Instruction
        TextMeshProUGUI ins = TaoText("Instruction", _fruitFallingPanel.transform, "Click vào 4 quả khác nhau để nhặt.\n<color=red>NHẶT TRÙNG QUẢ = THUA!</color>", 24, TextAlignmentOptions.Center);
        ins.color = Color.white;
        RectTransform iRect = ins.GetComponent<RectTransform>();
        iRect.anchorMin = new Vector2(0, 0.82f);
        iRect.anchorMax = new Vector2(1, 0.9f);

        // Score / Collection Info
        GameObject scoreObj = TaoPanel("CollectionInfo", _fruitFallingPanel.transform);
        scoreObj.GetComponent<Image>().color = new Color(1,1,1,0.1f);
        RectTransform sRect = scoreObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.3f, 0.05f);
        sRect.anchorMax = new Vector2(0.7f, 0.15f);
        
        _fruitFallingPanel.SetActive(false);
    }

    public static void InitializeFruitPreviewStage()
    {
        // Tạo stage ở xa để không bị cam chính thấy
        GameObject stage = new GameObject("FruitPreviewStage");
        stage.transform.position = new Vector3(999, 999, 999);
        DontDestroyOnLoad(stage);

        // Camera
        GameObject camObj = new GameObject("PreviewCamera");
        camObj.transform.SetParent(stage.transform);
        camObj.transform.localPosition = new Vector3(0, 0, -2);
        _previewCamera = camObj.AddComponent<Camera>();
        _previewCamera.clearFlags = CameraClearFlags.SolidColor;
        _previewCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent
        _previewCamera.orthographic = true;
        _previewCamera.orthographicSize = 0.5f;
        _previewCamera.enabled = false; // QUAN TRỌNG: Tắt để không đè màn hình chính

        // Light
        GameObject lightObj = new GameObject("PreviewLight");
        lightObj.transform.SetParent(stage.transform);
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.localRotation = Quaternion.Euler(30, -30, 0);

        // Chuẩn bị các quả
        string[] paths = { "fruits/mang_cau", "fruits/dua", "fruits/du_du", "fruits/xoai" };
        for (int i = 0; i < 4; i++)
        {
            _fruitTextures[i] = new RenderTexture(256, 256, 16);
            _fruitTextures[i].Create();

            GameObject prefab = Resources.Load<GameObject>(paths[i]);
            if (prefab != null)
            {
                _fruitPreviewModels[i] = Instantiate(prefab, stage.transform);
                _fruitPreviewModels[i].transform.localPosition = Vector3.right * (i * 2);
                _fruitPreviewModels[i].SetActive(false); // Ẩn mặc định
            }
            else
            {
                Debug.LogWarning("[AutoSetup] Không tìm thấy prefab tại Resources/" + paths[i]);
            }
        }
    }

    public static void StartFruitMinigame()
    {
        if (_fruitFallingPanel != null)
        {
            _fruitFallingPanel.SetActive(true);
            if (_minigameRunner == null)
            {
                _minigameRunner = new GameObject("FruitMinigameRunner");
                _minigameRunner.transform.SetParent(_fruitFallingPanel.transform);
                var runner = _minigameRunner.AddComponent<FruitFallingRunner>();
                runner.Setup(_fruitFallingPanel, _fruitTextures, _fruitPreviewModels, _previewCamera);
            }
            else
            {
                _minigameRunner.GetComponent<FruitFallingRunner>().RestartGame();
            }
        }
    }

    public static void StopFruitMinigame()
    {
        if (_fruitFallingPanel != null)
            _fruitFallingPanel.SetActive(false);
        
        foreach (var m in _fruitPreviewModels) if (m != null) m.SetActive(false);
    }

    // --- MINIGAME COMPONENTS ---

    public class FruitFallingRunner : MonoBehaviour
    {
        private GameObject _panel;
        private RenderTexture[] _textures;
        private GameObject[] _models;
        private Camera _cam;
        
        private List<string> _collected = new List<string>();
        private float _spawnTimer = 0f;
        private float _spawnInterval = 1.2f;
        private bool _isGameOver = false;

        private GameObject _msgBox;
        private TextMeshProUGUI _collectionText;

        public void Setup(GameObject panel, RenderTexture[] textures, GameObject[] models, Camera cam)
        {
            if (panel == null) {
                Debug.LogError("[FruitFallingRunner] Setup failed: panel is null!");
                return;
            }
            _panel = panel; _textures = textures; _models = models; _cam = cam;
            
            // Text hiển thị kết quả - Chỉ tìm hoặc tạo nếu thực sự cần
            if (_collectionText == null)
            {
                Transform infoT = _panel.transform.Find("CollectionInfo");
                if (infoT != null)
                {
                    _collectionText = infoT.GetComponent<TextMeshProUGUI>();
                    if (_collectionText == null) _collectionText = infoT.gameObject.AddComponent<TextMeshProUGUI>();
                }
                
                if (_collectionText == null)
                {
                    _collectionText = TaoText("CollectionInfo", _panel.transform, "", 24, TextAlignmentOptions.Center);
                    RectTransform rt = _collectionText.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.1f, 0.05f);
                    rt.anchorMax = new Vector2(0.9f, 0.15f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }
            }
            
            if (_collectionText != null)
            {
                _collectionText.alignment = TextAlignmentOptions.Center;
                _collectionText.fontSize = 24;
                _collectionText.color = Color.yellow;
                _collectionText.raycastTarget = false;
            }
            else
            {
                Debug.LogError("[FruitFallingRunner] Failed to create or find _collectionText!");
            }
            
            RestartGame();
        }

        public void RestartGame()
        {
            _collected.Clear();
            _isGameOver = false;
            _spawnTimer = 0;
            _spawnInterval = 1.2f;
            UpdateTimeText();

            // Clear old fruits
            foreach (Transform child in transform) {
                if (child.name.StartsWith("FallingFruit")) Destroy(child.gameObject);
            }
            
            if (_msgBox != null) _msgBox.SetActive(false);
            
            // Đảm bảo models render sẵn
            if (_cam != null && _models != null && _textures != null) {
                for (int i = 0; i < 4; i++) {
                    if (i < _models.Length && _models[i] != null && i < _textures.Length && _textures[i] != null) {
                        _models[i].SetActive(true);
                        _cam.enabled = true;
                        _cam.targetTexture = _textures[i];
                        _cam.transform.position = _models[i].transform.position + Vector3.forward * -2;
                        _cam.Render();
                        _cam.targetTexture = null;
                        _cam.enabled = false;
                        _models[i].SetActive(false);
                    }
                }
            }
        }

        void Update()
        {
            if (_isGameOver) return;

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _spawnInterval)
            {
                _spawnTimer = 0;
                SpawnFruit();
                _spawnInterval = Mathf.Max(0.5f, _spawnInterval * 0.98f); // Nhanh dần
            }
        }

        void SpawnFruit()
        {
            int index = Random.Range(0, 4);
            string[] names = { "Mãng cầu", "Dừa", "Đu đủ", "Xoài" };
            
            GameObject fruit = new GameObject("FallingFruit_" + names[index], typeof(RectTransform), typeof(RawImage), typeof(Button));
            fruit.transform.SetParent(this.transform, false);
            
            RectTransform rect = fruit.GetComponent<RectTransform>();
            float startX = Random.Range(100f, Screen.width - 100f);
            rect.position = new Vector3(startX, Screen.height + 50f, 0);
            rect.sizeDelta = new Vector2(200, 200);

            RawImage img = fruit.GetComponent<RawImage>();
            img.texture = _textures[index];

            float speed = Random.Range(200f, 400f);
            fruit.AddComponent<FallingObject>().speed = speed;

            string fruitName = names[index];
            fruit.GetComponent<Button>().onClick.AddListener(() => OnItemClicked(fruitName, fruit));
        }

        void OnItemClicked(string name, GameObject obj)
        {
            if (_isGameOver) return;

            if (_collected.Contains(name))
            {
                GameOver(false);
                Destroy(obj);
            }
            else
            {
                _collected.Add(name);
                UpdateTimeText();
                Destroy(obj);

                if (_collected.Count >= 4)
                {
                    GameOver(true);
                }
            }
        }

        void UpdateTimeText()
        {
            if (_collectionText != null)
                _collectionText.text = "Đã nhặt: " + string.Join(", ", _collected) + " (" + _collected.Count + "/4)";
        }

        void GameOver(bool win)
        {
            _isGameOver = true;
            // Clear remaining fruits
            foreach (Transform child in transform) {
                if (child.name.StartsWith("FallingFruit")) Destroy(child.gameObject);
            }

            if (win) {
                ShowMessage("CHIẾN THẮNG!", "Đã mua thành công mâm ngũ quả!", "NHẬN QUẢ", () => {
                    GameManager.Instance.MuaNguyenLieu("Mãng cầu", 0, false);
                    GameManager.Instance.MuaNguyenLieu("Dừa", 0, false);
                    GameManager.Instance.MuaNguyenLieu("Đu đủ", 0, false);
                    GameManager.Instance.MuaNguyenLieu("Xoài", 0, false);
                    GameManager.Instance.TruTien(100);
                    GameManager.Instance.OnThongBao?.Invoke("Đã mua thành công mâm ngũ quả!"); // Hiện 1 dòng cuối cùng
                    _panel.SetActive(false);
                    StopFruitMinigame();
                });
            } else {
                ShowMessage("THUA RỒI!", "Nhặt trùng quả rồi cậu ơi! Chị Lan không chịu đâu.", "THỬ LẠI", () => {
                    RestartGame();
                }, "THÔI", () => {
                    _panel.SetActive(false);
                    StopFruitMinigame();
                });
            }
        }

        void ShowMessage(string title, string content, string btnText, UnityEngine.Events.UnityAction onConfirm, string btn2Text = "", UnityEngine.Events.UnityAction onConfirm2 = null)
        {
            if (_msgBox == null)
            {
                _msgBox = TaoPanel("MessageBox", _panel.transform);
                _msgBox.GetComponent<Image>().color = new Color(0,0,0,0.95f);
                RectTransform mRect = _msgBox.GetComponent<RectTransform>();
                mRect.anchorMin = new Vector2(0.2f, 0.25f);
                mRect.anchorMax = new Vector2(0.8f, 0.75f);
                mRect.offsetMin = Vector2.zero;
                mRect.offsetMax = Vector2.zero;
            }
            
            _msgBox.SetActive(true);
            foreach (Transform child in _msgBox.transform) Destroy(child.gameObject);

            var tObj = TaoText("Title", _msgBox.transform, title, 36, TextAlignmentOptions.Center);
            tObj.color = Color.yellow;
            RectTransform tRect = tObj.GetComponent<RectTransform>();
            tRect.sizeDelta = new Vector2(600, 80);
            tRect.anchoredPosition = new Vector2(0, 100);

            var cObj = TaoText("Content", _msgBox.transform, content, 24, TextAlignmentOptions.Center);
            RectTransform cRect = cObj.GetComponent<RectTransform>();
            cRect.sizeDelta = new Vector2(600, 150);
            cRect.anchoredPosition = new Vector2(0, 0);

            if (string.IsNullOrEmpty(btn2Text))
            {
                var btn = TaoNut("ConfirmBtn", _msgBox.transform, btnText, 24);
                btn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -120);
                btn.GetComponent<Button>().onClick.AddListener(onConfirm);
            }
            else
            {
                var btn1 = TaoNut("ConfirmBtn1", _msgBox.transform, btnText, 24);
                btn1.GetComponent<RectTransform>().anchoredPosition = new Vector2(-120, -120);
                btn1.GetComponent<Button>().onClick.AddListener(onConfirm);

                var btn2 = TaoNut("ConfirmBtn2", _msgBox.transform, btn2Text, 24);
                btn2.GetComponent<RectTransform>().anchoredPosition = new Vector2(120, -120);
                btn2.GetComponent<Button>().onClick.AddListener(onConfirm2);
            }
        }
    }

    public class FallingObject : MonoBehaviour
    {
        public float speed = 300f;
        void Update()
        {
            transform.Translate(Vector3.down * speed * Time.deltaTime);
            if (transform.position.y < -150) Destroy(gameObject);
        }
    }

    // Helper class để xoay quả
    public class FruitSpinner : MonoBehaviour
    {
        void Update()
        {
            transform.Rotate(Vector3.up, 30 * Time.deltaTime);
        }
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
            "Nhấn <color=yellow><b>E</b></color> để tương tác", 30, TextAlignmentOptions.Center);
        gyText.color = Color.white;
        gyText.fontStyle = FontStyles.Bold;
        
        // Thêm Outline đen cực mạnh để chữ "nổi" hẳn lên
        gyText.outlineWidth = 0.35f;
        gyText.outlineColor = Color.black;
        
        // Thêm đổ bóng (Shadow)
        gyText.fontMaterial.EnableKeyword("UNDERLAY_ON");
        gyText.fontMaterial.SetColor("_UnderlayColor", new Color(0, 0, 0, 0.5f));
        gyText.fontMaterial.SetVector("_UnderlayOffset", new Vector4(2, -2, 0, 0));
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

    private static void TaoBargainUI()
    {
        _bargainPanel = TaoPanel("BargainPanel", _canvas.transform);
        _bargainPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.85f);
        RectTransform bpRect = _bargainPanel.GetComponent<RectTransform>();
        bpRect.anchorMin = new Vector2(0.35f, 0.4f);
        bpRect.anchorMax = new Vector2(0.65f, 0.65f);
        bpRect.offsetMin = Vector2.zero;
        bpRect.offsetMax = Vector2.zero;

        TaoText("BargainTitle", _bargainPanel.transform, "NHẬP GIÁ (Ví dụ: 80000 = 80,000đ)", 24, TextAlignmentOptions.Center).color = mauVang;

        // Input Field
        GameObject inputObj = new GameObject("PriceInputField");
        inputObj.transform.SetParent(_bargainPanel.transform, false);
        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.1f, 0.45f);
        inputRect.anchorMax = new Vector2(0.9f, 0.65f);
        inputRect.offsetMin = Vector2.zero;
        inputRect.offsetMax = Vector2.zero;
        inputObj.AddComponent<Image>().color = Color.white;

        _priceInputField = inputObj.AddComponent<TMP_InputField>();
        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform areaRect = textArea.AddComponent<RectTransform>();
        areaRect.anchorMin = Vector2.zero;
        areaRect.anchorMax = Vector2.one;
        areaRect.offsetMin = new Vector2(10, 5);
        areaRect.offsetMax = new Vector2(-10, -5);
        textArea.AddComponent<RectMask2D>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.color = Color.black;
        text.fontSize = 32;
        text.alignment = TextAlignmentOptions.Center;
        RectTransform tRect = textObj.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;

        _priceInputField.textComponent = text;
        _priceInputField.contentType = TMP_InputField.ContentType.IntegerNumber;

        // Buttons
        GameObject confirmObj = TaoNut("ConfirmBargain", _bargainPanel.transform, "XÁC NHẬN", 24);
        _confirmBargainButton = confirmObj.GetComponent<Button>();
        RectTransform confRect = confirmObj.GetComponent<RectTransform>();
        confRect.anchorMin = new Vector2(0.1f, 0.15f);
        confRect.anchorMax = new Vector2(0.45f, 0.35f);
        confRect.offsetMin = Vector2.zero;
        confRect.offsetMax = Vector2.zero;

        GameObject cancelObj = TaoNut("CancelBargain", _bargainPanel.transform, "HỦY", 24);
        _cancelBargainButton = cancelObj.GetComponent<Button>();
        RectTransform cancRect = cancelObj.GetComponent<RectTransform>();
        cancRect.anchorMin = new Vector2(0.55f, 0.15f);
        cancRect.anchorMax = new Vector2(0.9f, 0.35f);
        cancRect.offsetMin = Vector2.zero;
        cancRect.offsetMax = Vector2.zero;

        _bargainPanel.SetActive(false);
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

        dm.bargainPanel = _bargainPanel;
        dm.priceInputField = _priceInputField;
        dm.confirmBargainButton = _confirmBargainButton;
        dm.cancelBargainButton = _cancelBargainButton;

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
                box.isTrigger = true; // NPC nên là Trigger để không gây xung đột vật lý
                Debug.Log($"[AutoSetup] ✅ Thêm BoxCollider (Trigger) cho NPC: center={box.center}, size={box.size}");
            }
            else
            {
                CapsuleCollider capsule = npcObj.AddComponent<CapsuleCollider>();
                capsule.center = new Vector3(0, 1f, 0);
                capsule.radius = 1f;
                capsule.height = 2f;
                capsule.isTrigger = true; // Cường hóa: Luôn là Trigger
                Debug.Log("[AutoSetup] ✅ Thêm CapsuleCollider (Trigger) mặc định cho NPC");
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

        // Đặt bán kính CharacterController hợp lý để tránh giật camera và kẹt physics
        CharacterController charCtrl = playerObj.GetComponent<CharacterController>();
        if (charCtrl != null)
        {
            charCtrl.radius = 0.3f; // Tăng lên 0.3 để ổn định hơn (trước là 0.08 quá nhỏ gây jitter)
            charCtrl.stepOffset = 0.4f;
            charCtrl.slopeLimit = 60f;
            Debug.Log($"[AutoSetup] ✅ Tối ưu CharacterController: radius={charCtrl.radius}");
        }

        // Gắn PlayerInteraction
        PlayerInteraction pi = playerObj.GetComponent<PlayerInteraction>();
        if (pi == null)
            pi = playerObj.AddComponent<PlayerInteraction>();

        pi.khoangCachTuongTac = 3f; // Tầm nhìn ~3m theo yêu cầu
        pi.npcLayer = 1 << 6;
        pi.goiYTuongTacUI = _goiYTuongTacUI;

        Debug.Log($"[AutoSetup] ✅ Setup Player hoàn tất (Khoảng cách tương tác: {pi.khoangCachTuongTac}, LayerMask: {pi.npcLayer.value})");
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
        box.size = new Vector3(4, 3, 4);
        box.center = new Vector3(0, 1.5f, 0);

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
        // 1. Tìm NPC để làm mốc tọa độ
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

        // 2. Tìm tất cả vật thể có tên liên quan đến Mai và chọn cái gần NPC nhất
        GameObject maiObj = null;
        float minDistance = float.MaxValue;
        string[] keywords = { "diem_lay_mai", "chau_mai", "cay_mai", "nha_cay_mai" };
        
        // Chỉ tìm nếu có NPC để so sánh
        if (npcObj != null)
        {
            // Quét tất cả các object trong scene (hơi nặng nhưng chính xác cho scene dynamic)
            GameObject[] allObjs = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in allObjs)
            {
                foreach (var key in keywords)
                {
                    if (obj.name.ToLower().Contains(key.ToLower()))
                    {
                        float dist = Vector3.Distance(obj.transform.position, npcObj.transform.position);
                        if (dist < minDistance && dist > 0.1f) // tránh trùng chính nó
                        {
                            minDistance = dist;
                            maiObj = obj;
                        }
                        break;
                    }
                }
            }
        }

        // 3. Nếu không tìm thấy bằng quét dạo, fallback tìm theo tên cụ thể
        if (maiObj == null)
        {
            foreach (var ten in keywords)
            {
                maiObj = GameObject.Find(ten);
                if (maiObj != null) break;
            }
        }

        // 4. Tuyệt chiêu cuối: Nếu vẫn không có, tạo mới gần NPC
        if (maiObj == null)
        {
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
            box.size = new Vector3(1.5f, 2, 1.5f);
            box.center = new Vector3(0, 1f, 0);
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
    // XẠP GẠO (Bà Sáu - bán gạo nếp, đậu xanh, lá chuối)
    // =========================================
    private static void SetupXapGao()
    {
        GameObject xapObj = null;
        string[] tenThuongCo = { "xap_gao", "sap_gao", "ba_sau", "gian_hang_gao", "GaoShop", "RiceShop" };
        foreach (var ten in tenThuongCo)
        {
            xapObj = GameObject.Find(ten);
            if (xapObj != null) break;
        }

        // Tìm bằng cách quét tất cả root objects
        if (xapObj == null)
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var rootObj in scene.GetRootGameObjects())
            {
                if (rootObj.name.ToLower().Contains("gao") || rootObj.name.ToLower().Contains("rice"))
                {
                    xapObj = rootObj;
                    break;
                }
                var found = TimTheoTenTrongCon(rootObj.transform, "gao");
                if (found != null) { xapObj = found; break; }
            }
        }

        if (xapObj == null)
        {
            Debug.Log("[AutoSetup] \u26a0\ufe0f Không tìm thấy đối tượng Xạp Gạo, bỏ qua.");
            return;
        }

        XapGao xg = xapObj.GetComponent<XapGao>();
        if (xg == null)
            xg = xapObj.AddComponent<XapGao>();

        xg.tenNPC = "Bà Sáu";
        xg.quayVePhiaPlayer = false;

        Rigidbody rb = xapObj.GetComponent<Rigidbody>();
        if (rb == null) rb = xapObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // Đảm bảo có BoxCollider và là Trigger
        BoxCollider bc = xapObj.GetComponent<BoxCollider>();
        if (bc == null)
        {
            // Tắt các collider khác nếu có
            Collider otherCol = xapObj.GetComponent<Collider>();
            if (otherCol != null) otherCol.enabled = false;
            bc = xapObj.AddComponent<BoxCollider>();
        }
        bc.isTrigger = true;
        bc.size = new Vector3(1.1f, 2f, 1.1f); 
        bc.center = new Vector3(0, 1f, 0);

        SetLayerRecursive(xapObj, 6);
        Debug.Log($"[AutoSetup] \u2705 Setup Xạp Gạo: {xapObj.name}");
    }

    // =========================================
    // XẠP THỊT (Chú Tư - bán thịt heo)
    // =========================================
    private static void SetupXapThit()
    {
        GameObject xapObj = null;
        string[] tenThuongCo = { "xap_thit", "sap_thit", "chu_tu", "gian_hang_thit", "MeatShop", "PorkShop" };
        foreach (var ten in tenThuongCo)
        {
            xapObj = GameObject.Find(ten);
            if (xapObj != null) break;
        }

        // Tìm bằng cách quét tất cả root objects
        if (xapObj == null)
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var rootObj in scene.GetRootGameObjects())
            {
                if (rootObj.name.ToLower().Contains("thit") || rootObj.name.ToLower().Contains("meat") || rootObj.name.ToLower().Contains("pork"))
                {
                    xapObj = rootObj;
                    break;
                }
                var found = TimTheoTenTrongCon(rootObj.transform, "thit");
                if (found != null) { xapObj = found; break; }
            }
        }

        if (xapObj == null)
        {
            Debug.Log("[AutoSetup] \u26a0\ufe0f Không tìm thấy đối tượng Xạp Thịt, bỏ qua.");
            return;
        }

        XapThit xt = xapObj.GetComponent<XapThit>();
        if (xt == null)
            xt = xapObj.AddComponent<XapThit>();

        xt.tenNPC = "Chú Tư";
        xt.quayVePhiaPlayer = false;

        Rigidbody rb = xapObj.GetComponent<Rigidbody>();
        if (rb == null) rb = xapObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // Đảm bảo có BoxCollider và là Trigger
        BoxCollider bc = xapObj.GetComponent<BoxCollider>();
        if (bc == null)
        {
            // Tắt các collider khác nếu có
            Collider otherCol = xapObj.GetComponent<Collider>();
            if (otherCol != null) otherCol.enabled = false;
            bc = xapObj.AddComponent<BoxCollider>();
        }
        bc.isTrigger = true;
        bc.size = new Vector3(1.1f, 2f, 1.1f);
        bc.center = new Vector3(0, 1f, 0);

        SetLayerRecursive(xapObj, 6);
        Debug.Log($"[AutoSetup] \u2705 Setup Xạp Thịt: {xapObj.name}");
    }

    // =========================================
    // XẠP TRÁI CÂY (Chị Lan - mâm Ngũ Quả)
    // =========================================
    private static void SetupXapTraiCay()
    {
        GameObject xapObj = null;
        string[] tenThuongCo = { "xap_rau_cu", "xap_trai_cay", "sap_trai_cay", "chi_lan", "gian_hang_trai_cay", "FruitShop", "xap_traicay" };
        foreach (var ten in tenThuongCo)
        {
            xapObj = GameObject.Find(ten);
            if (xapObj != null) break;
        }

        // Tìm bằng cách quét tất cả root objects
        if (xapObj == null)
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var rootObj in scene.GetRootGameObjects())
            {
                if (rootObj.name.ToLower().Contains("trai_cay") || rootObj.name.ToLower().Contains("traicay") || rootObj.name.ToLower().Contains("fruit"))
                {
                    xapObj = rootObj;
                    break;
                }
                var found = TimTheoTenTrongCon(rootObj.transform, "trai_cay");
                if (found == null) found = TimTheoTenTrongCon(rootObj.transform, "traicay");
                if (found == null) found = TimTheoTenTrongCon(rootObj.transform, "fruit");
                if (found != null) { xapObj = found; break; }
            }
        }

        if (xapObj == null)
        {
            Debug.Log("[AutoSetup] \u26a0\ufe0f Không tìm thấy đối tượng Xạp Trái Cây, bỏ qua.");
            return;
        }

        XapTraiCay xtc = xapObj.GetComponent<XapTraiCay>();
        if (xtc == null)
            xtc = xapObj.AddComponent<XapTraiCay>();

        xtc.tenNPC = "Chị Lan";
        xtc.quayVePhiaPlayer = false;

        Rigidbody rb = xapObj.GetComponent<Rigidbody>();
        if (rb == null) rb = xapObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        // Đảm bảo có BoxCollider và là Trigger
        BoxCollider bc = xapObj.GetComponent<BoxCollider>();
        if (bc == null)
        {
            // Tắt các collider khác nếu có
            Collider otherCol = xapObj.GetComponent<Collider>();
            if (otherCol != null) otherCol.enabled = false;
            bc = xapObj.AddComponent<BoxCollider>();
        }
        bc.isTrigger = true;
        bc.size = new Vector3(1.1f, 2f, 1.1f);
        bc.center = new Vector3(0, 1f, 0);

        SetLayerRecursive(xapObj, 6);
        Debug.Log($"[AutoSetup] \u2705 Setup Xạp Trái Cây: {xapObj.name}");
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

    private static void SetupMarketOnly()
    {
        try { SetupGameManager(); } catch {}
        try { TaoCanvas(); } catch {}
        try { TaoDialogueUI(); } catch {}
        try { TaoHUD(); } catch {}
        try { TaoGoiYTuongTac(); } catch {}
        try { TaoCrosshair(); } catch {}
        try { TaoLuaChonButtonPrefab(); } catch {}
        try { TaoBargainUI(); } catch {}
        try { SetupDialogueManager(); } catch {}
        try { SetupCoGaiBanMai(); } catch {}
        try { SetupPlayer(); } catch {}
        try { SetupGameHUD(); } catch {}
        try { SetupDiemLayMai(); } catch {}
        try { SetupBauCua(); } catch {}
        try { SetupGiengNguyenUoc(); } catch {}
        try { SetupXapGao(); } catch {}
        try { SetupXapThit(); } catch {}
        try { SetupXapTraiCay(); } catch {}
        try { InitializeFruitPreviewStage(); } catch {}
        try { TaoFruitFallingUI(); } catch {}
    }

    private static void SetupVillageOutsideOnly()
    {
        try { SetupGameManager(); } catch {}
        try { TaoCanvas(); } catch {}
        try { TaoDialogueUI(); } catch {}
        try { TaoHUD(); } catch {}
        try { TaoGoiYTuongTac(); } catch {}
        try { TaoCrosshair(); } catch {}
        try { SetupDialogueManager(); } catch {}
        try { SetupPlayer(); } catch {}
        try { SetupGameHUD(); } catch {}

        // Managers đặc thù cho cả trong và ngoài (như quét sân)
        try { SetupRoomVillageManagers(); } catch {}

        // Minigames cụ thể cho khu vực làng (ngoài sân)
        try { BanhTetMinigame.Create(_canvas, _goiYTuongTacUI); } catch {}
        try { CanhNoiBanhMinigame.Create(_canvas, _goiYTuongTacUI); } catch {}
        
        // Tự động setup các tương tác làng (Chổi, Cửa)
        try { SetupVillageInteractions(); } catch {}
    }

    private static void SetupVillageOnly()
    {
        try { SetupGameManager(); } catch {}

        try { TaoCanvas(); } catch {}
        try { TaoHUD(); } catch {}
        try { TaoDialogueUI(); } catch {}
        try { TaoGoiYTuongTac(); } catch {}
        try { TaoCrosshair(); } catch {}
        try { SetupPlayer(); } catch {}
        try { SetupGameHUD(); } catch {}
        try { SetupDiemTraMai(); } catch {}
        
        // Setup các Manager đặc thù cho RoomVillage
        try { SetupRoomVillageManagers(); } catch {}
        // Tự động setup các tương tác (Mẹ, Cửa, v.v.)
        try { SetupVillageInteractions(); } catch {}
    }

    private static void SetupRoomVillageManagers()
    {
        // 1. RoomVillageManager
        if (Object.FindFirstObjectByType<RoomVillageManager>() == null)
        {
            GameObject rmObj = new GameObject("RoomVillageManager");
            rmObj.AddComponent<RoomVillageManager>();
        }

        // 2. AltarCleaningMinigame
        if (Object.FindFirstObjectByType<AltarCleaningMinigame>() == null)
        {
            GameObject minigameObj = new GameObject("AltarCleaningMinigame");
            minigameObj.AddComponent<AltarCleaningMinigame>();
        }

        // 3. YardSweepingMinigame
        if (Object.FindFirstObjectByType<YardSweepingMinigame>() == null)
        {
            GameObject yardMinigameObj = new GameObject("YardSweepingMinigame");
            yardMinigameObj.AddComponent<YardSweepingMinigame>();
        }
    }

    private static void SetupVillageInteractions()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[AutoSetup] 🔍 Bắt đầu quét tương tác cho scene: {sceneName}");
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"[AutoSetup] Tìm thấy {allObjects.Length} objects tổng cộng.");

        foreach (var obj in allObjects)
        {
            if (obj == null) continue;
            string lowerName = obj.name.ToLower();

            // 1. Nguoi_Me
            if (lowerName.Contains("nguoi_me") || lowerName.Contains("mother") || lowerName.Contains("me_npc"))
            {
                // ĐẶT LẠI ROTATION NGAY LẬP TỨC trước khi thêm gì – model GLB thường có baked -90X từ Blender
                ResetNPCRotation(obj);
                FreezeNPCPhysics(obj);
                if (obj.GetComponent<NguoiMeInteract>() == null) obj.AddComponent<NguoiMeInteract>();
                EnsureNPCCollider(obj);
                SetLayerRecursive(obj, 6);
                Debug.Log($"[AutoSetup] ✅ Đã reset và đóng băng rotation cho Mẹ: {obj.name}");
            }
            // 1b. Ong_Noi (Kiểm tra trước vì "grandfather" chứa "father")
            else if (lowerName.Contains("ong_noi") || lowerName.Contains("grandfather") || lowerName.Contains("ongnoi"))
            {
                ResetNPCRotation(obj);
                FreezeNPCPhysics(obj);
                if (obj.GetComponent<OngNoiInteract>() == null)
                {
                    obj.AddComponent<OngNoiInteract>();
                    Debug.Log($"[AutoSetup] 👨‍🦳 Gắn OngNoiInteract cho: {obj.name}");
                }
                EnsureNPCCollider(obj);
                SetLayerRecursive(obj, 6);
            }
            // 1c. Nguoi_Bo
            else if (lowerName.Contains("nguoi_bo") || lowerName.Contains("father") || lowerName.Contains("bo_npc") || lowerName.Contains("nguoibo"))
            {
                ResetNPCRotation(obj);
                FreezeNPCPhysics(obj);
                if (obj.GetComponent<NguoiBoInteract>() == null)
                {
                    obj.AddComponent<NguoiBoInteract>();
                    Debug.Log($"[AutoSetup] 👨 Gắn NguoiBoInteract cho: {obj.name}");
                }
                EnsureNPCCollider(obj);
                SetLayerRecursive(obj, 6);
            }
            // 2. CaiKhan
            else if (lowerName.Contains("caikhan"))
            {
                if (obj.GetComponent<ClothInteract>() == null) obj.AddComponent<ClothInteract>();
                EnsureNPCCollider(obj, true); // Use Box for items
                SetLayerRecursive(obj, 6);
            }
            // 3. BanTho
            else if (lowerName.Contains("bantho"))
            {
                if (obj.GetComponent<AltarInteract>() == null) obj.AddComponent<AltarInteract>();
                EnsureNPCCollider(obj, true);
                SetLayerRecursive(obj, 6);
            }
            // 4. Thoát (RaKhoiNha / Exit) - Thường trong RoomVillage để ra ngoài
            else if (lowerName.Contains("rakhoinha") || lowerName.Contains("door_exit") || lowerName.Contains("exit") || lowerName.Contains("door_to_village") || (sceneName == "RoomVillage" && (lowerName.Contains("door") || lowerName.Contains("gate") || lowerName.Contains("cong"))))
            {
                if (obj.GetComponent<DoorExit>() == null)
                {
                    obj.AddComponent<DoorExit>();
                    Debug.Log($"[AutoSetup] 🚪 PHÁT HIỆN CỬA RA: {obj.name} -> Gắn DoorExit");
                }
                var de = obj.GetComponent<DoorExit>();
                de.sceneName = "VillageScene";
                de.hanhDongTuongTac = "ra ngoài";
                
                EnsureNPCCollider(obj, true);
                SetLayerRecursive(obj, 6);
            }
            // 5. Vào (VaoTrongNha / Entrance) - Thường trong VillageScene để vào nhà
            else if (lowerName.Contains("vaotrongnha") || lowerName.Contains("vaonha") || lowerName.Contains("entrance") || lowerName.Contains("door_to_room") || (sceneName == "VillageScene" && (lowerName.Contains("door") || lowerName.Contains("gate"))))
            {
                if (obj.GetComponent<DoorEnter>() == null)
                {
                    obj.AddComponent<DoorEnter>();
                    Debug.Log($"[AutoSetup] 🚪 Gắn DoorEnter cho: {obj.name}");
                }
                var den = obj.GetComponent<DoorEnter>();
                den.sceneName = "RoomVillage";
                den.hanhDongTuongTac = "vào nhà";
                
                EnsureNPCCollider(obj, true);
                SetLayerRecursive(obj, 6);
            }
            // 6. Cái chổi (Chỉ ở VillageScene)
            else if (lowerName.Contains("caychoi") || lowerName.Contains("broom") || lowerName.Contains("choi") || lowerName.Contains("quét") || lowerName.Contains("sweeper"))
            {
                if (sceneName == "VillageScene")
                {
                    // Đảm bảo có Collider để PlayerInteraction phát hiện được
                    if (obj.GetComponent<Collider>() == null)
                    {
                        // Thêm BoxCollider bao quanh mesh
                        var meshRenderer = obj.GetComponentInChildren<MeshRenderer>();
                        if (meshRenderer != null)
                        {
                            var bc = obj.AddComponent<BoxCollider>();
                            bc.center = obj.transform.InverseTransformPoint(meshRenderer.bounds.center);
                            bc.size = obj.transform.InverseTransformVector(meshRenderer.bounds.size) * 1.5f; // To hơn chút cho dễ bấm
                            bc.isTrigger = true;
                            Debug.Log($"[AutoSetup] 🛠 Đã tạo BoxCollider (Trigger) cho {obj.name}");
                        }
                        else
                        {
                            var bc = obj.AddComponent<BoxCollider>();
                            bc.size = new Vector3(1f, 2f, 1f);
                            bc.isTrigger = true;
                            Debug.Log($"[AutoSetup] ⚠️ Không thấy MeshRenderer, tạo BoxCollider mặc định cho {obj.name}");
                        }
                    }

                    if (obj.GetComponent<YardInteract>() == null)
                    {
                        var yi = obj.AddComponent<YardInteract>();
                        yi.tenNPC = "Cái Chổi";
                        yi.hanhDongTuongTac = "quét sân";
                    }
                    FreezeNPCPhysics(obj); // Call FreezeNPCPhysics after collider setup
                    SetLayerRecursive(obj, 6);
                }
            }
        }
        Debug.Log($"[AutoSetup] 🏁 Hoàn tất quét tương tác.");
    }

    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = layer;
        }
    }

    private static void EnsureNPCCollider(GameObject obj, bool useBox = false)
    {
        // Xóa collider cũ nếu nó không phải là trigger hoặc ở sai layer (để setup lại cho chuẩn)
        Collider oldCol = obj.GetComponent<Collider>();
        if (oldCol != null && (!oldCol.isTrigger && useBox)) 
        {
            // Nếu là vật cản vật lý cố định, có thể để lại, nhưng ta cần trigger để tương tác
        }

        if (useBox)
        {
            BoxCollider bc = obj.GetComponent<BoxCollider>();
            if (bc == null) bc = obj.AddComponent<BoxCollider>();
            
            bc.isTrigger = true; // Luôn để trigger cho tương tác
            
            Renderer rend = obj.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                bc.center = obj.transform.InverseTransformPoint(rend.bounds.center);
                bc.size = obj.transform.InverseTransformVector(rend.bounds.size) * 1.2f; // To hơn 20% cho dễ trúng
            }
            else
            {
                bc.size = new Vector3(1.5f, 2f, 1.5f);
                bc.center = new Vector3(0, 1f, 0);
            }
        }
        else
        {
            if (obj.GetComponent<Collider>() == null)
            {
                var cc = obj.AddComponent<CapsuleCollider>();
                cc.center = new Vector3(0, 1f, 0);
                cc.radius = 0.5f;
                cc.height = 2f;
                cc.isTrigger = true; // Luôn làm Trigger để tránh bị lật do va chạm
            }
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

    /// <summary>
    /// Chuẩn bị NPC trước khi AddComponent: chỉ tắt Animator root motion.
    /// KHÔNG thay đổi rotation vì scene đã set đúng rồi.
    /// NPCBase.Awake() sẽ cache và khóa rotation tự động.
    /// </summary>
    private static void ResetNPCRotation(GameObject obj)
    {
        if (obj == null) return;

        // Chỉ tắt root motion - rotation do NPCBase.Awake() cache và bảo vệ
        Animator[] animators = obj.GetComponentsInChildren<Animator>(true);
        foreach (var anim in animators)
            anim.applyRootMotion = false;

        Debug.Log($"[AutoSetup] 🧍 Đã chuẩn bị NPC: {obj.name} (root rotation giữ nguyên)");
    }

    private static void FreezeNPCPhysics(GameObject obj)
    {
        if (obj == null) return;
        
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) rb = obj.AddComponent<Rigidbody>();
        
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        
        // Khóa luôn các con nếu có
        Rigidbody[] childRbs = obj.GetComponentsInChildren<Rigidbody>(true);
        foreach (var c in childRbs)
        {
            c.isKinematic = true;
            c.useGravity = false;
            c.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

}
