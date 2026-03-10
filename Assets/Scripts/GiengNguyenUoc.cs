using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

/// <summary>
/// Quản lý chức năng Giếng Nguyện Ước
/// </summary>
public class GiengNguyenUoc : NPCBase
{
    [Header("=== TRẠNG THÁI ===")]
    public int giaTien1LanUoc = 10;
    
    // UI Elements
    [HideInInspector] public GameObject panelUocNguyen;
    [HideInInspector] public TMP_InputField inputDieuUoc;
    [HideInInspector] public Button btnGuiDieuUoc;
    [HideInInspector] public Button btnDongPanel;

    private void Awake()
    {
        tenNPC = "Giếng Nguyện Ước";
        quayVePhiaPlayer = false; // Giếng thì không tự quay đầu
    }

    protected override void Start()
    {
        base.Start();
        
        // Gắn sự kiện cho nút bấm
        if (btnGuiDieuUoc != null)
        {
            btnGuiDieuUoc.onClick.AddListener(ThucHienUocNguyen);
        }

        if (btnDongPanel != null)
        {
            btnDongPanel.onClick.AddListener(DongPanel);
        }
    }

    protected override void OnTuongTac()
    {
        if (GameManager.Instance == null) return;

        // Nếu panel đang mở thì bỏ qua
        if (panelUocNguyen != null && panelUocNguyen.activeSelf) return;

        // Kiểm tra tiền
        if (!GameManager.Instance.CoĐuTien(giaTien1LanUoc))
        {
            GameManager.Instance.OnThongBao?.Invoke($"Cần ít nhất {GameManager.FormatTien(giaTien1LanUoc)} để ném xuống giếng!");
            KetThucTuongTac();
            return;
        }

        HienThiPanelUocNguyen();
    }

    private void HienThiPanelUocNguyen()
    {
        if (panelUocNguyen == null) return;

        panelUocNguyen.SetActive(true);
        inputDieuUoc.text = ""; // Xóa text cũ
    }

    public void DongPanel()
    {
        if (panelUocNguyen != null)
        {
            panelUocNguyen.SetActive(false);
        }

        KetThucTuongTac();
    }

    private void ThucHienUocNguyen()
    {
        string noiDung = inputDieuUoc.text.Trim();
        if (string.IsNullOrEmpty(noiDung))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnThongBao?.Invoke("Hãy thành tâm viết một điều ước nhé!");
            return;
        }

        // Trừ tiền
        if (GameManager.Instance != null && GameManager.Instance.TruTien(giaTien1LanUoc))
        {
            GameManager.Instance.OnThongBao?.Invoke($"Đã ném xu, nguyện vọng \"{noiDung}\" đã được giếng tinh chứng giám!");
        }

        DongPanel();
    }

    protected override void Update()
    {
        base.Update();

        // Nếu panel đang mở, cho phép bấm ESC để đóng
        if (panelUocNguyen != null && panelUocNguyen.activeSelf)
        {
            bool escPressed = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) || 
                              Input.GetKeyDown(KeyCode.Escape);

            if (escPressed)
            {
                DongPanel();
            }
        }
    }
}
