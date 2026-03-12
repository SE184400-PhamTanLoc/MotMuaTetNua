using UnityEngine;

public class ClothInteract : NPCBase
{
    private void Start()
    {
        tenNPC = "Cái Khăn";
        hanhDongTuongTac = "lấy";
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
                new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Mẹ chưa bảo mình làm gì cả." }
            };
            DialogueManager.Instance.BatDauHoiThoai(nodes);
            KetThucTuongTac();
            return;
        }

        if (!RoomVillageManager.Instance.hasCloth)
        {
            RoomVillageManager.Instance.PickUpCloth();
            // Ẩn object hoặc phá huỷ
            gameObject.SetActive(false);
        }
        KetThucTuongTac();
    }
}
