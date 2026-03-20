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
            tenNguoiNoi = "Cô gái bán mai",
            noiDung = "Chào anh ơi! Tết đến xuân về rồi!\n\nGhé mua cành mai vàng về chưng cho đẹp nhà không anh? Em có mai vàng Bến Tre xịn lắm nè, nở đúng mùng Một luôn á!",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Cho tôi xem mai đi!",
                    nodeKeTiep = 1
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Mai ở đây đẹp lắm ha, giá cả sao cô?",
                    nodeKeTiep = 3
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Thôi, cảm ơn cô. Tôi đi xem thêm đã.",
                    nodeKeTiep = 8 // Chuyển sang NODE 8 (Từ chối ngay)
                }
            }
        });

        // ===== NODE 1: Giới thiệu mai (Phần 1) =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Cô gái bán mai",
            noiDung = "Dạ, anh coi nè!\n\nEm có hai loại mai rất đẹp:\n\n" +
                      "Cây mai nhỏ xinh xắn — Nụ nhiều, hoa vàng rực rỡ, phù hợp để bàn hoặc bàn thờ. " +
                      $"Giá chỉ <color=#FFD700><b>{GameManager.FormatTien(giaMaiNho)}</b></color> thôi á!",
            nodeKeTiep = 2 // Chuyển sang NODE 2 (Giới thiệu P2)
        });

        // ===== NODE 2: Giới thiệu mai (Phần 2 + Lựa chọn) =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Cô gái bán mai",
            noiDung = "Cây mai lớn tuyệt đẹp — Dáng cổ thụ, tán rộng, hoa nở rộ, chưng phòng khách sang trọng lắm! " +
                      $"Giá <color=#FFD700><b>{GameManager.FormatTien(giaMaiLon)}</b></color> nha anh!\n\nAnh thấy sao?",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = $"Mua cây mai nhỏ ({GameManager.FormatTien(giaMaiNho)})",
                    onChon = () => XuLyMuaMai("Cây mai nhỏ", giaMaiNho)
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = $"Mua cây mai lớn ({GameManager.FormatTien(giaMaiLon)})",
                    onChon = () => XuLyMuaMai("Cây mai lớn", giaMaiLon)
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Để tôi suy nghĩ thêm đã...",
                    nodeKeTiep = 7 // Chuyển sang NODE 7 (Suy nghĩ thêm)
                }
            }
        });

        // ===== NODE 3: Hỏi giá trước =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Cô gái bán mai",
            noiDung = "Dạ, năm nay em bán giá cả phải chăng lắm anh ơi!\n\n" +
                      "Mai nhỏ: " + GameManager.FormatTien(giaMaiNho) + " — Xinh xắn, dễ thương!\n" +
                      "Mai lớn: " + GameManager.FormatTien(giaMaiLon) + " — Sang trọng, quý phái!",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Lấy cây mai nhỏ nha cô (" + GameManager.FormatTien(giaMaiNho) + ")",
                    onChon = () => XuLyMuaMai("Cây mai nhỏ", giaMaiNho)
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Cho tôi cây mai lớn luôn! (" + GameManager.FormatTien(giaMaiLon) + ")",
                    onChon = () => XuLyMuaMai("Cây mai lớn", giaMaiLon)
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Để tôi coi ví còn bao nhiêu đã...",
                    nodeKeTiep = 9 // Chuyển sang NODE 9 (Xem ví)
                }
            }
        });

        // ===== NODE 4: Kết quả mua (Placeholder) =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Cô gái bán mai",
            noiDung = "", 
            nodeKeTiep = -1
        });

        // ===== NODE 5: Không đủ tiền =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Cô gái bán mai",
            noiDung = "Dạ... anh ơi, hình như tiền không đủ rồi ạ\n\n" +
                      "Không sao đâu anh! Anh đi dạo chợ Tết kiếm thêm tiền rồi quay lại nha. " +
                      "Em sẽ giữ cây mai đẹp nhất cho anh!\n\n" +
                      "<i>(Mẹ anh sẽ vui lắm khi có cây mai đẹp chưng Tết đó!)</i>",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Ừ, tôi sẽ quay lại nha!",
                    nodeKeTiep = -1
                }
            }
        });

        // ===== NODE 6: Từ chối nhẹ nhàng (Quay lại sau) =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Cô gái bán mai",
            noiDung = "Dạ không sao anh ơi!\n\nAnh cứ đi dạo chợ Tết cho vui, khi nào ưng thì quay lại nha. " +
                      "Em bán tới chiều 30 Tết luôn á!\n\n" +
                      "Nhớ mua mai sớm kẻo hết cây đẹp nha anh!",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Cảm ơn cô, tôi sẽ quay lại!",
                    nodeKeTiep = -1
                }
            }
        });

        // ===== NODE 7: Suy nghĩ thêm =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Cô gái bán mai",
            noiDung = "Dạ anh cứ thong thả suy nghĩ thêm nha!\n\nKhi nào quyết định được thì quay lại đây với em, " +
                      "em luôn để dành cây mai tươi nhất cho anh đó!",
            nodeKeTiep = -1
        });

        // ===== NODE 8: Từ chối ngay (Đi dạo vui vẻ) =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Cô gái bán mai",
            noiDung = "Dạ, không sao ạ! Anh đi dạo vui nha!\n\nKhi nào muốn mua mai thì ghé lại chỗ em, " +
                      "em để dành cây đẹp cho anh! Chúc anh mua sắm Tết vui vẻ!",
            nodeKeTiep = -1
        });

        // ===== NODE 9: Xem ví =====
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Cô gái bán mai",
            noiDung = "Dạ anh cứ xem đi ạ! Hiện tại túi tiền của anh đang có <color=#00FF00><b>" + 
                      GameManager.FormatTien(GameManager.Instance.SoTien) + "</b></color> đó anh!\n\n" +
                      "Anh cứ thong thả chọn nha, em lúc nào cũng sẵn sàng phục vụ ạ!",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Cảm ơn cô, để tôi xem tiếp.",
                    nodeKeTiep = 2 // Quay lại NODE 2 (Giới thiệu P2)
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Ừ, để tôi xem thêm chỗ khác nữa.",
                    nodeKeTiep = 8 // Chuyển sang NODE 8 (Từ chối ngay)
                }
            }
        });

        // Bắt đầu hội thoại
        DialogueManager.Instance.BatDauHoiThoai(nodes, () => KetThucTuongTac());
    }

    private void HienThongBaoKetQuaMua(bool thanhCong, string loaiMai, int giaTien)
    {
        var nodes = new List<DialogueManager.DialogueNode>();
        
        if (thanhCong)
        {
            nodes.Add(new DialogueManager.DialogueNode
            {
                tenNguoiNoi = "Cô gái bán mai",
                noiDung = $"Tuyệt vời!\n\nDạ cảm ơn anh nhiều nha! Anh chọn <b>{loaiMai}</b> đúng rồi đó!\n\n" +
                          $"<color=#FFD700>-{GameManager.FormatTien(giaTien)}</color>\n" +
                          $"Tiền còn lại: <color=#00FF00>{GameManager.FormatTien(GameManager.Instance.SoTien)}</color>\n\n" +
                          "Mai đẹp lắm, mẹ anh chắc chắn sẽ vui mừng lắm luôn á!\n" +
                          "Chúc anh và gia đình năm mới AN KHANG THỊNH VƯỢNG, VẠN SỰ NHƯ Ý!",
                nodeKeTiep = -1
            });
        }
        else
        {
            int thieu = giaTien - GameManager.Instance.SoTien;
            nodes.Add(new DialogueManager.DialogueNode
            {
                tenNguoiNoi = "Cô gái bán mai",
                noiDung = $"Dạ... anh ơi\n\n" +
                          $"<b>{loaiMai}</b> giá <color=#FFD700>{GameManager.FormatTien(giaTien)}</color>, " +
                          $"mà anh chỉ còn <color=#FF6B6B>{GameManager.FormatTien(GameManager.Instance.SoTien)}</color> thôi.\n" +
                          $"Còn thiếu <color=#FF0000>{GameManager.FormatTien(thieu)}</color> nữa ạ!\n\n" +
                          "Không sao đâu anh! Anh đi dạo chợ kiếm thêm rồi quay lại nha. " +
                          "Em giữ cây đẹp cho anh!",
                nodeKeTiep = -1
            });
        }

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
            tenNguoiNoi = "Cô gái bán mai",
            noiDung = $"Ơ anh quay lại rồi!\n\nAnh mua <b>{loaiMai}</b> rồi mà! Mai đẹp lắm đó, " +
                      "mang về cho mẹ chưng đi anh, kẻo trễ!\n\n" +
                      "Chúc anh và gia đình năm mới an khang thịnh vượng, " +
                      "vạn sự như ý nha!",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Cảm ơn cô, Tết vui vẻ nha!",
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
        bool thanhCong = false;
        if (GameManager.Instance.CoĐuTien(giaTien))
        {
            thanhCong = GameManager.Instance.MuaMai(loaiMai, giaTien);
            if (thanhCong)
            {
                PhatAmThanh(tiengThanhCong);
                if (hieuUngMuaThanhCong != null)
                    hieuUngMuaThanhCong.Play();
            }
        }
        
        HienThongBaoKetQuaMua(thanhCong, loaiMai, giaTien);
    }
}
