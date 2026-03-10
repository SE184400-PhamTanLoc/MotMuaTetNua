using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// BauCuaMinigame - Minigame Bầu Cua Tôm Cá
/// Kế thừa NPCBase để người chơi tương tác bằng E
/// </summary>
public class BauCuaMinigame : NPCBase
{
    [System.Serializable]
    public class OCuoc
    {
        public string tenCon;
        public int soTienCuoc = 0;
        [HideInInspector] public TextMeshProUGUI textSoTienCuoc;
    }

    [Header("=== UI ===")]
    public GameObject uiPanel;
    public TextMeshProUGUI[] textXucXac = new TextMeshProUGUI[3];
    public TextMeshProUGUI textKetQua;
    public TextMeshProUGUI textTongTien;
    public Button btnLac;
    public Button btnDatLai;
    public Button btnThoat;
    public OCuoc[] oCuoc = new OCuoc[6];

    private string[] danhSachCon = { "Bầu", "Cua", "Tôm", "Cá", "Gà", "Nai" };
    private bool dangLac = false;
    private int tongTienDaCuoc = 0;

    private void Awake()
    {
        tenNPC = "Sòng Bầu Cua";
        quayVePhiaPlayer = false;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTuongTac()
    {
        if (uiPanel == null) return;

        uiPanel.SetActive(true);
        CapNhatTongTien();

        // Reset xúc xắc
        for (int i = 0; i < 3; i++)
        {
            if (textXucXac[i] != null) textXucXac[i].text = "?";
        }

        if (textKetQua != null)
            textKetQua.text = "Bơm tiền vào con mình thích rồi Lắc nhé!";
    }

    public void DatCuoc(int indexCon, int soTien)
    {
        if (dangLac) return;
        if (GameManager.Instance == null) return;
        
        if (!GameManager.Instance.CoĐuTien(soTien))
        {
            if (textKetQua != null)
                textKetQua.text = "Hết tiền rồi bạn ơi!";
            return;
        }

        GameManager.Instance.TruTien(soTien);
        oCuoc[indexCon].soTienCuoc += soTien;
        tongTienDaCuoc += soTien;

        if (oCuoc[indexCon].textSoTienCuoc != null)
            oCuoc[indexCon].textSoTienCuoc.text = GameManager.FormatTien(oCuoc[indexCon].soTienCuoc);

        CapNhatTongTien();
    }

    public void DatLaiCuoc()
    {
        if (dangLac) return;

        // Hoàn tiền
        for (int i = 0; i < oCuoc.Length; i++)
        {
            if (oCuoc[i].soTienCuoc > 0)
            {
                GameManager.Instance.ThemTien(oCuoc[i].soTienCuoc);
                oCuoc[i].soTienCuoc = 0;
                if (oCuoc[i].textSoTienCuoc != null)
                    oCuoc[i].textSoTienCuoc.text = "0đ";
            }
        }
        tongTienDaCuoc = 0;
        CapNhatTongTien();

        if (textKetQua != null)
            textKetQua.text = "Đã hoàn tiền. Đặt lại đi nào!";
    }

    public void LacXucXac()
    {
        if (dangLac) return;

        // Kiểm tra có đặt cược gì chưa
        if (tongTienDaCuoc <= 0)
        {
            if (textKetQua != null)
                textKetQua.text = "Phải đặt cược trước mới lắc được!";
            return;
        }

        StartCoroutine(AnimationLac());
    }

    private IEnumerator AnimationLac()
    {
        dangLac = true;
        if (btnLac != null) btnLac.interactable = false;

        // Animation lắc xúc xắc
        for (int lap = 0; lap < 15; lap++)
        {
            for (int i = 0; i < 3; i++)
            {
                int randomIdx = Random.Range(0, danhSachCon.Length);
                if (textXucXac[i] != null) textXucXac[i].text = danhSachCon[randomIdx];
            }
            yield return new WaitForSeconds(0.08f);
        }

        // Kết quả cuối
        string[] ketQua = new string[3];
        for (int i = 0; i < 3; i++)
        {
            int randomIdx = Random.Range(0, danhSachCon.Length);
            ketQua[i] = danhSachCon[randomIdx];
            if (textXucXac[i] != null) textXucXac[i].text = ketQua[i];
        }

        // Tính tiền thắng
        int tongTienThang = 0;
        for (int i = 0; i < oCuoc.Length; i++)
        {
            if (oCuoc[i].soTienCuoc <= 0) continue;

            int soLanXuatHien = 0;
            for (int j = 0; j < 3; j++)
            {
                if (ketQua[j] == oCuoc[i].tenCon) soLanXuatHien++;
            }

            if (soLanXuatHien > 0)
            {
                tongTienThang += oCuoc[i].soTienCuoc * (soLanXuatHien + 1);
            }
        }

        // Thưởng tiền
        if (tongTienThang > 0)
        {
            GameManager.Instance.ThemTien(tongTienThang);
            if (textKetQua != null)
                textKetQua.text = $"THẮNG {GameManager.FormatTien(tongTienThang)}! Quá đỉnh!";
        }
        else
        {
            if (textKetQua != null)
                textKetQua.text = "Thua rồi! Cố lên lần sau nhé!";
        }

        // Reset cược
        for (int i = 0; i < oCuoc.Length; i++)
        {
            oCuoc[i].soTienCuoc = 0;
            if (oCuoc[i].textSoTienCuoc != null)
                oCuoc[i].textSoTienCuoc.text = "0đ";
        }
        tongTienDaCuoc = 0;

        CapNhatTongTien();
        dangLac = false;
        if (btnLac != null) btnLac.interactable = true;
    }

    public void DongSoba()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        KetThucTuongTac();
    }

    private void CapNhatTongTien()
    {
        if (textTongTien != null && GameManager.Instance != null)
            textTongTien.text = $"Tiền trong túi: {GameManager.FormatTien(GameManager.Instance.SoTien)}";
    }
}
