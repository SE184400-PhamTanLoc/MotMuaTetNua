using UnityEngine;
using System.Collections.Generic;

public class Mung1Manager : MonoBehaviour
{
    public static Mung1Manager Instance;

    public bool hasGreetedMe = false;
    public bool hasGreetedBo = false;
    public bool hasGreetedOngNoi = false;

    private void Awake()
    {
        // Để AutoSetup quản lý vòng đời: luôn gán Instance mới nhất
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        // Đồng bộ dữ liệu từ GameManager (nơi thực sự lưu trữ persist)
        if (GameManager.Instance != null)
        {
            hasGreetedMe = GameManager.Instance.hasGreetedMe;
            hasGreetedBo = GameManager.Instance.hasGreetedBo;
            hasGreetedOngNoi = GameManager.Instance.hasGreetedOngNoi;
            CheckAllGreetings();
        }
    }

    public void Greet(string npcName)
    {
        if (GameManager.Instance.currentDay != GameManager.TetDay.Mung1) return;

        if (npcName == "Mẹ")
        {
            hasGreetedMe = true;
            GameManager.Instance.hasGreetedMe = true; // Persist qua scene
        }
        else if (npcName == "Bố")
        {
            hasGreetedBo = true;
            GameManager.Instance.hasGreetedBo = true;
        }
        else if (npcName == "Ông Nội")
        {
            hasGreetedOngNoi = true;
            GameManager.Instance.hasGreetedOngNoi = true;
        }

        // Cập nhật UI ngay sau mỗi lần chúc Tết
        GameManager.Instance.OnNhiemVuThayDoi?.Invoke();

        CheckAllGreetings();
    }

    private void CheckAllGreetings()
    {
        if (hasGreetedMe && hasGreetedBo && hasGreetedOngNoi)
        {
            GameManager.Instance.daChucTet = true;
            GameManager.Instance.HienThongBao("Đã chúc Tết cả nhà! Giờ hãy đến nhận lì xì từ Ông Nội nhé.");
            GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
        }
    }

    public void ReceiveLuckyMoney()
    {
        if (!GameManager.Instance.daChucTet)
        {
            GameManager.Instance.HienThongBao("Phải chúc Tết mọi người trước đã chứ!");
            return;
        }

        GameManager.Instance.daNhanLiXi = true;
        GameManager.Instance.ThemTien(500); // Lì xì 500k
        GameManager.Instance.HienThongBao("Đã nhận lì xì từ Ông Nội! Chúc con năm mới học giỏi.");
        GameManager.Instance.OnNhiemVuThayDoi?.Invoke();

        GameManager.Instance.HienThongBao("Nhiệm vụ cuối cùng: Chụp một tấm ảnh gia đình kỷ niệm.");
    }
    
    public void StartPhotoMinigame()
    {
        if (!GameManager.Instance.daNhanLiXi)
        {
            GameManager.Instance.HienThongBao("Nhận lì xì xong đã rồi mới chụp ảnh nhé!");
            return;
        }
        
        // Kích hoạt FamilyPhotoMinigame (Dùng null check tường minh cho Unity Object)
        if (FamilyPhotoMinigame.Instance != null)
        {
            FamilyPhotoMinigame.Instance.StartMinigame();
        }
        else
        {
            Debug.LogError("[Mung1Manager] FamilyPhotoMinigame Instance not found in scene!");
            GameManager.Instance.HienThongBao("Lỗi: Không tìm thấy máy ảnh trong khu vực này.");
        }
    }
}
