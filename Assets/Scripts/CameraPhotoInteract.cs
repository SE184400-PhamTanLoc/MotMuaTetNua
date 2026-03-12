using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tương tác với chiếc máy ảnh trong sân để bắt đầu minigame chụp ảnh gia đình (Mùng 1).
/// Gắn script này lên GameObject chiếc máy ảnh trong scene (có collider) và đặt layer giống NPC.
/// </summary>
public class CameraPhotoInteract : NPCBase
{
    protected override void Start()
    {
        base.Start();
        tenNPC = "máy ảnh";
        hanhDongTuongTac = "chụp ảnh";
        coTheTuongTac = true;

        // Đảm bảo có collider trigger để PlayerInteraction bắt được
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider bc = gameObject.AddComponent<BoxCollider>();
            bc.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }

        // Đưa lên layer NPC (6) để PlayerInteraction quét được
        gameObject.layer = 6;
    }

    protected override void OnTuongTac()
    {
        if (GameManager.Instance == null)
        {
            KetThucTuongTac();
            return;
        }

        // Chỉ hoạt động vào Mùng 1
        if (GameManager.Instance.currentDay != GameManager.TetDay.Mung1)
        {
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Giờ chưa phải lúc chụp ảnh gia đình." }
            });
            KetThucTuongTac();
            return;
        }

        // Phải chúc Tết và nhận lì xì xong mới chụp ảnh
        if (!GameManager.Instance.daChucTet)
        {
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Mình nên chúc Tết mọi người xong rồi hãy chụp ảnh." }
            });
            KetThucTuongTac();
            return;
        }

        if (!GameManager.Instance.daNhanLiXi)
        {
            DialogueManager.Instance.BatDauHoiThoai(new List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Nhận lì xì của Ông Nội xong rồi chụp ảnh sẽ trọn vẹn hơn." }
            });
            KetThucTuongTac();
            return;
        }

        // Tất cả điều kiện đã đủ → bắt đầu minigame chụp ảnh
        if (FamilyPhotoMinigame.Instance != null)
        {
            FamilyPhotoMinigame.Instance.StartMinigame();
        }
        else
        {
            // Fallback: gọi qua Mung1Manager nếu có logic riêng
            Mung1Manager.Instance?.StartPhotoMinigame();
        }

        KetThucTuongTac();
    }
}

