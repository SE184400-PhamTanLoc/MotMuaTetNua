using UnityEngine;
using System.Collections.Generic;

public class NguoiBoInteract : NPCBase
{
    protected override void Start()
    {
        base.Start();
        tenNPC = "Bố";
        hanhDongTuongTac = "chúc Tết";
    }

    protected override void OnTuongTac()
    {
        if (GameManager.Instance.currentDay != GameManager.TetDay.Mung1)
        {
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Con chuẩn bị Tết giúp mẹ xong chưa?" }
            });
            KetThucTuongTac();
            return;
        }

        if (!Mung1Manager.Instance.hasGreetedBo)
        {
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Con chúc Bố năm mới dồi dào sức khỏe, vạn sự như ý ạ!" },
                new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Giỏi lắm, bố chúc con năm mới học hành tiến tới, ngoan ngoãn nhé." }
            }, () => {
                Mung1Manager.Instance.Greet("Bố");
                KetThucTuongTac();
            });
        }
        else
        {
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Đi chúc Tết Ông Nội chưa con?" }
            });
            KetThucTuongTac();
        }
    }
}
