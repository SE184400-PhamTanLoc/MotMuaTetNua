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

        if (thongBaoPanel != null) thongBaoPanel.SetActive(false);
        if (hoanThanhPanel != null) hoanThanhPanel.SetActive(false);

        // Hiển thị intro
        if (batDauPanel != null)
        {
            batDauPanel.SetActive(true);
            if (batDauText != null)
            {
                batDauText.text = "[ NHIỆM VỤ NGÀY TẾT ]\n\n" +
                                 "Mẹ dặn bạn ra chợ Tết chọn mua một cây mai thật đẹp về chưng nhà cho có không khí.\n\n" +
                                 "- <b>Nhiệm vụ 1:</b> Tìm cô gái bán mai và mua 1 cây (Nho/Lớn).\n" +
                                 "- <b>Nhiệm vụ 2:</b> Mua đầy đủ nguyên liệu để gói bánh chưng.\n" +
                                 "- <b>Nhiệm vụ 3:</b> Thu thập đủ 4 loại trái cây chưng mâm Ngũ Quả.\n\n" +
                                 "<size=22><i>(Nhấn Space hoặc nút bên dưới để bắt đầu)</i></size>";
            }
            
            Button btn = batDauPanel.GetComponentInChildren<Button>();
            if (btn != null) btn.onClick.AddListener(DongBatDauPanel);

            // Cursor được quản lý bởi CursorStateController
        }

        Debug.Log("[GameHUD] ✅ Kết nối events và hiện Intro thành công");
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
            tienText.text = $"[Tiền] {GameManager.FormatTien(soTien)}";
    }

    private void CapNhatNhiemVu()
    {
        if (nhiemVuText != null && GameManager.Instance != null)
            nhiemVuText.text = GameManager.Instance.LayMoTaNhiemVu();
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
        if (batDauPanel != null && batDauPanel.activeSelf)
        {
            // Kiểm tra cả Input System mới và cũ để đảm bảo luôn hoạt động
            bool spacePressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) || 
                                Input.GetKeyDown(KeyCode.Space) || 
                                Input.GetKeyDown(KeyCode.Return);

            if (spacePressed)
            {
                DongBatDauPanel();
            }
            return; // Đang hiện intro thì không làm gì khác
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
