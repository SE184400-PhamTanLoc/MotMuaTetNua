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
        Instance = this;
    }

    public void Greet(string npcName)
    {
        if (GameManager.Instance.currentDay != GameManager.TetDay.Mung1) return;

        if (npcName == "Mẹ") hasGreetedMe = true;
        else if (npcName == "Bố") hasGreetedBo = true;
        else if (npcName == "Ông Nội") hasGreetedOngNoi = true;

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
        
        // Kích hoạt FamilyPhotoMinigame
        FamilyPhotoMinigame.Instance?.StartMinigame();
    }
}
