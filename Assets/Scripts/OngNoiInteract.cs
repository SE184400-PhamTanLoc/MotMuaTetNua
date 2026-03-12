using UnityEngine;
using System.Collections.Generic;

public class OngNoiInteract : NPCBase
{
    protected override void Start()
    {
        base.Start();
        tenNPC = "Ông Nội";
        hanhDongTuongTac = "tương tác";

        // Ngày 30: Ông Nội chỉ xuất hiện thông qua minigame canh nồi bánh (dialogue kể chuyện trong UI minigame),
        // nên tạm tắt tương tác trực tiếp bằng phím E để không bị trùng với trigger nồi bánh.
        if (GameManager.Instance != null && GameManager.Instance.currentDay == GameManager.TetDay.Day30)
        {
            coTheTuongTac = false;
        }
    }

    protected override void OnTuongTac()
    {
        if (GameManager.Instance.currentDay == GameManager.TetDay.Day29)
        {
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Nhà cửa sạch sẽ thì năm mới mới may mắn con ạ." }
            });
            KetThucTuongTac();
            return;
        }

        if (GameManager.Instance.currentDay != GameManager.TetDay.Mung1)
        {
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Tết đến nơi rồi, con cháu quây quần là ông vui nhất." }
            });
            KetThucTuongTac();
            return;
        }

        if (!Mung1Manager.Instance.hasGreetedOngNoi)
        {
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Con kính chúc Ông Nội năm mới sống lâu trăm tuổi, bình an vô sự ạ!" },
                new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Ôi, cháu ông ngoan quá." }
            }, () => {
                Mung1Manager.Instance.Greet("Ông Nội");
                // Sau khi chúc xong, kiểm tra xem có nhận được lì xì luôn không
                if (GameManager.Instance.daChucTet && !GameManager.Instance.daNhanLiXi)
                {
                    Mung1Manager.Instance.ReceiveLuckyMoney();
                }
                else if (!GameManager.Instance.daChucTet)
                {
                    DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Mà con đã chúc Tết Bố Mẹ chưa? Đi chúc Bố Mẹ trước đi rồi ông lì xì cho nhé!" }
                    });
                }
                KetThucTuongTac();
            });
        }
        else if (GameManager.Instance.daChucTet && !GameManager.Instance.daNhanLiXi)
        {
            // Trường hợp đã chúc Ông rồi nhưng chưa nhận lì xì (do lúc đó chưa chúc Bố Mẹ)
            Mung1Manager.Instance.ReceiveLuckyMoney();
            KetThucTuongTac();
        }
        else if (!GameManager.Instance.daChucTet)
        {
            // Đã chúc Ông rồi nhưng vẫn chưa chúc Bố Mẹ
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Nhớ đi chúc Tết Bố Mẹ trước đã rồi quay lại đây nhận lì xì của ông nhé!" }
            });
            KetThucTuongTac();
        }
        else if (!GameManager.Instance.daChupAnhGiaDinh)
        {
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Nào, cả nhà mình chụp một tấm ảnh kỷ niệm đi!" }
            }, () => {
                Mung1Manager.Instance.StartPhotoMinigame();
                KetThucTuongTac();
            });
        }
        else
        {
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Năm mới vạn sự tốt lành nhé cháu." }
            });
            KetThucTuongTac();
        }
    }
}
