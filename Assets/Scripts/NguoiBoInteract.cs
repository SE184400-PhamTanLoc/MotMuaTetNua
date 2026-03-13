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

        // Ngày Mùng 1: Chúc chung tại chỗ Ông Nội (vì hai người luôn đứng cạnh nhau)
        DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
            new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Con sang chúc Tết Ông Nội kìa!" }
        });
        KetThucTuongTac();
    }
}
