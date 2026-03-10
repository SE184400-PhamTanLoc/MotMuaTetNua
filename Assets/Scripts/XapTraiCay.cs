using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// XapTraiCay - Quản lý sạp trái cây và mở trò chơi Hứng quả
/// </summary>
public class XapTraiCay : NPCBase
{
    protected override void Start()
    {
        base.Start();
        quayVePhiaPlayer = false;

        if (GetComponent<Collider>() == null)
        {
            var col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3f, 2f, 3f);
            col.center = new Vector3(0, 1f, 0);
        }
    }

    protected override void OnTuongTac()
    {
        var nodes = new List<DialogueManager.DialogueNode>();

        nodes.Add(new DialogueManager.DialogueNode
        {
            tenNguoiNoi = "Chị Lan",
            noiDung = "Chào em trai! Lựa trái cây chưng mâm Ngũ Quả hả? Chị mới dọn sạp xong, em chơi trò 'Hứng quả' này đi, nhặt đủ 4 trái khác nhau chị tính giá rẻ cho nhen!",
            danhSachLuaChon = new List<DialogueManager.DialogueChoice>
            {
                new DialogueManager.DialogueChoice { 
                    noiDungLuaChon = "Dạ để em thử ạ.", 
                    onChon = () => {
                        AutoSetup.StartFruitMinigame();
                    }
                },
                new DialogueManager.DialogueChoice { 
                    noiDungLuaChon = "Để em xem thêm một chút.", 
                    nodeKeTiep = -1 
                }
            }
        });

        DialogueManager.Instance.BatDauHoiThoai(nodes, () => KetThucTuongTac());
    }
}
