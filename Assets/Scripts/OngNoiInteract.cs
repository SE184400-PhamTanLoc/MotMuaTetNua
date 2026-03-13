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

        // === MÙNG 1 ===
        var gm = GameManager.Instance;
        // Reload lại các flag từ Mung1Manager cho chắc chắn (vì Singleton Mung1Manager giữ local state)
        bool daChucMe  = gm.hasGreetedMe;
        bool daChucBo  = gm.hasGreetedBo;
        bool daChucOng = gm.hasGreetedOngNoi;

        Debug.Log($"[OngNoiInteract] Check Greetings: Me={daChucMe}, Bo={daChucBo}, Ong={daChucOng}, daChucTet (all 3)={gm.daChucTet}");

        if (!daChucOng)
        {
            // Lần đầu chúc Ông (Gộp luôn cả Bố vì hai người đứng cạnh nhau)
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Con kính chúc Bố và Ông Nội năm mới dồi dào sức khỏe, vạn sự như ý ạ!" },
                new DialogueManager.DialogueNode { tenNguoiNoi = "Bố", noiDung = "Giỏi lắm, bố chúc con năm mới học hành tiến tới nhé." },
                new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Ôi, cháu ông ngoan quá. Có hiếu với cả bố và ông là tốt." }
            }, () => {
                // Đánh dấu chúc cả 2 người cùng lúc
                Mung1Manager.Instance.Greet("Bố");
                Mung1Manager.Instance.Greet("Ông Nội");

                // Kiểm tra đã chúc Mẹ chưa để phát lì xì ngay lập tức
                if (gm.hasGreetedMe && !gm.daNhanLiXi)
                {
                    Mung1Manager.Instance.ReceiveLuckyMoney();
                }
                else if (!gm.hasGreetedMe)
                {
                    DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Mà con đã chúc Tết Mẹ chưa? Đi chúc Mẹ trước rồi quay lại ông lì xì cho nhé!" }
                    });
                }
                KetThucTuongTac();
            });
        }
        else if (gm.hasGreetedMe && !gm.daNhanLiXi)
        {
            // Trường hợp đã chúc rồi nhưng chưa nhận lì xì (do lúc trước chưa chúc mẹ)
            Mung1Manager.Instance.ReceiveLuckyMoney();
            KetThucTuongTac();
        }
        else if (!gm.hasGreetedMe)
        {
            // Đã chúc rồi nhưng vẫn chưa chúc Mẹ
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Nhớ đi chúc Tết Mẹ trước đã rồi quay lại đây nhận lì xì của ông nhé!" }
            });
            KetThucTuongTac();
        }
        else if (!gm.daChupAnhGiaDinh)
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
