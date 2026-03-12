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
            // Mở minigame lau bàn thờ thay vì hoàn thành ngay
            if (AltarCleaningMinigame.Instance != null)
            {
                AltarCleaningMinigame.Instance.OpenMinigame();
            }
            else
            {
                // Fallback nếu chưa có minigame trong scene
                RoomVillageManager.Instance.CleanAltar();
            }
            KetThucTuongTac();
            return;
        }

        if (!RoomVillageManager.Instance.incenseLit)
        {
            RoomVillageManager.Instance.LightIncense();
            
            // Kích hoạt flashback
            FlashbackController.Instance?.StartFlashback();
            KetThucTuongTac();
            return;
        }
        
        var endNodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
            new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Bàn thờ đã được dọn dẹp sạch sẽ và thắp nhang." }
        };
        DialogueManager.Instance.BatDauHoiThoai(endNodes);
        KetThucTuongTac();
    }
}
