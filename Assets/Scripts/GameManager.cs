using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GameManager - Quản lý trạng thái game: tiền, nhiệm vụ, túi đồ
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("=== TIỀN CỦA NGƯỜI CHƠI ===")]
    public int soTienBanDau = 1000;

    [Header("=== TRẠNG THÁI NHIỆM VỤ ===")]
    public bool daMuaMai = false;
    public bool daLayMai = false; // Cần lấy mai trước khi mang về
    public bool daMangMaiVeMe = false;

    [Header("=== EVENTS ===")]
    public UnityEvent<int> OnTienThayDoi;
    public UnityEvent<string> OnThongBao;
    public UnityEvent OnNhiemVuHoanThanh;
    public UnityEvent OnMuaMaiThanhCong;
    public UnityEvent OnLayMaiThanhCong; // Event phụ nếu cần update UI

    private int _soTien;
    private string _loaiMaiDaMua = "";

    public int SoTien
    {
        get => _soTien;
        private set
        {
            _soTien = value;
            OnTienThayDoi?.Invoke(_soTien);
        }
    }

    public string LoaiMaiDaMua => _loaiMaiDaMua;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Khởi tạo Events (quan trọng khi tạo bằng AddComponent)
        if (OnTienThayDoi == null) OnTienThayDoi = new UnityEvent<int>();
        if (OnThongBao == null) OnThongBao = new UnityEvent<string>();
        if (OnNhiemVuHoanThanh == null) OnNhiemVuHoanThanh = new UnityEvent();
        if (OnMuaMaiThanhCong == null) OnMuaMaiThanhCong = new UnityEvent();
        if (OnLayMaiThanhCong == null) OnLayMaiThanhCong = new UnityEvent();
    }

    private void Start()
    {
        SoTien = soTienBanDau;
        OnThongBao?.Invoke("[ NHIỆM VỤ ] Mua một cây mai về cho mẹ chưng Tết!");
    }

    public bool CoĐuTien(int soTienCan)
    {
        return _soTien >= soTienCan;
    }

    public bool TruTien(int soTienTru)
    {
        if (_soTien >= soTienTru)
        {
            SoTien -= soTienTru;
            return true;
        }
        return false;
    }

    public void ThemTien(int soTienThem)
    {
        SoTien += soTienThem;
        OnThongBao?.Invoke($"[ NHẬN TIỀN ] Nhận được {FormatTien(soTienThem)}!");
    }

    public bool MuaMai(string loaiMai, int giaTien)
    {
        if (daMuaMai)
        {
            OnThongBao?.Invoke("Bạn đã mua mai rồi!");
            return false;
        }

        if (!TruTien(giaTien))
        {
            OnThongBao?.Invoke("Không đủ tiền mua mai!");
            return false;
        }

        daMuaMai = true;
        _loaiMaiDaMua = loaiMai;

        // Thay đổi thông báo: Báo người chơi cần ra lấy mai
        OnThongBao?.Invoke($"Đã trả tiền {loaiMai}! Hãy đến chỗ chậu mai để lấy cây dọn về nhé!");
        OnMuaMaiThanhCong?.Invoke();
        return true;
    }

    public void LayMai()
    {
        if (!daMuaMai || daLayMai) return;
        daLayMai = true;
        daMangMaiVeMe = true; // Bỏ qua bước đi tới nhà mẹ, lấy mai là xong luôn

        OnThongBao?.Invoke("Cây mai thật đẹp! Mẹ chắc chắn sẽ rất vui. Nhiệm vụ hoàn thành!");
        OnNhiemVuHoanThanh?.Invoke();
        OnLayMaiThanhCong?.Invoke();
    }

    public void MangMaiVeMeHoanThanh()
    {
        // Hàm này giữ lại để cho code cũ không bị lỗi (compatibility)
        // Nhưng thực tế nhiệm vụ đã hoàn thành ngay khi LayMai()
        if (!daLayMai) return; 
        daMangMaiVeMe = true;
        OnNhiemVuHoanThanh?.Invoke();
    }

    public static string FormatTien(int soTienNghin)
    {
        int soTienDay = soTienNghin * 1000;
        return string.Format("{0:N0}đ", soTienDay);
    }

    public string LayMoTaNhiemVu()
    {
        if (!daMuaMai)
            return "- Mua một cây mai về cho mẹ chưng Tết";
        else if (!daLayMai)
            return $"- Đến chỗ chậu mai để lấy {_loaiMaiDaMua}";
        else if (!daMangMaiVeMe)
            return $"- Mang {_loaiMaiDaMua} về cho mẹ";
        else
            return "- Đã hoàn thành nhiệm vụ!";
    }
}
