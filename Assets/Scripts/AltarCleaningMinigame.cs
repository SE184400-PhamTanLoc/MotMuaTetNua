using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AltarCleaningMinigame - Mini game lau bàn thờ
/// Hiển thị canvas với ảnh bàn thờ, vết dơ, khăn lau và thanh tiến độ.
/// Khi lau xong 100% sẽ gọi RoomVillageManager.CleanAltar().
/// </summary>
public class AltarCleaningMinigame : MonoBehaviour
{
    public static AltarCleaningMinigame Instance { get; private set; }

    // ─── Cấu hình ─────────────────────────────────────────────
    [Header("=== CẤU HÌNH ===")]
    [Tooltip("Số vết dơ trên bàn thờ")]
    public int numberOfSpots = 8;

    [Tooltip("Tốc độ lau vết dơ (alpha giảm/giây khi khăn chạm)")]
    public float cleanSpeed = 1.5f;

    [Tooltip("Màu tối ban đầu của ảnh bàn thờ (chưa lau)")]
    public Color altarDirtyColor = new Color(0.38f, 0.33f, 0.28f, 1f);

    [Tooltip("Màu sáng của ảnh bàn thờ khi lau xong")]
    public Color altarCleanColor = Color.white;

    [Tooltip("Kích thước vùng lau của khăn (pixel trên canvas)")]
    public float clothRadius = 60f;

    // ─── Runtime refs (tạo bằng code) ─────────────────────────
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private Image _altarImage;
    private Image _clothCursor;
    private Slider _progressSlider;
    private Text _progressText;

    private readonly List<DirtySpot> _dirtySpots = new List<DirtySpot>();

    private bool _isOpen = false;
    public bool IsOpen => _isOpen;  // CursorStateController dùng để check
    private float _progress = 0f;        // 0 → 1

    // ─── mouse hold ───────────────────────────────────────────
    private bool _mouseHeld = false;

    // ─────────────────────────────────────────────────────────
    private class DirtySpot
    {
        public Image image;
        public RectTransform rect;
        public float alpha = 1f;
        public bool cleaned = false;
    }

    // ═════════════════════════════════════════════════════════
    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        BuildCanvas();
        CloseMinigame(instant: true);
    }

    private void Update()
    {
        if (!_isOpen) return;

        // Cloth cursor theo chuột
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.GetComponent<RectTransform>(),
            Input.mousePosition,
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
            out localPoint);

        if (_clothCursor != null)
            _clothCursor.rectTransform.anchoredPosition = localPoint;

        // Di chuột qua vết dơ → lau (không cần giữ nút)
        ProcessCleaning(localPoint);

        // Xoay nhẹ khăn theo hướng di chuyển chuột
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        if (mouseDelta.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(mouseDelta.y, mouseDelta.x) * Mathf.Rad2Deg;
            _clothCursor.rectTransform.rotation = Quaternion.Lerp(
                _clothCursor.rectTransform.rotation,
                Quaternion.Euler(0f, 0f, angle - 45f),
                Time.deltaTime * 12f);
        }

        // Cập nhật độ sáng bàn thờ
        if (_altarImage != null)
            _altarImage.color = Color.Lerp(altarDirtyColor, altarCleanColor, _progress);
    }
    #endregion

    // ═════════════════════════════════════════════════════════
    #region Public API

    public void OpenMinigame()
    {
        if (_isOpen) return;

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player movement + camera look
        SetPlayerMovement(false);

        // Reset spots
        ResetDirtySpots();
        _progress = 0f;
        UpdateProgressUI();

        // Show canvas
        _canvas.gameObject.SetActive(true);
        _isOpen = true;
    }

    public void CloseMinigame(bool instant = false)
    {
        _isOpen = false;
        _canvas.gameObject.SetActive(false);

        if (!instant)
        {
            // Re-lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SetPlayerMovement(true);
        }
    }

    #endregion

    // ═════════════════════════════════════════════════════════
    #region Cleaning Logic

    private void ProcessCleaning(Vector2 clothLocalPos)
    {
        bool anyChanged = false;

        foreach (var spot in _dirtySpots)
        {
            if (spot.cleaned) continue;

            // Khoảng cách giữa tâm khăn và tâm vết dơ (local space canvas)
            float dist = Vector2.Distance(clothLocalPos, spot.rect.anchoredPosition);
            float hitRadius = clothRadius * 0.7f + spot.rect.sizeDelta.x * 0.4f;

            if (dist <= hitRadius)
            {
                // Lau nhanh hơn khi chuột đang di chuyển
                float speed = cleanSpeed;
                Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                if (mouseDelta.sqrMagnitude > 0.1f) speed *= 2.5f;

                spot.alpha -= speed * Time.deltaTime;
                spot.alpha = Mathf.Max(0f, spot.alpha);

                Color c = spot.image.color;
                c.a = spot.alpha;
                spot.image.color = c;

                if (spot.alpha <= 0f)
                {
                    spot.cleaned = true;
                    spot.image.gameObject.SetActive(false);
                }
                anyChanged = true;
            }
        }

        if (anyChanged) RecalculateProgress();
    }

    private void RecalculateProgress()
    {
        if (_dirtySpots.Count == 0) return;

        float totalAlpha = 0f;
        foreach (var spot in _dirtySpots)
            totalAlpha += spot.alpha;

        // Progress = tỉ lệ alpha đã giảm
        float maxAlpha = _dirtySpots.Count; // mỗi spot ban đầu = 1.0
        _progress = 1f - (totalAlpha / maxAlpha);
        _progress = Mathf.Clamp01(_progress);

        UpdateProgressUI();

        if (_progress >= 1f)
            StartCoroutine(OnCleaningComplete());
    }

    private IEnumerator OnCleaningComplete()
    {
        // Đảm bảo ảnh sáng hẳn
        if (_altarImage != null)
            _altarImage.color = altarCleanColor;

        // Hiện thông báo ngắn
        if (_progressText != null)
            _progressText.text = "Sạch rồi! ✓";

        yield return new WaitForSeconds(1.2f);

        CloseMinigame();

        // Gọi manager hoàn thành nhiệm vụ
        if (RoomVillageManager.Instance != null)
            RoomVillageManager.Instance.CleanAltar();

        // Kích hoạt flashback ký ức
        FlashbackController.Instance?.StartFlashback();
    }

    private void UpdateProgressUI()
    {
        if (_progressSlider != null)
            _progressSlider.value = _progress;

        if (_progressText != null && _progress < 1f)
            _progressText.text = Mathf.RoundToInt(_progress * 100f) + "%";
    }

    #endregion

    // ═════════════════════════════════════════════════════════
    #region Canvas Builder

    private void BuildCanvas()
    {
        // ---------- Root Canvas ----------
        var canvasGO = new GameObject("AltarCleaningCanvas");
        canvasGO.transform.SetParent(transform);
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 50;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        _canvasGroup = canvasGO.AddComponent<CanvasGroup>();

        var canvasRT = canvasGO.GetComponent<RectTransform>();
        canvasRT.anchorMin = Vector2.zero;
        canvasRT.anchorMax = Vector2.one;
        canvasRT.offsetMin = Vector2.zero;
        canvasRT.offsetMax = Vector2.zero;

        // ---------- Background (dim overlay) ----------
        var bg = CreateUIImage(canvasGO, "Background");
        bg.color = new Color(0f, 0f, 0f, 0.75f);
        FillParent(bg.rectTransform);

        // ---------- Panel trung tâm ----------
        var panel = CreateUIObject("Panel", canvasGO);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.1f, 0.05f);
        panelRT.anchorMax = new Vector2(0.9f, 0.95f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // Nền panel mờ
        var panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.05f, 0.03f, 0.9f);

        // ---------- Altar Image ----------
        var altarGO = CreateUIObject("AltarImage", panel);
        var altarRT = altarGO.GetComponent<RectTransform>();
        altarRT.anchorMin = new Vector2(0.02f, 0.12f);
        altarRT.anchorMax = new Vector2(0.98f, 0.95f);
        altarRT.offsetMin = Vector2.zero;
        altarRT.offsetMax = Vector2.zero;

        _altarImage = altarGO.AddComponent<Image>();
        _altarImage.color = altarDirtyColor;
        _altarImage.preserveAspect = true;

        // Load ảnh từ Resources
        Sprite altarSprite = LoadSprite("NhiemVuLauBanTho/anhbantho");
        if (altarSprite != null)
            _altarImage.sprite = altarSprite;
        else
            _altarImage.color = new Color(0.3f, 0.22f, 0.15f, 1f); // fallback màu nâu

        // ---------- Dirty Spots (con của AltarImage) ----------
        var spotsParent = CreateUIObject("DirtySpots", altarGO);
        FillParent(spotsParent.GetComponent<RectTransform>());

        SpawnDirtySpots(spotsParent);

        // ---------- Progress Bar ----------
        var progressRoot = CreateUIObject("ProgressRoot", panel);
        var progressRT = progressRoot.GetComponent<RectTransform>();
        progressRT.anchorMin = new Vector2(0.1f, 0.02f);
        progressRT.anchorMax = new Vector2(0.9f, 0.10f);
        progressRT.offsetMin = Vector2.zero;
        progressRT.offsetMax = Vector2.zero;

        // Label "Tiến độ lau:"
        CreateLabel("LauLabel", progressRoot, "🧹 Tiến độ lau:", new Vector2(0f, 0f), new Vector2(0.3f, 1f));

        // Slider
        var sliderGO = CreateUIObject("ProgressSlider", progressRoot);
        var sliderRT = sliderGO.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.28f, 0.1f);
        sliderRT.anchorMax = new Vector2(0.88f, 0.9f);
        sliderRT.offsetMin = Vector2.zero;
        sliderRT.offsetMax = Vector2.zero;

        _progressSlider = sliderGO.AddComponent<Slider>();
        _progressSlider.minValue = 0f;
        _progressSlider.maxValue = 1f;
        _progressSlider.value = 0f;
        _progressSlider.interactable = false;

        // BG slider
        var sliderBG = CreateUIImage(sliderGO, "Background");
        FillParent(sliderBG.rectTransform);
        sliderBG.color = new Color(0.15f, 0.1f, 0.08f, 1f);

        // Fill area
        var fillArea = CreateUIObject("Fill Area", sliderGO);
        var fillAreaRT = fillArea.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0f, 0.1f);
        fillAreaRT.anchorMax = new Vector2(1f, 0.9f);
        fillAreaRT.offsetMin = new Vector2(5, 0);
        fillAreaRT.offsetMax = new Vector2(-5, 0);

        var fill = CreateUIImage(fillArea, "Fill");
        fill.color = new Color(0.85f, 0.65f, 0.2f, 1f); // màu vàng đất
        var fillRT = fill.rectTransform;
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        _progressSlider.fillRect = fill.rectTransform;

        // Text % 
        var textRT = CreateUIObject("ProgressText", progressRoot);
        var textRectT = textRT.GetComponent<RectTransform>();
        textRectT.anchorMin = new Vector2(0.88f, 0f);
        textRectT.anchorMax = new Vector2(1f, 1f);
        textRectT.offsetMin = Vector2.zero;
        textRectT.offsetMax = Vector2.zero;

        _progressText = textRT.AddComponent<Text>();
        _progressText.text = "0%";
        _progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _progressText.fontSize = 22;
        _progressText.color = new Color(1f, 0.9f, 0.6f, 1f);
        _progressText.alignment = TextAnchor.MiddleCenter;
        _progressText.fontStyle = FontStyle.Bold;

        // ---------- Title ----------
        var titleGO = CreateUIObject("Title", panel);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.93f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;
        var titleText = titleGO.AddComponent<Text>();
        titleText.text = "Lau Bàn Thờ - Di chuyển chuột và giữ LMB để lau";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 20;
        titleText.color = new Color(1f, 0.88f, 0.55f, 1f);
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;

        // ---------- Cloth Cursor (khăn theo chuột) ----------
        var clothGO = CreateUIObject("ClothCursor", canvasGO);
        _clothCursor = clothGO.AddComponent<Image>();
        _clothCursor.color = Color.white;
        _clothCursor.preserveAspect = true;
        _clothCursor.raycastTarget = false; // không chặn raycast

        var clothRT = clothGO.GetComponent<RectTransform>();
        float clothSize = clothRadius * 2.8f; // to hơn để thấy rõ
        clothRT.sizeDelta = new Vector2(clothSize, clothSize);
        clothRT.anchorMin = new Vector2(0.5f, 0.5f);
        clothRT.anchorMax = new Vector2(0.5f, 0.5f);
        clothRT.pivot = new Vector2(0.5f, 0.5f);

        // Load ảnh khăn thật (CaiKhan.png)
        Sprite clothSprite = LoadSprite("NhiemVuLauBanTho/CaiKhan");
        if (clothSprite != null)
        {
            _clothCursor.sprite = clothSprite;
        }
        else
        {
            // Fallback: hình chữ nhật trắng
            _clothCursor.color = new Color(0.95f, 0.93f, 0.87f, 0.88f);
        }

        // Ẩn system cursor, thay bằng cloth cursor
        Cursor.visible = false;
    }

    private void SpawnDirtySpots(GameObject parent)
    {
        _dirtySpots.Clear();

        // Load ảnh vết bẩn thật
        Sprite vetBanSprite = LoadSprite("NhiemVuLauBanTho/VetBan");

        // Vị trí các vết dơ – trải đều trên mặt bàn thờ
        // X: trải rộng theo chiều ngang (-350 → +350)
        // Y: tập trung ở giữa và phần trên của ảnh (mặt bàn thờ)
        Vector2[] positions =
        {
            new Vector2(-320f,  20f),   // góc trái trên
            new Vector2(-180f,  95f),   // trái giữa trên
            new Vector2( -50f,  60f),   // giữa hơi trái
            new Vector2( 100f, 110f),   // giữa phải trên
            new Vector2( 290f,  35f),   // góc phải trên
            new Vector2(-260f, -70f),   // trái dưới
            new Vector2(  20f, -55f),   // giữa dưới
            new Vector2( 210f, -85f),   // phải dưới
        };

        int count = Mathf.Min(numberOfSpots, positions.Length);

        for (int i = 0; i < count; i++)
        {
            var spotGO = new GameObject($"DirtySpot_{i}");
            spotGO.transform.SetParent(parent.transform, false);

            var img = spotGO.AddComponent<Image>();
            img.preserveAspect = true;

            if (vetBanSprite != null)
            {
                img.sprite = vetBanSprite;
                // Tint nhẹ để tạo sự khác biệt giữa các vết
                float tint = Random.Range(0.75f, 1.0f);
                img.color = new Color(tint, tint * 0.9f, tint * 0.8f, 0.92f);
            }
            else
            {
                // Fallback: hình chữ nhật nâu mờ
                float r = Random.Range(42f, 60f) / 255f;
                float g = Random.Range(30f, 45f) / 255f;
                float b = Random.Range(20f, 35f) / 255f;
                img.color = new Color(r, g, b, 0.92f);
            }

            // Kích thước đa dạng
            float size = Random.Range(70f, 115f);
            var rt = spotGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size * Random.Range(0.7f, 1.0f)); // hơi oval
            rt.anchoredPosition = positions[i];
            // Random xoay để trông tự nhiên
            rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            var ds = new DirtySpot { image = img, rect = rt, alpha = 0.92f };
            _dirtySpots.Add(ds);
        }
    }

    private void ResetDirtySpots()
    {
        _altarImage.color = altarDirtyColor;

        foreach (var spot in _dirtySpots)
        {
            spot.alpha = 0.92f;
            spot.cleaned = false;
            spot.image.gameObject.SetActive(true);
            Color c = spot.image.color;
            c.a = 0.92f;
            spot.image.color = c;
        }

        _progress = 0f;
        UpdateProgressUI();
    }

    #endregion

    // ═════════════════════════════════════════════════════════
    #region Helpers

    private static Sprite LoadSprite(string resourcePath)
    {
        var tex = Resources.Load<Texture2D>(resourcePath);
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                             new Vector2(0.5f, 0.5f), 100f);
    }

    private static Image CreateUIImage(GameObject parent, string name)
    {
        var go = CreateUIObject(name, parent);
        return go.AddComponent<Image>();
    }

    private static GameObject CreateUIObject(string name, GameObject parent)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    private static void FillParent(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void CreateLabel(string name, GameObject parent, string text,
                                    Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = CreateUIObject(name, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 20;
        t.color = new Color(1f, 0.9f, 0.7f, 1f);
        t.alignment = TextAnchor.MiddleRight;
    }

    private void SetPlayerMovement(bool enabled)
    {
        // Thử tắt/bật StarterAssets FirstPersonController
        var fpc = FindObjectOfType<StarterAssets.FirstPersonController>();
        if (fpc != null) fpc.enabled = enabled;

        // Thử tắt/bật PlayerMovement (legacy)
        var pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.enabled = enabled;

        // Tắt MouseLook
        var ml = FindObjectOfType<MouseLook>();
        if (ml != null) ml.enabled = enabled;
    }

    #endregion
}
