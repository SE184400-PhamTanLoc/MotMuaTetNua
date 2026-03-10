using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
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
        hanhDongTuongTac = "cầu nguyện";
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
        if (DialogueManager.Instance == null)
        {
            HienThiPanelUocNguyen();
            return;
        }

        // Tạo danh sách câu thoại kể chuyện
        List<DialogueManager.DialogueNode> nodes = new List<DialogueManager.DialogueNode>();
        
        nodes.Add(new DialogueManager.DialogueNode {
            tenNguoiNoi = "Giếng Cổ Làng Ta",
            noiDung = "Tương truyền rằng, giếng nước này đã có từ buổi đầu lập làng. Mỗi độ Tết đến xuân về, linh khí trời đất hội tụ, mặt nước giếng bỗng trở nên xanh trong lạ kỳ.",
            nodeKeTiep = 1
        });

        nodes.Add(new DialogueManager.DialogueNode {
            tenNguoiNoi = "Giếng Cổ Làng Ta",
            noiDung = "Người dân làng thường tìm đến đây vào những ngày đầu năm để gửi gắm những ước nguyện chân thành nhất. Hễ ai thành tâm, ném một đồng xu xuống mặt nước linh thiêng, lời cầu nguyện sẽ theo dòng nước mà thấu tận trời xanh.",
            nodeKeTiep = 2
        });

        nodes.Add(new DialogueManager.DialogueNode {
            tenNguoiNoi = "Giếng Cổ Làng Ta",
            noiDung = "Con có muốn thử gửi gắm một điều ước cho năm mới bình an, vạn sự như ý không?",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice> {
                new DialogueManager.DialogueChoice {
                    noiDungLuaChon = "Con muốn cầu ước (Mất 10 xu)",
                    onChon = () => {
                        if (GameManager.Instance.CoĐuTien(giaTien1LanUoc)) {
                            HienThiPanelUocNguyen();
                        } else {
                            GameManager.Instance.OnThongBao?.Invoke("Cần ít nhất " + GameManager.FormatTien(giaTien1LanUoc) + " để ném xuống giếng!");
                            KetThucTuongTac();
                        }
                    }
                },
                new DialogueManager.DialogueChoice {
                    noiDungLuaChon = "Để lúc khác con quay lại",
                    onChon = () => KetThucTuongTac()
                }
            }
        });

        DialogueManager.Instance.BatDauHoiThoai(nodes);
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
