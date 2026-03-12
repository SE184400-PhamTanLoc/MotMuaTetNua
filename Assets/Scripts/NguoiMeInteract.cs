using UnityEngine;

public class NguoiMeInteract : NPCBase
{
    private void Start()
    {
        tenNPC = "Mẹ";
        hanhDongTuongTac = "nói chuyện";
        coTheTuongTac = true;
    }

    protected override void OnTuongTac()
    {
        if (RoomVillageManager.Instance != null && !RoomVillageManager.Instance.missionActive)
        {
            var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Vào nhà lau bàn thờ giúp mẹ." }
            };
            DialogueManager.Instance.BatDauHoiThoai(nodes);
            RoomVillageManager.Instance.StartMission();
        }
        else if (RoomVillageManager.Instance != null && RoomVillageManager.Instance.missionActive)
        {
            if (!RoomVillageManager.Instance.altarCleaned)
            {
                var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                    new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Con lau bàn thờ xong chưa?" }
                };
                DialogueManager.Instance.BatDauHoiThoai(nodes);
            }
            else
            {
                var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                    new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Giỏi lắm, ngoan quá." }
                };
                DialogueManager.Instance.BatDauHoiThoai(nodes);
            }
        }
        KetThucTuongTac();
    }
}
