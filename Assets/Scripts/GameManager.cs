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

    [Header("=== NHIỆM VỤ 1: CHUẨN BỊ NGUYÊN LIỆU ===")]
    public bool coLaChuoi = false;
    public bool coGaoNep = false;
    public bool coDau = false;
    public bool coThit = false;
    public bool daNhanNhiemVu1 = false;
    
    [Header("=== NHIỆM VỤ 2: MÂM NGŨ QUẢ ===")]
    public bool coMangCau = false;
    public bool coDua = false;
    public bool coDuDu = false;
    public bool coXoai = false;
    public bool daNhanNhiemVu2 = false;

    [Header("=== EVENTS ===")]
    public UnityEvent<int> OnTienThayDoi;
    public UnityEvent<string> OnThongBao;
    public UnityEvent OnNhiemVuHoanThanh;
    public UnityEvent OnNhiemVuThayDoi;
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
        if (OnNhiemVuThayDoi == null) OnNhiemVuThayDoi = new UnityEvent();
        if (OnMuaMaiThanhCong == null) OnMuaMaiThanhCong = new UnityEvent();
        if (OnLayMaiThanhCong == null) OnLayMaiThanhCong = new UnityEvent();
    }

    private void Start()
    {
        SoTien = soTienBanDau;
        OnThongBao?.Invoke("[ NHIỆM VỤ ] Mua một cây mai về cho mẹ chưng Tết!");
        
        // Kích hoạt luôn Nhiệm vụ 1 và 2 để người dùng thấy danh sách (theo yêu cầu)
        daNhanNhiemVu1 = true;
        daNhanNhiemVu2 = true;
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

        // Sau khi hoàn thành nhiệm vụ Mai, chuyển sang Nhiệm vụ 1
        daNhanNhiemVu1 = true;
        OnThongBao?.Invoke("[ NHIỆM VỤ 1 ] Đi chuẩn bị nguyên liệu gói bánh chưng: Lá chuối, Gạo nếp, Đậu, Thịt.");
    }

    public bool MuaNguyenLieu(string ten, int gia, bool hienThongBao = true)
    {
        if (!TruTien(gia))
        {
            OnThongBao?.Invoke($"Không đủ tiền mua {ten}!");
            return false;
        }

        string lowName = ten.ToLower();
        if (lowName.Contains("lá chuối")) coLaChuoi = true;
        else if (lowName.Contains("gạo nếp")) coGaoNep = true;
        else if (lowName.Contains("đậu")) coDau = true;
        else if (lowName.Contains("thịt")) coThit = true;
        else if (lowName.Contains("mãng cầu")) coMangCau = true;
        else if (lowName.Contains("dừa")) coDua = true;
        else if (lowName.Contains("đu đủ")) coDuDu = true;
        else if (lowName.Contains("xoài")) coXoai = true;

        if (hienThongBao) OnThongBao?.Invoke($"Đã mua {ten}!");
        OnNhiemVuThayDoi?.Invoke(); // Đảm bảo UI cập nhật checkbox
        
        if (DaThuThapDuNguyenLieu())
        {
            OnThongBao?.Invoke("Đã đủ nguyên liệu! Hãy về nhà chuẩn bị gói bánh thôi.");
            OnNhiemVuHoanThanh?.Invoke();
        }

        if (DaThuThapDuNguQua())
        {
            OnThongBao?.Invoke("Đã đủ mâm Ngũ Quả! Mọi thứ đã sẵn sàng cho ngày Tết.");
            OnNhiemVuHoanThanh?.Invoke();
        }

        return true;
    }

    public bool DaThuThapDuNguyenLieu()
    {
        return coLaChuoi && coGaoNep && coDau && coThit;
    }

    public bool DaThuThapDuNguQua()
    {
        return coMangCau && coDua && coDuDu && coXoai;
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
        string currentTask = "";
        
        // Nhiệm vụ chính (Mai)
        if (!daMuaMai)
            currentTask = "- Mua một cây mai về cho mẹ chưng Tết\n";
        else if (!daLayMai)
            currentTask = $"- Đến chỗ chậu mai để lấy {_loaiMaiDaMua}\n";
        else if (!daMangMaiVeMe)
            currentTask = "- Mang mai về cho mẹ\n";

        // Nhiệm vụ 1: Nguyên liệu (Hiện song song hoặc sau)
        if (daNhanNhiemVu1)
        {
            if (DaThuThapDuNguyenLieu())
            {
                currentTask += "- Đã đủ nguyên liệu gói bánh!\n";
            }
            else
            {
                string s = "- Mua đồ để gói bánh:\n";
                s += (coLaChuoi ? " <color=green>[x]</color> " : " <color=red>[ ]</color> ") + "Lá chuối\n";
                s += (coGaoNep ? " <color=green>[x]</color> " : " <color=red>[ ]</color> ") + "Gạo nếp\n";
                s += (coDau ? " <color=green>[x]</color> " : " <color=red>[ ]</color> ") + "Đậu xanh\n";
                s += (coThit ? " <color=green>[x]</color> " : " <color=red>[ ]</color> ") + "Thịt heo\n";
                currentTask += s;
            }
        }

        // Nhiệm vụ 2: Mâm Ngũ Quả (Cầu Vừa Đủ Xài)
        if (daNhanNhiemVu2)
        {
            if (DaThuThapDuNguQua())
            {
                currentTask += "- Đã đủ mâm Ngũ Quả!\n";
            }
            else
            {
                string ng = "- Mua đồ chưng mâm Ngũ Quả:\n";
                ng += (coMangCau ? " <color=green>[x]</color> " : " <color=red>[ ]</color> ") + "Mãng cầu\n";
                ng += (coDua ? " <color=green>[x]</color> " : " <color=red>[ ]</color> ") + "Dừa\n";
                ng += (coDuDu ? " <color=green>[x]</color> " : " <color=red>[ ]</color> ") + "Đu đủ\n";
                ng += (coXoai ? " <color=green>[x]</color> " : " <color=red>[ ]</color> ") + "Xoài\n";
                currentTask += ng;
            }
        }

        bool daXongHet = DaThuThapDuNguyenLieu() && DaThuThapDuNguQua() && daMangMaiVeMe;
        if (daXongHet)
            return "<color=green><b>- Bạn đã sẵn sàng đón Tết!</b></color>\n- Hãy trở về nhà thôi.";

        return currentTask;
    }
}
