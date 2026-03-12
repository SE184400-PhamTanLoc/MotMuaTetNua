using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// YardSweepingMinigame - Mini game quét sân
/// Hiển thị canvas với ảnh sân, lá rụng, cái chổi và thanh tiến độ.
/// Khi quét xong 100% sẽ gọi RoomVillageManager.Instance.SweepYard().
/// </summary>
public class YardSweepingMinigame : MonoBehaviour
{
    public static YardSweepingMinigame Instance { get; private set; }

    // ─── Cấu hình ─────────────────────────────────────────────
    [Header("=== CẤU HÌNH ===")]
    [Tooltip("Số lá trên sân")]
    public int numberOfLeaves = 10;

    [Tooltip("Tốc độ quét lá (alpha giảm/giây khi chổi chạm)")]
    public float sweepSpeed = 2.0f;

    [Tooltip("Kích thước vùng quét của chổi (pixel trên canvas)")]
    public float broomRadius = 80f;

    [Header("=== SPRITES (CÓ THỂ GÁN TAY) ===")]
    [Tooltip("Sprite nền sân. Nếu để trống sẽ tự load từ Resources.")]
    public Sprite yardBackgroundSprite;
    [Tooltip("Sprite lá rụng. Nếu để trống sẽ tự load từ Resources.")]
    public Sprite leafSprite;
    [Tooltip("Sprite cái chổi. Nếu để trống sẽ tự load từ Resources.")]
    public Sprite broomSprite;

    // ─── Runtime refs ─────────────────────────────────────────
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private Image _yardImage;
    private Image _broomCursor;
    private Slider _progressSlider;
    private Text _progressText;
    private GameObject _successPanel; // Bảng thông báo hoàn thành

    private readonly List<LeafSpot> _leafSpots = new List<LeafSpot>();

    private bool _isOpen = false;
    public bool IsOpen => _isOpen;
    private float _progress = 0f;

    private class LeafSpot
    {
        public Image image;
        public RectTransform rect;
        public float alpha = 1f;
        public bool swept = false;
    }

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

        // Broom cursor theo chuột
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.GetComponent<RectTransform>(),
            Input.mousePosition,
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
            out localPoint);

        if (_broomCursor != null)
            _broomCursor.rectTransform.anchoredPosition = localPoint;

        // Xử lý quét lá
        ProcessSweeping(localPoint);

        // Xoay nhẹ chổi theo hướng di chuyển chuột (nhưng giữ góc đẹp, không xoay quá mạnh)
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        if (mouseDelta.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(mouseDelta.y, mouseDelta.x) * Mathf.Rad2Deg;
            // Offset để đầu chổi hướng về phía quét, giới hạn trong [-60, 60] cho đỡ kỳ
            float targetZ = Mathf.Clamp(angle - 70f, -60f, 60f);
            _broomCursor.rectTransform.rotation = Quaternion.Lerp(
                _broomCursor.rectTransform.rotation,
                Quaternion.Euler(0f, 0f, targetZ),
                Time.deltaTime * 8f);
        }
    }

    public void OpenMinigame()
    {
        if (_isOpen) return;

        // Đảm bảo Canvas đã được dựng
        if (_canvas == null || _leafSpots.Count == 0)
        {
            Debug.Log("[YardSweepingMinigame] Rebuilding Canvas...");
            BuildCanvas();
        }

        // Ẩn cursor hệ thống, dùng chổi làm cursor giống minigame lau bàn thờ
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        SetPlayerMovement(false);

        ResetLeaves();
        _progress = 0f;
        UpdateProgressUI();

        if (_successPanel != null) _successPanel.SetActive(false);
        _canvas.gameObject.SetActive(true);
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
        _isOpen = true;
        
        Debug.Log("[YardSweepingMinigame] Opened. Leaves count: " + _leafSpots.Count);
    }

    public void CloseMinigame(bool instant = false)
    {
        _isOpen = false;
        if (_canvas != null) _canvas.gameObject.SetActive(false);

        if (!instant)
        {
            // Khóa lại chuột và ẩn/minh họa giống minigame lau bàn thờ
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SetPlayerMovement(true);
        }
    }

    private void ProcessSweeping(Vector2 broomLocalPos)
    {
        bool anyChanged = false;
        foreach (var leaf in _leafSpots)
        {
            if (leaf.swept) continue;

            float dist = Vector2.Distance(broomLocalPos, leaf.rect.anchoredPosition);
            float hitRadius = broomRadius * 0.8f;

            if (dist <= hitRadius)
            {
                float speed = sweepSpeed;
                Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                if (mouseDelta.sqrMagnitude > 0.1f) speed *= 3f;

                leaf.alpha -= speed * Time.deltaTime;
                leaf.alpha = Mathf.Max(0f, leaf.alpha);

                Color c = leaf.image.color;
                c.a = leaf.alpha;
                leaf.image.color = c;

                if (leaf.alpha <= 0f)
                {
                    leaf.swept = true;
                    leaf.image.gameObject.SetActive(false);
                }
                anyChanged = true;
            }
        }
        if (anyChanged) RecalculateProgress();
    }

    private void RecalculateProgress()
    {
        if (_leafSpots.Count == 0) return;
        float totalAlpha = 0f;
        foreach (var leaf in _leafSpots) totalAlpha += leaf.alpha;
        _progress = 1f - (totalAlpha / _leafSpots.Count);
        _progress = Mathf.Clamp01(_progress);
        UpdateProgressUI();

        if (_progress >= 1f) StartCoroutine(OnSweepingComplete());
    }

    private IEnumerator OnSweepingComplete()
    {
        if (_progressText != null) _progressText.text = "Sạch bóng! ✓";
        if (_successPanel != null) _successPanel.SetActive(true);
        
        yield return new WaitForSeconds(2.0f);
        CloseMinigame();
        if (RoomVillageManager.Instance != null)
            RoomVillageManager.Instance.SweepYard();
    }

    private void UpdateProgressUI()
    {
        if (_progressSlider != null) _progressSlider.value = _progress;
        if (_progressText != null && _progress < 1f)
            _progressText.text = Mathf.RoundToInt(_progress * 100f) + "%";
    }

    private void BuildCanvas()
    {
        var canvasGO = new GameObject("YardSweepingCanvas");
        canvasGO.transform.SetParent(transform);
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 60; // gần giống AltarCleaningCanvas

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        _canvasGroup = canvasGO.AddComponent<CanvasGroup>();

        // Background dim
        var bg = CreateUIObject("Background", canvasGO);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.75f);
        FillParent(bg.GetComponent<RectTransform>());

        // Central panel giống bố cục lau bàn thờ
        var panel = CreateUIObject("YardPanel", canvasGO);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.1f, 0.05f);
        panelRT.anchorMax = new Vector2(0.9f, 0.95f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        var panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.05f, 0.03f, 0.9f);

        // Yard image chiếm phần lớn panel, giống ảnh bàn thờ
        var yardGO = CreateUIObject("YardImage", panel);
        var yardRT = yardGO.GetComponent<RectTransform>();
        yardRT.anchorMin = new Vector2(0.02f, 0.15f);
        yardRT.anchorMax = new Vector2(0.98f, 0.95f);
        yardRT.offsetMin = Vector2.zero;
        yardRT.offsetMax = Vector2.zero;

        _yardImage = yardGO.AddComponent<Image>();
        _yardImage.sprite = ResolveSprite(
            yardBackgroundSprite,
            new[]
            {
                "NhiemVuQuetSan/CaiSan",
                "NhiemVuQuetSan/san",
                "NhiemVuQuetSan/san_quet",
            },
            "Nền sân quét lá"
        );
        _yardImage.color = Color.white;
        _yardImage.preserveAspect = true;

        // Leaves Parent (là con của YardImage để khớp ảnh nền)
        var leavesParent = CreateUIObject("Leaves", yardGO);
        FillParent(leavesParent.GetComponent<RectTransform>());
        SpawnLeaves(leavesParent);

        // Progress Root (dưới panel giống lau bàn thờ)
        var progressRoot = CreateUIObject("ProgressRoot", panel);
        var progressRT = progressRoot.GetComponent<RectTransform>();
        progressRT.anchorMin = new Vector2(0.1f, 0.02f);
        progressRT.anchorMax = new Vector2(0.9f, 0.10f);
        progressRT.offsetMin = Vector2.zero;
        progressRT.offsetMax = Vector2.zero;

        // Label
        var labelGO = CreateUIObject("Label", progressRoot);
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0f, 0f);
        labelRT.anchorMax = new Vector2(0.3f, 1f);
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;
        var labelText = labelGO.AddComponent<Text>();
        labelText.text = "🧹 Tiến độ quét sân:";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 20;
        labelText.color = new Color(1f, 0.9f, 0.7f, 1f);
        labelText.alignment = TextAnchor.MiddleRight;

        // Progress Slider
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

        var sliderBg = CreateUIObject("BG", sliderGO).AddComponent<Image>();
        sliderBg.color = new Color(0.15f, 0.1f, 0.08f, 1f);
        FillParent(sliderBg.rectTransform);
        var fillArea = CreateUIObject("FillArea", sliderGO);
        var fill = CreateUIObject("Fill", fillArea).AddComponent<Image>();
        fill.color = new Color(0.3f, 0.8f, 0.3f, 1f);
        FillParent(fill.rectTransform);
        _progressSlider.fillRect = fill.rectTransform;

        var textGO = CreateUIObject("ProgressText", progressRoot);
        _progressText = textGO.AddComponent<Text>();
        _progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _progressText.alignment = TextAnchor.MiddleCenter;
        _progressText.fontSize = 22;
        _progressText.color = new Color(1f, 0.9f, 0.6f, 1f);
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.88f, 0f);
        textRT.anchorMax = new Vector2(1f, 1f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        // Title
        var titleGO = CreateUIObject("Title", panel);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 0.93f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;
        var titleText = titleGO.AddComponent<Text>();
        titleText.text = "Quét Sân - Di chuyển chuột để quét hết lá khô";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 20;
        titleText.color = new Color(1f, 0.88f, 0.55f, 1f);
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;

        // Broom Cursor (icon chổi đẹp giống khăn lau bàn thờ)
        var broomGO = CreateUIObject("BroomCursor", canvasGO);
        _broomCursor = broomGO.AddComponent<Image>();
        _broomCursor.sprite = ResolveSprite(
            broomSprite,
            new[]
            {
                "NhiemVuQuetSan/CaiChoi",
                "NhiemVuQuetSan/choi",
                "NhiemVuQuetSan/broom",
            },
            "Cái chổi quét sân"
        );
        _broomCursor.color = Color.white;
        _broomCursor.preserveAspect = true;
        _broomCursor.raycastTarget = false;
        var broomRT = broomGO.GetComponent<RectTransform>();

        // Nếu có sprite mới -> scale theo tỉ lệ cho đẹp, dài hơn theo chiều dọc
        float broomBaseSize = broomRadius * 2.7f;
        if (_broomCursor.sprite != null)
        {
            float aspect = (float)_broomCursor.sprite.rect.height / Mathf.Max(1f, _broomCursor.sprite.rect.width);
            broomRT.sizeDelta = new Vector2(broomBaseSize, broomBaseSize * aspect);
        }
        else
        {
            broomRT.sizeDelta = new Vector2(broomBaseSize, broomBaseSize * 1.6f);
        }

        // Pivot gần phần tay cầm để cảm giác quét tự nhiên
        broomRT.pivot = new Vector2(0.4f, 0.15f);
        broomRT.anchorMin = new Vector2(0.5f, 0.5f);
        broomRT.anchorMax = new Vector2(0.5f, 0.5f);

        // Success Panel
        _successPanel = CreateUIObject("SuccessPanel", canvasGO);
        var successRT = _successPanel.GetComponent<RectTransform>();
        successRT.sizeDelta = new Vector2(400, 200);
        successRT.anchoredPosition = Vector2.zero;
        
        var sImg = _successPanel.AddComponent<Image>();
        sImg.color = new Color(0, 0, 0, 0.85f);
        
        var sTextGO = CreateUIObject("Text", _successPanel);
        var sText = sTextGO.AddComponent<Text>();
        sText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        sText.text = "HOÀN THÀNH!";
        sText.fontSize = 40;
        sText.color = Color.yellow;
        sText.alignment = TextAnchor.MiddleCenter;
        FillParent(sTextGO.GetComponent<RectTransform>());
        
        _successPanel.SetActive(false);
    }

    private void SpawnLeaves(GameObject parent)
    {
        Sprite usedLeafSprite = ResolveSprite(
            leafSprite,
            new[]
            {
                "NhiemVuQuetSan/CaiLa",
                "NhiemVuQuetSan/la",
                "NhiemVuQuetSan/la_quet",
            },
            "Lá rụng trên sân"
        );
        for (int i = 0; i < numberOfLeaves; i++)
        {
            var go = CreateUIObject($"Leaf_{i}", parent);
            var img = go.AddComponent<Image>();
            img.sprite = usedLeafSprite;
            img.preserveAspect = true;
            var rt = go.GetComponent<RectTransform>();
            // Giới hạn vùng sinh lá để nằm trong Panel sân (tránh bị tràn ra rìa canvas khó quét)
            rt.anchoredPosition = new Vector2(Random.Range(-450f, 450f), Random.Range(-250f, 250f));
            rt.sizeDelta = new Vector2(80, 80);
            rt.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));
            _leafSpots.Add(new LeafSpot { image = img, rect = rt });
        }
    }

    private void ResetLeaves()
    {
        foreach (var leaf in _leafSpots)
        {
            leaf.alpha = 1f;
            leaf.swept = false;
            leaf.image.gameObject.SetActive(true);
            leaf.image.color = Color.white;
        }
    }

    private GameObject CreateUIObject(string name, GameObject parent)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    private void FillParent(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private Sprite ResolveSprite(Sprite overrideSprite, string[] resourcePaths, string debugName)
    {
        if (overrideSprite != null) return overrideSprite;

        foreach (var path in resourcePaths)
        {
            var sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                Debug.Log($"[YardSweepingMinigame] Đã load sprite {debugName} từ Resources/{path}");
                return sprite;
            }

            var tex = Resources.Load<Texture2D>(path);
            if (tex != null)
            {
                Debug.Log($"[YardSweepingMinigame] Đã tạo sprite {debugName} từ Texture2D Resources/{path}");
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }

        Debug.LogWarning($"[YardSweepingMinigame] KHÔNG TÌM THẤY sprite cho {debugName}. Vui lòng gán tay trong Inspector.");
        return null;
    }

    private void SetPlayerMovement(bool enabled)
    {
        var fpc = FindObjectOfType<StarterAssets.FirstPersonController>();
        if (fpc != null) fpc.enabled = enabled;
        var pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.enabled = enabled;
        var ml = FindObjectOfType<MouseLook>();
        if (ml != null) ml.enabled = enabled;
    }
}
