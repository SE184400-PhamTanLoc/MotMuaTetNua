using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

/// <summary>
/// GameHUD - Giao diện HUD hiển thị tiền, nhiệm vụ, thông báo, crosshair
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("=== TIỀN ===")]
    public TextMeshProUGUI tienText;

    [Header("=== NHIỆM VỤ ===")]
    public GameObject nhiemVuPanel;
    public TextMeshProUGUI nhiemVuText;

    [Header("=== THÔNG BÁO ===")]
    public GameObject thongBaoPanel;
    public TextMeshProUGUI thongBaoText;
    public float thoiGianThongBao = 3f;

    [Header("=== CROSSHAIR ===")]
    public Image crosshairImage;

    [Header("=== PANEL HOÀN THÀNH ===")]
    public GameObject hoanThanhPanel;
    public TextMeshProUGUI hoanThanhText;

    [Header("=== PANEL BẮT ĐẦU ===")]
    public GameObject batDauPanel;
    public TextMeshProUGUI batDauText;

    private Coroutine _coroutineThongBao;
    private bool _daKetNoi = false;

    [Header("=== NHIỆM VỤ: THU GỌN/MỞ RỘNG ===")]
    public KeyCode toggleNhiemVuKey = KeyCode.H;
    private bool _nhiemVuCollapsed = false;
    private Sprite _taskSpriteExpanded;
    private Sprite _taskSpriteCollapsed;
    private RectTransform _nhiemVuRect;
    private RectTransform _nhiemVuTextBgRect;
    private GameObject _nhiemVuTextBgGO;
    private Image _nhiemVuImage;
    private TMP_Text _toggleHintTMP;
    private Vector2 _expandedAnchorMin, _expandedAnchorMax;
    private Vector2 _expandedOffsetMin, _expandedOffsetMax;

    private void Start()
    {
        // Delay kết nối để đợi GameManager khởi tạo xong
        StartCoroutine(KetNoiSauDelay());
    }

    private IEnumerator KetNoiSauDelay()
    {
        // Chờ GameManager sẵn sàng
        while (GameManager.Instance == null)
            yield return null;

        // Đợi thêm 1 frame để Awake/Start của GM chạy xong
        yield return null;

        KetNoiEvents();
    }

    private void KetNoiEvents()
    {
        if (_daKetNoi || GameManager.Instance == null) return;
        _daKetNoi = true;

        GameManager.Instance.OnTienThayDoi.AddListener(CapNhatTien);
        GameManager.Instance.OnThongBao.AddListener(HienThongBao);
        GameManager.Instance.OnMuaMaiThanhCong.AddListener(OnMuaMaiThanhCong);
        GameManager.Instance.OnNhiemVuHoanThanh.AddListener(OnHoanThanhNhiemVu);
        GameManager.Instance.OnLayMaiThanhCong.AddListener(CapNhatNhiemVu);
        
        // Cập nhật HUD mỗi khi tiền thay đổi hoặc có thông báo (thường đi kèm update nhiệm vụ)
        GameManager.Instance.OnTienThayDoi.AddListener((t) => CapNhatNhiemVu());
        GameManager.Instance.OnNhiemVuThayDoi.AddListener(() => CapNhatNhiemVu());
        
        // Hiển thị ban đầu
        CapNhatTien(GameManager.Instance.SoTien);
        CapNhatNhiemVu();

        if (tienText != null && tienText.transform.parent != null)
        {
            tienText.transform.parent.gameObject.SetActive(true);
        }

        if (thongBaoPanel != null) thongBaoPanel.SetActive(false);
        if (hoanThanhPanel != null) hoanThanhPanel.SetActive(false);

        // BỎ CÁO THỊ NGÀY TẾT
        if (batDauPanel != null) batDauPanel.SetActive(false);

        Debug.Log("[GameHUD] ✅ Kết nối events và hiện Intro thành công");
        
        // Nếu bảng nhiệm vụ có MissionUIController thì để controller đó quản lý thu gọn/mở rộng,
        // tránh bị trùng logic với GameHUD.
        if (nhiemVuPanel == null || nhiemVuPanel.GetComponent<MissionUIController>() != null)
        {
            return;
        }

        CacheTaskPanelParts();
        ApplyTaskPanelCollapsedState(force: true);
    }

    private void CacheTaskPanelParts()
    {
        if (nhiemVuPanel == null) return;

        _nhiemVuRect = nhiemVuPanel.GetComponent<RectTransform>();
        _nhiemVuImage = nhiemVuPanel.GetComponent<Image>();

        if (_nhiemVuRect != null)
        {
            _expandedAnchorMin = _nhiemVuRect.anchorMin;
            _expandedAnchorMax = _nhiemVuRect.anchorMax;
            _expandedOffsetMin = _nhiemVuRect.offsetMin;
            _expandedOffsetMax = _nhiemVuRect.offsetMax;
        }

        // Find text bg created by AutoSetup
        Transform bgT = nhiemVuPanel.transform.Find("NhiemVuTextBg");
        if (bgT != null)
        {
            _nhiemVuTextBgGO = bgT.gameObject;
            _nhiemVuTextBgRect = bgT.GetComponent<RectTransform>();
        }

        Transform hintT = nhiemVuPanel.transform.Find("NhiemVuToggleHint");
        if (hintT != null) _toggleHintTMP = hintT.GetComponent<TMP_Text>();

        _taskSpriteExpanded = Resources.Load<Sprite>("BangNhiemVu");
        _taskSpriteCollapsed = Resources.Load<Sprite>("BangNhiemVuThuGon");
        if (_taskSpriteExpanded == null)
        {
            Texture2D t = Resources.Load<Texture2D>("BangNhiemVu");
            if (t != null) _taskSpriteExpanded = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
        }
        if (_taskSpriteCollapsed == null)
        {
            Texture2D t = Resources.Load<Texture2D>("BangNhiemVuThuGon");
            if (t != null) _taskSpriteCollapsed = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
        }
    }

    private void ApplyTaskPanelCollapsedState(bool force = false)
    {
        if (_nhiemVuRect == null || _nhiemVuImage == null) return;

        if (_nhiemVuCollapsed)
        {
            // Small header strip (giống ảnh BangNhiemVuThuGon)
            // Tăng kích thước thu gọn to rõ hơn theo yêu cầu (Gốc: 0.90f)
            _nhiemVuRect.anchorMin = new Vector2(0.01f, 0.65f);
            _nhiemVuRect.anchorMax = new Vector2(0.35f, 0.985f);
            _nhiemVuRect.offsetMin = Vector2.zero;
            _nhiemVuRect.offsetMax = Vector2.zero;

            if (_taskSpriteCollapsed != null)
            {
                _nhiemVuImage.sprite = _taskSpriteCollapsed;
                _nhiemVuImage.color = Color.white;
                _nhiemVuImage.type = Image.Type.Simple;
                _nhiemVuImage.preserveAspect = true;
            }

            if (_nhiemVuTextBgGO != null) _nhiemVuTextBgGO.SetActive(false);
            if (nhiemVuText != null) nhiemVuText.gameObject.SetActive(false);
            if (_toggleHintTMP != null) { _toggleHintTMP.text = "[H] Mở rộng"; _toggleHintTMP.gameObject.SetActive(true); }
        }
        else
        {
            // Back to expanded layout (AutoSetup)
            _nhiemVuRect.anchorMin = _expandedAnchorMin;
            _nhiemVuRect.anchorMax = _expandedAnchorMax;
            _nhiemVuRect.offsetMin = _expandedOffsetMin;
            _nhiemVuRect.offsetMax = _expandedOffsetMax;

            if (_taskSpriteExpanded != null)
            {
                _nhiemVuImage.sprite = _taskSpriteExpanded;
                _nhiemVuImage.color = Color.white;
                _nhiemVuImage.type = Image.Type.Simple;
                _nhiemVuImage.preserveAspect = true;
            }

            if (_nhiemVuTextBgGO != null) _nhiemVuTextBgGO.SetActive(true);
            if (nhiemVuText != null) nhiemVuText.gameObject.SetActive(true);
            if (_toggleHintTMP != null) { _toggleHintTMP.text = "[H] Thu gọn"; _toggleHintTMP.gameObject.SetActive(true); }
        }
    }

    public void DongBatDauPanel()
    {
        if (batDauPanel != null && batDauPanel.activeSelf)
        {
            batDauPanel.SetActive(false);
            // Cursor được quản lý bởi CursorStateController
        }
    }

    private void CapNhatTien(int soTien)
    {
        if (tienText != null)
        {
            tienText.text = $"[Tiền] {GameManager.FormatTien(soTien)}";
            
            if (tienText.transform.parent != null && !tienText.transform.parent.gameObject.activeSelf)
            {
                tienText.transform.parent.gameObject.SetActive(true);
            }
        }
    }

    private void CapNhatNhiemVu()
    {
        if (nhiemVuText != null && GameManager.Instance != null)
        {
            string taskText = GameManager.Instance.LayMoTaNhiemVu();
            nhiemVuText.text = taskText;

            if (nhiemVuPanel != null)
            {
                nhiemVuPanel.SetActive(!string.IsNullOrEmpty(taskText));
            }

            if (tienText != null && tienText.transform.parent != null)
            {
                if (!tienText.transform.parent.gameObject.activeSelf)
                {
                    tienText.transform.parent.gameObject.SetActive(true);
                }
            }
        }
    }

    public void HienThongBao(string noiDung)
    {
        if (thongBaoPanel == null || thongBaoText == null) return;

        if (_coroutineThongBao != null)
            StopCoroutine(_coroutineThongBao);

        _coroutineThongBao = StartCoroutine(HienThongBaoCoroutine(noiDung));
    }

    private IEnumerator HienThongBaoCoroutine(string noiDung)
    {
        thongBaoText.text = noiDung;
        thongBaoPanel.SetActive(true);

        CanvasGroup canvasGroup = thongBaoPanel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            float timer = 0;
            while (timer < 0.3f)
            {
                timer += Time.unscaledDeltaTime;
                canvasGroup.alpha = timer / 0.3f;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }

        yield return new WaitForSeconds(thoiGianThongBao);

        if (canvasGroup != null)
        {
            float timer = 0;
            while (timer < 0.5f)
            {
                timer += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1 - (timer / 0.5f);
                yield return null;
            }
        }

        thongBaoPanel.SetActive(false);
    }

    private void OnMuaMaiThanhCong()
    {
        CapNhatNhiemVu();
    }

    private void OnHoanThanhNhiemVu()
    {
        CapNhatNhiemVu();
        
        // Chỉ hiện bảng chúc mừng khi ĐÃ XONG CẢ 3 nhiệm vụ
        if (GameManager.Instance.daMangMaiVeMe && 
            GameManager.Instance.DaThuThapDuNguyenLieu() && 
            GameManager.Instance.DaThuThapDuNguQua())
        {
            if (hoanThanhPanel != null)
            {
                hoanThanhPanel.SetActive(true);
                if (hoanThanhText != null)
                {
                    hoanThanhText.text = "<color=#FFD700><b>[ CHÚC MỪNG NĂM MỚI! ]</b></color>\n\n" +
                                         "Bạn đã sắm sửa đầy đủ cho ngày Tết rồi!\n" +
                                         "Cây mai đã có, nguyên liệu gói bánh cũng xong.\n\n" +
                                         "<b>Hãy mau trở về nhà chuẩn bị đón Tết cùng gia đình thôi!</b>\n\n" +
                                         "<size=20><i>(Nhấn phím Space hoặc E để đóng)</i></size>";
                }
            }
        }
    }

    private void Update()
    {
        // Toggle bảng nhiệm vụ bằng H (chỉ khi KHÔNG có MissionUIController)
        if (nhiemVuPanel != null && nhiemVuPanel.GetComponent<MissionUIController>() == null)
        {
            bool togglePressed = (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame) ||
                                 Input.GetKeyDown(toggleNhiemVuKey);
            if (togglePressed && nhiemVuPanel.activeSelf)
            {
                _nhiemVuCollapsed = !_nhiemVuCollapsed;
                ApplyTaskPanelCollapsedState();
            }
        }

        if (batDauPanel != null && batDauPanel.activeSelf)
        {
            batDauPanel.SetActive(false);
            return;
        }

        if (hoanThanhPanel != null && hoanThanhPanel.activeSelf)
        {
            bool spacePressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) || 
                                Input.GetKeyDown(KeyCode.Space) || 
                                Input.GetKeyDown(KeyCode.Return) ||
                                Input.GetKeyDown(KeyCode.E); // Hỗ trợ E luôn

            if (spacePressed)
            {
                hoanThanhPanel.SetActive(false);
            }
            return; // Đang hiện panel hoàn thành thì không làm gì khác
        }

        if (crosshairImage != null)
        {
            bool dangHoiThoai = DialogueManager.Instance != null && DialogueManager.Instance.DangHoiThoai;
            crosshairImage.enabled = !dangHoiThoai;
        }
    }
}
