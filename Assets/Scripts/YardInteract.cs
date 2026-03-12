using UnityEngine;

public class YardInteract : NPCBase
{
    protected override void Start()
    {
        base.Start();
        tenNPC = "Cái Chổi";
        hanhDongTuongTac = "quét sân";
        coTheTuongTac = true; 
    }

    protected override void Update()
    {
        base.Update();
        
        if (GameManager.Instance != null)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            // Luôn cho phép tương tác nếu chưa quét xong và ở đúng scene
            coTheTuongTac = sceneName == "VillageScene" && !GameManager.Instance.yardSwept;
        }
    }

    protected override void OnTuongTac()
    {
        if (GameManager.Instance == null)
        {
            KetThucTuongTac();
            return;
        }

        // Tự động nhận nhiệm vụ nếu chưa có
        if (!GameManager.Instance.daNhanNhiemVuQuetSan)
        {
            GameManager.Instance.daNhanNhiemVuQuetSan = true;
        }

        if (GameManager.Instance.yardSwept)
        {
            var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Sân đã sạch bong rồi!" }
            };
            DialogueManager.Instance.BatDauHoiThoai(nodes);
            KetThucTuongTac();
            return;
        }

        // Mở minigame quét sân
        if (YardSweepingMinigame.Instance != null)
        {
            YardSweepingMinigame.Instance.OpenMinigame();
            if (GameManager.Instance != null) GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
        }
        else
        {
            // Fallback nếu không có minigame
            if (RoomVillageManager.Instance != null)
                RoomVillageManager.Instance.SweepYard();
            else
            {
                GameManager.Instance.yardSwept = true;
                GameManager.Instance.OnThongBao?.Invoke("Đã quét sạch lá.");
            }
        }
        KetThucTuongTac();
    }
}
