using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// MissionUIController - Quản lý việc thu gọn/mở rộng bảng nhiệm vụ với hiệu ứng
/// </summary>
public class MissionUIController : MonoBehaviour
{
    [Header("=== Sprites ===")]
    public Sprite expandedSprite;
    public Sprite minimizedSprite;

    [Header("=== References ===")]
    public GameObject contentContainer; // Container chứa text
    public RectTransform panelRect;
    public TMPro.TextMeshProUGUI hintText;

    private Image _panelImage;
    private bool _isExpanded = true;
    private bool _isHiddenCompletely = false; // Mới: theo dõi trạng thái ẩn hẳn
    private Coroutine _animCoroutine;
    private Coroutine _autoHideCoroutine; // Mới: coroutine tự động ẩn sau khi thu gọn
    
    // Lưu lại anchors ban đầu để bung ra đúng chỗ
    private Vector2 _originalAnchorMax;
    private Vector2 _minimizedAnchorMax;
    private CanvasGroup _canvasGroup; // Cache CanvasGroup

    void Awake()
    {
        _panelImage = GetComponent<Image>();
        panelRect = GetComponent<RectTransform>();
    }

    private Vector2 _originalAnchorMin;
    private Vector2 _minimizedAnchorMin;
    private float _fixedTopY = 0.985f;

    void Start()
    {
        _fixedTopY = panelRect.anchorMax.y;
        _originalAnchorMin = panelRect.anchorMin;
        _originalAnchorMax = panelRect.anchorMax;
        
        // Tải lại sprite một cách chắc chắn nếu AutoSetup truyền vào null
        if (expandedSprite == null) expandedSprite = LoadSpriteRobust("BangNhiemVu");
        if (minimizedSprite == null) minimizedSprite = LoadSpriteRobust("BangNhiemVuThuGon");

        _isExpanded = true;
        _canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (_panelImage != null && expandedSprite != null) 
        {
            _panelImage.sprite = expandedSprite;
            _panelImage.type = Image.Type.Sliced;
            _panelImage.preserveAspect = false; 
        }

        UpdateHeight();
    }

    private Sprite LoadSpriteRobust(string resourcePath)
    {
        Sprite s = Resources.Load<Sprite>(resourcePath);
        if (s == null)
        {
            Texture2D tex = Resources.Load<Texture2D>(resourcePath);
            if (tex != null)
            {
                s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
        return s;
    }

    private string _lastText = "";
    void Update()
    {
        // Phím tắt H để thu gọn/mở rộng
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.hKey.wasPressedThisFrame)
        {
            Toggle();
        }

        // Theo dõi sự thay đổi của text để cập nhật độ cao
        if (contentContainer != null)
        {
            TMPro.TextMeshProUGUI text = contentContainer.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null && text.text != _lastText)
            {
                _lastText = text.text;
                UpdateHeight();
            }
        }
    }

    public void UpdateHeight()
    {
        if (!_isExpanded || contentContainer == null) return;
        
        TMPro.TextMeshProUGUI text = contentContainer.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text == null) return;

        // Tính toán độ cao dựa trên preferredHeight của text
        float preferredHeight = text.preferredHeight;
        float screenHeight = Screen.height;
        
        // Chiều cao tối thiểu cho header và footer ảnh (px)
        float paddingPx = 250f;
        float heightRatio = (preferredHeight + paddingPx) / screenHeight; 
        
        // Pin TOP (anchorMax.y), thay đổi BOTTOM (anchorMin.y)
        float newMinY = Mathf.Clamp(_fixedTopY - heightRatio, 0.4f, _fixedTopY - 0.15f);
        
        _originalAnchorMin = new Vector2(_originalAnchorMin.x, newMinY);
        
        // Nếu đang mở thì cập nhật trực tiếp
        if (_isExpanded && _animCoroutine == null)
        {
            panelRect.anchorMin = _originalAnchorMin;
        }

        // Vị trí thu gọn - Tăng kích thước để thấy rõ ảnh "BangNhiemVuThuGon"
        float minimizedHeightRatio = 0.45f; // Tăng mạnh độ cao thu gọn (Gốc: 0.25f)
        _minimizedAnchorMin = new Vector2(_originalAnchorMin.x, _fixedTopY - minimizedHeightRatio);
        // Nới rộng chiều ngang đáng kể (gần bằng bảng thật)
        _minimizedAnchorMax = new Vector2(_originalAnchorMax.x, _fixedTopY);
    }

    public void Toggle()
    {
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        if (_autoHideCoroutine != null) 
        {
            StopCoroutine(_autoHideCoroutine);
            _autoHideCoroutine = null;
        }

        // Nếu đang ẩn hẳn thì khi bấm H phải hiện lại trước
        if (_isHiddenCompletely)
        {
            _isHiddenCompletely = false;
            _canvasGroup.alpha = 1f;
            _isExpanded = false; // Giả định là đang thu gọn để bung ra (hoặc tùy logic)
        }

        _animCoroutine = StartCoroutine(AnimateToggle());
    }

    /// <summary>
    /// Cho phép các script khác (như Settings) ẩn/hiện bảng nhiệm vụ
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = visible ? (_isHiddenCompletely ? 0f : 1f) : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }
    }

    private IEnumerator AnimateToggle()
    {
        _isExpanded = !_isExpanded;
        float duration = 0.8f; // Làm chậm lại hiệu ứng (Gốc: 0.4f)
        float timer = 0f;

        if (_isExpanded) UpdateHeight();

        Vector2 startAnchorMin = panelRect.anchorMin;
        Vector2 startAnchorMax = panelRect.anchorMax;
        Vector2 endAnchorMin = _isExpanded ? _originalAnchorMin : _minimizedAnchorMin;
        Vector2 endAnchorMax = _isExpanded ? _originalAnchorMax : _minimizedAnchorMax;
        
        CanvasGroup cg = contentContainer.GetComponent<CanvasGroup>();
        if (cg == null) cg = contentContainer.AddComponent<CanvasGroup>();

        float startAlpha = cg.alpha;
        float endAlpha = _isExpanded ? 1f : 0f;

        if (_isExpanded)
        {
            contentContainer.SetActive(true);
            _panelImage.sprite = expandedSprite;
            _panelImage.type = Image.Type.Sliced;
            _panelImage.preserveAspect = false;
            if (hintText != null) hintText.text = "[H] Thu gọn";
        }

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            t = t * t * (3f - 2f * t);

            panelRect.anchorMin = Vector2.Lerp(startAnchorMin, endAnchorMin, t);
            panelRect.anchorMax = Vector2.Lerp(startAnchorMax, endAnchorMax, t);
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            
            // Hint text mờ đi một chút khi thu gọn
            if (hintText != null) hintText.alpha = Mathf.Lerp(0.5f, 0.3f, _isExpanded ? (1-t) : t);

            yield return null;
        }

        panelRect.anchorMin = endAnchorMin;
        panelRect.anchorMax = endAnchorMax;
        cg.alpha = endAlpha;

        if (!_isExpanded)
        {
            contentContainer.SetActive(false);
            _panelImage.sprite = minimizedSprite;
            _panelImage.type = Image.Type.Sliced;
            _panelImage.preserveAspect = false;
            if (hintText != null) hintText.text = "[H] Mở rộng";

            // Sau khi thu gọn thành công, bắt đầu đếm ngược để ẩn hẳn
            _autoHideCoroutine = StartCoroutine(AutoHideAfterDelay(3f));
        }
        else
        {
            _isHiddenCompletely = false;
            _canvasGroup.alpha = 1f;
        }

        _animCoroutine = null;
    }

    private IEnumerator AutoHideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        float timer = 0f;
        float duration = 1f; // Mờ dần trong 1s
        float startAlpha = _canvasGroup.alpha;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / duration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _isHiddenCompletely = true;
        _autoHideCoroutine = null;
    }
}
