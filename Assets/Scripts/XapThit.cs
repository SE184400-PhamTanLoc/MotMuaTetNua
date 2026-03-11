using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// XapThit - Xạp bán thịt heo
/// </summary>
public class XapThit : NPCBase
{
    private int giaThit = 100;
    private bool _dangGian = false;

    protected override void Start()
    {
        base.Start();
        quayVePhiaPlayer = false;

        // Đảm bảo có BoxCollider và là Trigger
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }
        col.isTrigger = true;
        col.size = new Vector3(1.1f, 2f, 1.1f);
        col.center = new Vector3(0, 1f, 0);
    }

    protected override void OnTuongTac()
    {
        if (GameManager.Instance.coThit)
        {
            HienHoiThoaiDaMua();
            return;
        }

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
            tenNguoiNoi = "Chú Tư",
            noiDung = "Thịt heo nay tươi rói luôn con ơi! Mua về làm nhân bánh chưng là hết sảy.",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = $"Lấy con 1 ký thịt ({GameManager.FormatTien(giaThit)})",
                    onChon = () => HienHoiThoaiMuaThit()
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Để con đi vòng vòng xem đã chú.",
                    nodeKeTiep = -1
                }
            }
        });

        DialogueManager.Instance.BatDauHoiThoai(nodes, () => KetThucTuongTac());
    }

    private void HienHoiThoaiMuaThit()
    {
        var nodes = new List<DialogueManager.DialogueNode>();
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Chú Tư",
            noiDung = $"Thịt này ngon nhất chợ đó, chú để con {GameManager.FormatTien(giaThit)} thôi.",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Dạ con lấy luôn!",
                    onChon = () => GameManager.Instance.MuaNguyenLieu("thịt", giaThit)
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Trả giá khác...",
                    onChon = () => DialogueManager.Instance.HienNhapGiaTra(giaThit, (nhap) => XuLyTraGia(nhap), () => HienHoiThoaiMuaThit())
                },
                new DialogueManager.DialogueChoice
                {
                    noiDungLuaChon = "Thôi con chưa mua đâu.",
                    nodeKeTiep = -1
                }
            }
        });
        DialogueManager.Instance.BatDauHoiThoai(nodes, () => KetThucTuongTac());
    }

    private void XuLyTraGia(int giaDeNghi)
    {
        var nodes = new List<DialogueManager.DialogueNode>();
        if (giaDeNghi < 70)
        {
            _dangGian = true;
            nodes.Add(new DialogueManager.DialogueNode
            {
                tenNguoiNoi = "Chú Tư",
                noiDung = "Cái gì? Trả giá vậy sao bán được? Thôi biến đi cho chú bán hàng, bực mình thiệt chớ!",
                danhSachLuaChon = new List<DialogueManager.DialogueChoice>
                {
                    new DialogueManager.DialogueChoice { noiDungLuaChon = "Dạ con xin lỗi chú...", nodeKeTiep = -1 }
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
                    tenNguoiNoi = "Chú Tư",
                    noiDung = "Thôi được rồi, trông con cũng sáng sủa, chú bớt cho lấy hên đó. Cầm lấy thịt đi!",
                    onNodeShow = () => GameManager.Instance.MuaNguyenLieu("thịt", giaDeNghi),
                    danhSachLuaChon = new List<DialogueManager.DialogueChoice>
                    {
                        new DialogueManager.DialogueChoice { noiDungLuaChon = "Dạ con cảm ơn chú!", nodeKeTiep = -1 }
                    }
                });
            }
            else
            {
                nodes.Add(new DialogueManager.DialogueNode
                {
                    tenNguoiNoi = "Chú Tư",
                    noiDung = $"Giá đó cũng được, mà trong túi con hổng có đủ {GameManager.FormatTien(giaDeNghi)} kìa. Kiếm đủ tiền rồi quay lại chú để cho.",
                    danhSachLuaChon = new List<DialogueManager.DialogueChoice>
                    {
                        new DialogueManager.DialogueChoice { noiDungLuaChon = "Dạ để con xem lại túi tiền...", nodeKeTiep = -1 }
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
            tenNguoiNoi = "Chú Tư",
            noiDung = "Hôm nay chú không bán cho mấy đứa trả giá tào lao đâu nhé!",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice { 
                    noiDungLuaChon = "Dạ thôi mà chú, con lỡ dại, chú bán cho con đi.", 
                    onChon = () => { _dangGian = false; HienHoiThoaiChinh(); }
                },
                new DialogueManager.DialogueChoice { noiDungLuaChon = "Dạ...", nodeKeTiep = -1 }
            }
        });
        DialogueManager.Instance.BatDauHoiThoai(nodes, () => KetThucTuongTac());
    }

    private void HienHoiThoaiDaMua()
    {
        var nodes = new List<DialogueManager.DialogueNode>();
        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Chú Tư",
            noiDung = "Mua thịt rồi thì về gói bánh đi con, để lâu thịt nó giảm ngon đó!",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice { noiDungLuaChon = "Dạ con về liền!", nodeKeTiep = -1 }
            }
        });
        DialogueManager.Instance.BatDauHoiThoai(nodes, () => KetThucTuongTac());
    }
}
