using UnityEngine;

public class AltarInteract : NPCBase
{
    private void Start()
    {
        tenNPC = "Bàn Thờ";
        hanhDongTuongTac = "tương tác";
        coTheTuongTac = true;
    }

    protected override void OnTuongTac()
    {
        if (RoomVillageManager.Instance == null) 
        {
            KetThucTuongTac();
            return;
        }

        if (!RoomVillageManager.Instance.missionActive)
        {
            var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Bàn thờ gia tiên trang nghiêm." }
            };
            DialogueManager.Instance.BatDauHoiThoai(nodes);
            KetThucTuongTac();
            return;
        }

        if (GameManager.Instance.currentDay == GameManager.TetDay.Day29)
        {
            if (!RoomVillageManager.Instance.hasCloth)
            {
                var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                    new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Mình cần tìm khăn để lau bàn thờ trước." }
                };
                DialogueManager.Instance.BatDauHoiThoai(nodes);
                KetThucTuongTac();
                return;
            }

            if (!RoomVillageManager.Instance.altarCleaned)
            {
                if (AltarCleaningMinigame.Instance != null)
                    AltarCleaningMinigame.Instance.OpenMinigame();
                else
                {
                    RoomVillageManager.Instance.CleanAltar();
                    FlashbackController.Instance?.StartFlashback();
                }
                KetThucTuongTac();
                return;
            }

            DialogueManager.Instance.BatDauHoiThoai(new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Bàn thờ đã được dọn dẹp sạch sẽ cho ngày Tết." }
            });
        }
        else if (GameManager.Instance.currentDay == GameManager.TetDay.Day30)
        {
            if (!GameManager.Instance.daCanhNoiBanh)
            {
                DialogueManager.Instance.BatDauHoiThoai(new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                    new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Đợi vớt bánh xong rồi mới chuẩn bị mâm cúng được." }
                });
            }
            else if (!GameManager.Instance.daChuanBiMamCung)
            {
                if (!GameManager.Instance.daNhanNhiemVuNgay30TuMe)
                {
                    DialogueManager.Instance.BatDauHoiThoai(new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Mình nên hỏi Mẹ xem cần bày mâm cúng thế nào đã." }
                    });
                }
                else if (GameManager.Instance.DaThuThapDuNguQua() && GameManager.Instance.daGoiBanhTet)
                {
                    DialogueManager.Instance.BatDauHoiThoai(new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Mâm ngũ quả và bánh Tét đã sẵn sàng. Con xin dâng lên ông bà tổ tiên." }
                    }, () => {
                        GameManager.Instance.daChuanBiMamCung = true;
                        GameManager.Instance.OnThongBao?.Invoke("Đã chuẩn bị xong mâm cúng Giao Thừa!");
                        GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
                        // Kích hoạt sự kiện Giao Thừa
                        NewYearsEveManager.Instance?.StartCountdown();
                    });
                }
                else
                {
                    DialogueManager.Instance.BatDauHoiThoai(new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Mình cần chuẩn bị đủ mâm ngũ quả và bánh Tét để cúng bái." }
                    });
                }
            }
            else
            {
                DialogueManager.Instance.BatDauHoiThoai(new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                    new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Mâm cúng đã được sửa soạn chu đáo." }
                });
            }
        }
        KetThucTuongTac();
    }
}
