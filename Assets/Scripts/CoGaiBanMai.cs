using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// CoGaiBanMai - NPC Cô gái bán mai
/// Chứa toàn bộ logic hội thoại mua mai
/// Gắn script này vào model co_gai_ban_mai trong Scene
/// </summary>
public class CoGaiBanMai : NPCBase
{
    [Header("=== CÀI ĐẶT GIÁ MAI ===")]
    [Tooltip("Giá cây mai nhỏ (đơn vị nghìn đồng)")]
    public int giaMaiNho = 200;

    [Tooltip("Giá cây mai lớn (đơn vị nghìn đồng)")]
    public int giaMaiLon = 500;

    [Header("=== HIỆU ỨNG ===")]
    [Tooltip("Particle effect khi mua thành công")]
    public ParticleSystem hieuUngMuaThanhCong;

    [Tooltip("AudioSource cho giọng nói/tiếng")]
    public AudioSource audioSource;

    [Tooltip("Âm thanh chào hỏi")]
    public AudioClip tiengChao;

    [Tooltip("Âm thanh mua thành công")]
    public AudioClip tiengThanhCong;

    protected override void Start()
    {
        base.Start();
        tenNPC = "Cô gái bán mai";
        quayVePhiaPlayer = false; // Không quay về phía player khi tương tác
    }

    // =========================================
    // PHÁT ÂM THANH
    // =========================================
    
    /// <summary>
    /// Phát âm thanh nếu có AudioSource và AudioClip
    /// </summary>
    private void PhatAmThanh(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // =========================================
    // XỬ LÝ TƯƠNG TÁC
    // =========================================
    
    protected override void OnTuongTac()
    {
        // Phát âm thanh chào
        PhatAmThanh(tiengChao);

        // Kiểm tra đã mua mai chưa
        if (GameManager.Instance.daMuaMai)
        {
            HienHoiThoaiDaMuaRoi();
        }
        else
        {
            HienHoiThoaiChinh();
        }
    }

    // =========================================
    // CÁC NHÁNH HỘI THOẠI
    // =========================================

    /// <summary>
    /// Hội thoại chính - Lần đầu gặp
    /// </summary>
    private void HienHoiThoaiChinh()
    {
        var nodes = new List<DialogueManager.DialogueNode>();

        // ===== NODE 0: Chào hỏi =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "🌼 Cô gái bán mai",
            noiDung = "Chào anh ơi! Tết đến xuân về rồi! 🌸\n\nGhé mua cành mai vàng về chưng cho đẹp nhà không anh? Em có mai vàng Bến Tre xịn lắm nè, nở đúng mùng Một luôn á!",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "🌼 Cho tôi xem mai đi!",
                    nodeKeTiep = 1
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "😊 Mai ở đây đẹp lắm ha, giá cả sao cô?",
                    nodeKeTiep = 2
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "❌ Thôi, cảm ơn cô. Tôi đi xem thêm đã.",
                    nodeKeTiep = 6
                }
            }
        });

        // ===== NODE 1: Giới thiệu mai =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "🌼 Cô gái bán mai",
            noiDung = "Dạ, anh coi nè! 🌼\n\nEm có hai loại mai rất đẹp:\n\n" +
                      "🌱 <b>Cây mai nhỏ xinh xắn</b> — Nụ nhiều, hoa vàng rực rỡ, phù hợp để bàn hoặc bàn thờ. " +
                      $"Giá chỉ <color=#FFD700><b>{GameManager.FormatTien(giaMaiNho)}</b></color> thôi á!\n\n" +
                      "🌳 <b>Cây mai lớn tuyệt đẹp</b> — Dáng cổ thụ, tán rộng, hoa nở rộ, chưng phòng khách sang trọng lắm! " +
                      $"Giá <color=#FFD700><b>{GameManager.FormatTien(giaMaiLon)}</b></color> nha anh!",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = $"🌱 Mua cây mai nhỏ ({GameManager.FormatTien(giaMaiNho)})",
                    nodeKeTiep = 3,
                    onChon = () => XuLyMuaMai("Cây mai nhỏ", giaMaiNho)
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = $"🌳 Mua cây mai lớn ({GameManager.FormatTien(giaMaiLon)})",
                    nodeKeTiep = 3,
                    onChon = () => XuLyMuaMai("Cây mai lớn", giaMaiLon)
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "🤔 Để tôi suy nghĩ thêm đã...",
                    nodeKeTiep = 5
                }
            }
        });

        // ===== NODE 2: Hỏi giá trước =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "🌼 Cô gái bán mai",
            noiDung = "Dạ, năm nay em bán giá cả phải chăng lắm anh ơi! 😄\n\n" +
                      $"🌱 <b>Mai nhỏ</b>: {GameManager.FormatTien(giaMaiNho)} — Xinh xắn, dễ thương!\n" +
                      $"🌳 <b>Mai lớn</b>: {GameManager.FormatTien(giaMaiLon)} — Sang trọng, quý phái!\n\n" +
                      "Mai em toàn mai Bến Tre chính gốc, nụ nhiều hoa đẹp. Mẹ anh chắc chắn thích lắm á! 🧧",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = $"🌱 Lấy cây mai nhỏ nha cô ({GameManager.FormatTien(giaMaiNho)})",
                    nodeKeTiep = 3,
                    onChon = () => XuLyMuaMai("Cây mai nhỏ", giaMaiNho)
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = $"🌳 Cho tôi cây mai lớn luôn! ({GameManager.FormatTien(giaMaiLon)})",
                    nodeKeTiep = 3,
                    onChon = () => XuLyMuaMai("Cây mai lớn", giaMaiLon)
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "💸 Để tôi coi ví còn bao nhiêu đã...",
                    nodeKeTiep = 5
                }
            }
        });

        // ===== NODE 3: Kết quả mua (sẽ redirect trong XuLyMuaMai) =====
        // Placeholder - sẽ được thay bằng node thành công hoặc thất bại
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "🌼 Cô gái bán mai",
            noiDung = "", // Sẽ được cập nhật trong XuLyMuaMai
            nodeKeTiep = -1
        });

        // ===== NODE 4: Không đủ tiền =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "🌼 Cô gái bán mai",
            noiDung = "Dạ... anh ơi, hình như tiền không đủ rồi ạ 😅\n\n" +
                      "Không sao đâu anh! Anh đi dạo chợ Tết kiếm thêm tiền rồi quay lại nha. " +
                      "Em sẽ giữ cây mai đẹp nhất cho anh! 🌼\n\n" +
                      "<i>(Mẹ anh sẽ vui lắm khi có cây mai đẹp chưng Tết đó!)</i>",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "😊 Ừ, tôi sẽ quay lại nha!",
                    nodeKeTiep = -1
                }
            }
        });

        // ===== NODE 5: Từ chối nhẹ nhàng =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "🌼 Cô gái bán mai",
            noiDung = "Dạ không sao anh ơi! 😊\n\nAnh cứ đi dạo chợ Tết cho vui, khi nào ưng thì quay lại nha. " +
                      "Em bán tới chiều 30 Tết luôn á!\n\n" +
                      "Nhớ mua mai sớm kẻo hết cây đẹp nha anh! 🌼",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "👋 Cảm ơn cô, tôi sẽ quay lại!",
                    nodeKeTiep = -1
                }
            }
        });

        // ===== NODE 6: Từ chối ngay =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "🌼 Cô gái bán mai",
            noiDung = "Dạ, không sao ạ! 😊 Anh đi dạo vui nha!\n\nKhi nào muốn mua mai thì ghé lại chỗ em, " +
                      "em để dành cây đẹp cho anh! Chúc anh mua sắm Tết vui vẻ! 🧧",
            nodeKeTiep = -1
        });

        // Bắt đầu hội thoại
        DialogueManager.Instance.BatDauHoiThoai(nodes, () => KetThucTuongTac());
    }

    /// <summary>
    /// Hội thoại khi đã mua mai rồi
    /// </summary>
    private void HienHoiThoaiDaMuaRoi()
    {
        var nodes = new List<DialogueManager.DialogueNode>();

        string loaiMai = GameManager.Instance.LoaiMaiDaMua;

        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "🌼 Cô gái bán mai",
            noiDung = $"Ơ anh quay lại rồi! 😄\n\nAnh mua <b>{loaiMai}</b> rồi mà! Mai đẹp lắm đó, " +
                      "mang về cho mẹ chưng đi anh, kẻo trễ!\n\n" +
                      "🌼 Chúc anh và gia đình năm mới an khang thịnh vượng, " +
                      "vạn sự như ý nha! 🧧🎉",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "😊 Cảm ơn cô, Tết vui vẻ nha!",
                    nodeKeTiep = -1
                }
            }
        });

        DialogueManager.Instance.BatDauHoiThoai(nodes, () => KetThucTuongTac());
    }

    // =========================================
    // XỬ LÝ MUA MAI
    // =========================================
    
    private bool _muaThanhCong = false;

    /// <summary>
    /// Logic mua mai - Gọi khi player chọn mua
    /// </summary>
    private void XuLyMuaMai(string loaiMai, int giaTien)
    {
        if (GameManager.Instance.CoĐuTien(giaTien))
        {
            // Mua thành công!
            _muaThanhCong = GameManager.Instance.MuaMai(loaiMai, giaTien);

            if (_muaThanhCong)
            {
                // Cập nhật node 3 thành thông báo thành công
                CapNhatNodeThanhCong(loaiMai, giaTien);

                // Hiệu ứng
                PhatAmThanh(tiengThanhCong);
                if (hieuUngMuaThanhCong != null)
                    hieuUngMuaThanhCong.Play();
            }
        }
        else
        {
            _muaThanhCong = false;
            // Không đủ tiền → redirect sang node 4
            // Trick: thay đổi nodeKeTiep trong luaChon thành 4
            // Nhưng vì nodeKeTiep đã được set, ta cần cập nhật node 3
            CapNhatNodeKhongDuTien(loaiMai, giaTien);
        }
    }

    private void CapNhatNodeThanhCong(string loaiMai, int giaTien)
    {
        // Node 3 sẽ hiển thị khi mua thành công
        // Do hệ thống đã navigate tới node 3, ta cần hook vào onNodeShow
        // Tuy nhiên vì text được set trước khi BatDauHoiThoai,
        // ta cần sử dụng cách khác: đặt text trực tiếp

        // Workaround: Dùng Invoke delay nhỏ để cập nhật text
        StartCoroutine(CapNhatNodeSauDelay(
            $"Tuyệt vời! 🎉🌼\n\nDạ cảm ơn anh nhiều nha! Anh chọn <b>{loaiMai}</b> đúng rồi đó!\n\n" +
            $"<color=#FFD700>-{GameManager.FormatTien(giaTien)}</color>\n" +
            $"💰 Tiền còn lại: <color=#00FF00>{GameManager.FormatTien(GameManager.Instance.SoTien)}</color>\n\n" +
            "Mai đẹp lắm, mẹ anh chắc chắn sẽ vui mừng lắm luôn á! 🧧\n" +
            "Chúc anh và gia đình năm mới AN KHANG THỊNH VƯỢNG, VẠN SỰ NHƯ Ý! 🎊"
        ));
    }

    private void CapNhatNodeKhongDuTien(string loaiMai, int giaTien)
    {
        int thieu = giaTien - GameManager.Instance.SoTien;
        StartCoroutine(CapNhatNodeSauDelay(
            $"Dạ... anh ơi 😅\n\n" +
            $"<b>{loaiMai}</b> giá <color=#FFD700>{GameManager.FormatTien(giaTien)}</color>, " +
            $"mà anh chỉ còn <color=#FF6B6B>{GameManager.FormatTien(GameManager.Instance.SoTien)}</color> thôi.\n" +
            $"Còn thiếu <color=#FF0000>{GameManager.FormatTien(thieu)}</color> nữa ạ!\n\n" +
            "Không sao đâu anh! Anh đi dạo chợ kiếm thêm rồi quay lại nha. " +
            "Em giữ cây đẹp cho anh! 🌼"
        ));
    }

    private System.Collections.IEnumerator CapNhatNodeSauDelay(string noiDungMoi)
    {
        yield return null; // Chờ 1 frame

        // Cập nhật text hiển thị trực tiếp trên UI
        if (DialogueManager.Instance != null)
        {
            var noiDungText = DialogueManager.Instance.noiDungText;
            if (noiDungText != null)
            {
                noiDungText.text = noiDungMoi;
            }
        }
    }
}
