using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// XapGao - Xạp bán gạo nếp, đậu xanh và lá chuối
/// </summary>
public class XapGao : NPCBase
{
    private int giaGao = 50;
    private int giaDau = 30;
    private int giaLa = 20;

    private bool _dangGian = false;

    protected override void Start()
    {
        base.Start();
        quayVePhiaPlayer = false; // Không xoay xạp hàng

        // Đảm bảo có BoxCollider và là Trigger
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }
        col.isTrigger = true; // Để player không bị chặn
        col.size = new Vector3(1.1f, 2f, 1.1f); // Kích thước gọn hơn
        col.center = new Vector3(0, 1f, 0);

        // Đảm bảo layer đúng để PlayerInteraction tìm thấy
        // Giả sử layer NPC là 7 hoặc dùng tên (thường trong project này là mặc định hoặc NPC)
        // Nếu không biết chính xác, ta để PlayerInteraction tìm theo component NPCBase (nó đã làm vậy)
    }

    protected override void OnTuongTac()
    {
        if (_dangGian)
        {
            HienHoiThoaiDangGian();
            return;
        }

        HienHoiThoaiChinh();
    }

    private void HienHoiThoaiChinh()
    {
        var nodes = new List<DialogueManager.DialogueNode>();

        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Bà Sáu",
            noiDung = "Chào con, con mua gì cho mẹ đó? Nay bà có nếp mới thơm lắm, đậu xanh cũng bùi, lá chuối thì xanh mướt luôn!",
            danhSachLuaChon = GetChoicesHienTai()
        });

        DialogueManager.Instance.BatDauHoiThoai(nodes, () => KetThucTuongTac());
    }

    private List<DialogueManager.DialogueChoice> GetChoicesHienTai()
    {
        var choices = new List<DialogueManager.DialogueChoice>();

        if (!GameManager.Instance.coGaoNep)
        {
            choices.Add(new DialogueManager.DialogueChoice
            {
                noiDungLuaChon = $"Mua Gạo nếp ({GameManager.FormatTien(giaGao)})",
                onChon = () => HienHoiThoaiMuaItem("Gạo nếp", giaGao)
            });
        }

        if (!GameManager.Instance.coDau)
        {
            choices.Add(new DialogueManager.DialogueChoice
            {
                noiDungLuaChon = $"Mua Đậu xanh ({GameManager.FormatTien(giaDau)})",
                onChon = () => HienHoiThoaiMuaItem("Đậu xanh", giaDau)
            });
        }

        if (!GameManager.Instance.coLaChuoi)
        {
            choices.Add(new DialogueManager.DialogueChoice
            {
                noiDungLuaChon = $"Mua Lá chuối ({GameManager.FormatTien(giaLa)})",
                onChon = () => HienHoiThoaiMuaItem("Lá chuối", giaLa)
            });
        }

        choices.Add(new DialogueManager.DialogueChoice
        {
            noiDungLuaChon = "Thôi con xem tí đã.",
            nodeKeTiep = -1
        });

        return choices;
    }

    private void HienHoiThoaiMuaItem(string tenItem, int giaGoc)
    {
        var nodes = new List<DialogueManager.DialogueNode>();

        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Bà Sáu",
            noiDung = $"{tenItem} hả con? Cái này bà bán {GameManager.FormatTien(giaGoc)} một bó/ký nha.",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = $"Dạ con lấy luôn ({GameManager.FormatTien(giaGoc)})",
                    onChon = () => GameManager.Instance.MuaNguyenLieu(tenItem, giaGoc)
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Con muốn trả giá khác...",
                    onChon = () => DialogueManager.Instance.HienNhapGiaTra(giaGoc, (nhap) => XuLyTraGia(tenItem, giaGoc, nhap), () => HienHoiThoaiMuaItem(tenItem, giaGoc))
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Để con xem lại sau.",
                    nodeKeTiep = -1
                }
            }
        });

        DialogueManager.Instance.BatDauHoiThoai(nodes, () => KetThucTuongTac());
    }

    private void XuLyTraGia(string tenItem, int giaGoc, int giaDeNghi)
    {
        var nodes = new List<DialogueManager.DialogueNode>();
        float tiLe = (float)giaDeNghi / giaGoc;

        if (tiLe < 0.6f)
        {
            // Trả giá quá lố
            _dangGian = true;
            nodes.Add(new DialogueManager.DialogueNode
            {
                tenNguoiNoi = "Bà Sáu",
                noiDung = "Trời ơi con ơi! Trả vậy thì bà bán lỗ vốn à? Thôi đi chỗ khác mua giùm cái đi, bà không bán cho con nữa đâu!",
                danhSachLuaChon = new List<DialogueManager.DialogueChoice>
                {
                    new DialogueManager.DialogueChoice { noiDungLuaChon = "Dạ con xin lỗi...", nodeKeTiep = -1 }
                }
            });
        }
        else
        {
            // Kiểm tra tiền trước khi chấp nhận
            if (GameManager.Instance.CoĐuTien(giaDeNghi))
            {
                nodes.Add(new DialogueManager.DialogueNode
                {
                    tenNguoiNoi = "Bà Sáu",
                    noiDung = tiLe < 0.9f ? "Hừm... thôi được rồi, Tết nhất bà bớt cho con lấy thảo đó. Nhớ về gói bánh cho ngon nha!" 
                                         : "Dễ thương chưa kìa! Bà bán cho con luôn đó. Cảm ơn con nha!",
                    onNodeShow = () => GameManager.Instance.MuaNguyenLieu(tenItem, giaDeNghi),
                    danhSachLuaChon = new List<DialogueManager.DialogueChoice>
                    {
                        new DialogueManager.DialogueChoice { noiDungLuaChon = "Dạ con cảm ơn bà!", nodeKeTiep = -1 }
                    }
                });
            }
            else
            {
                nodes.Add(new DialogueManager.DialogueNode
                {
                    tenNguoiNoi = "Bà Sáu",
                    noiDung = $"Bà cũng muốn bớt cho con lắm, mà con coi lại trong túi thử... có {GameManager.FormatTien(giaDeNghi)} đâu mà mua?",
                    danhSachLuaChon = new List<DialogueManager.DialogueChoice>
                    {
                        new DialogueManager.DialogueChoice { noiDungLuaChon = "Dạ để con kiếm thêm tiền...", nodeKeTiep = -1 }
                    }
                });
            }
        }

        DialogueManager.Instance.BatDauHoiThoai(nodes, () => KetThucTuongTac());
    }

    private void HienHoiThoaiDangGian()
    {
        var nodes = new List<DialogueManager.DialogueNode>();
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Bà Sáu",
            noiDung = "Bà còn đang giận đó! Đi chỗ khác chơi đi!",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice { 
                    noiDungLuaChon = "Năn nỉ bà mà, bán cho con đi...", 
                    onChon = () => { _dangGian = false; HienHoiThoaiChinh(); }
                },
                new DialogueManager.DialogueChoice { noiDungLuaChon = "Dạ con đi ngay.", nodeKeTiep = -1 }
            }
        });
        DialogueManager.Instance.BatDauHoiThoai(nodes, () => KetThucTuongTac());
    }
}
