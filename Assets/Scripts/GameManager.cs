using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GameManager - Quản lý trạng thái game: tiền, nhiệm vụ, túi đồ
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("=== TIỀN CỦA NGƯỜI CHƠI ===")]
    [Tooltip("Số tiền khởi điểm khi chưa được Mẹ giao nhiệm vụ đi chợ. Mặc định = 0, Mẹ sẽ cho tiền sau.")]
    public int soTienBanDau = 0;

    public enum TetDay { Day29, Day30, Mung1 }
    [Header("=== TRẠNG THÁI NGÀY ===")]
    public TetDay currentDay = TetDay.Day29;

    [Header("=== TRẠNG THÁI NHIỆM VỤ CHUNG ===")]
    public bool daMuaMai = false;
    public bool daLayMai = false;
    public bool daMangMaiVeMe = false;
    public bool daKetThucDiCho = false;
    public bool daNhanTienTuMe = false; // Mới: theo dõi đã nhận tiền từ mẹ chưa

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

    [Header("=== NHIỆM VỤ 3: QUÉT SÂN & LAU BÀN THỜ ===")]
    public bool daNhanNhiemVuQuetSan = false;
    public bool yardSwept = false;
    public bool altarCleaned = false;

    [Header("=== NHIỆM VỤ NGÀY 30 & MÙNG 1 ===")]
    public bool daGoiBanhTet = false;
    public bool daCanhNoiBanh = false;
    public bool daChuanBiMamCung = false;
    [Tooltip("Đã nói chuyện với Mẹ để nhận các nhiệm vụ ngày 30 (gói bánh, canh nồi, mâm cúng)")]
    public bool daNhanNhiemVuNgay30TuMe = false;
    public bool daChucTet = false;
    public bool daNhanLiXi = false;
    public bool daChupAnhGiaDinh = false;
    // Trạng thái chúc Tết từng thành viên (lưu ở đây để persist qua scene)
    public bool hasGreetedMe = false;
    public bool hasGreetedBo = false;
    public bool hasGreetedOngNoi = false;

    [Header("=== TRẠNG THÁI PHASE CHƠI ===")]
    public bool isVillagePhase = false;

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
    }

    private void Update()
    {
        // === DEBUG SKIP SYSTEM (F1-F5) ===
        if (Input.GetKeyDown(KeyCode.F1)) SkipToDay29Market();
        if (Input.GetKeyDown(KeyCode.F2)) SkipToDay29Home();
        if (Input.GetKeyDown(KeyCode.F3)) SkipToDay30();
        if (Input.GetKeyDown(KeyCode.F4)) SkipToMung1();
        if (Input.GetKeyDown(KeyCode.F5)) CompleteCurrentMissions();
    }

    private void SkipToDay29Market()
    {
        currentDay = TetDay.Day29;
        daNhanTienTuMe = true;
        if (SoTien < 1000) SoTien = 1000;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Day_28_Scene");
        HienThongBao("DEBUG: Đã chuyển sang Ngày 29 - Chợ Tết");
    }

    private void SkipToDay29Home()
    {
        currentDay = TetDay.Day29;
        daNhanTienTuMe = true;
        if (SoTien < 1000) SoTien = 1000;
        UnityEngine.SceneManagement.SceneManager.LoadScene("VillageScene");
        HienThongBao("DEBUG: Đã chuyển sang Ngày 29 - Ở Nhà");
    }

    private void SkipToDay30()
    {
        currentDay = TetDay.Day30;
        daNhanNhiemVuNgay30TuMe = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("VillageScene");
        HienThongBao("DEBUG: Đã chuyển sang Ngày 30 - Gói Bánh");
    }

    private void SkipToMung1()
    {
        currentDay = TetDay.Mung1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("VillageScene");
        HienThongBao("DEBUG: Đã chuyển sang Mùng 1 - Chúc Tết");
    }

    private void CompleteCurrentMissions()
    {
        if (currentDay == TetDay.Day29)
        {
            daMuaMai = true;
            daLayMai = true;
            daMangMaiVeMe = true;
            altarCleaned = true;
            yardSwept = true;
        }
        else if (currentDay == TetDay.Day30)
        {
            coLaChuoi = coGaoNep = coDau = coThit = true;
            coMangCau = coDua = coDuDu = coXoai = true;
            daGoiBanhTet = true;
            daCanhNoiBanh = true;
            daNhanNhiemVuNgay30TuMe = true;
        }
        else if (currentDay == TetDay.Mung1)
        {
            daChucTet = true;
            daNhanLiXi = true;
            daChupAnhGiaDinh = true;
        }
        OnNhiemVuThayDoi?.Invoke();
        HienThongBao("DEBUG: Đã hoàn thành tất cả nhiệm vụ hiện tại!");
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

        // Hoàn thành nhiệm vụ mua mai
        if (currentDay == TetDay.Day29)
        {
            OnThongBao?.Invoke("Cây mai thật đẹp! Mẹ chắc chắn sẽ rất vui. Hãy mang mai về nhà cho Mẹ xem ngay thôi.");
        }
        else
        {
            OnThongBao?.Invoke("Cây mai đã sẵn sàng, mẹ sẽ rất vui khi thấy đó.");
        }
        OnNhiemVuHoanThanh?.Invoke();
        OnLayMaiThanhCong?.Invoke();
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
            // Hoàn thành nhiệm vụ chuẩn bị nguyên liệu gói bánh
            if (currentDay == TetDay.Day30)
            {
                OnThongBao?.Invoke("[ NHIỆM VỤ ] Đã chuẩn bị đủ nguyên liệu gói bánh.");
            }
            else
            {
                OnThongBao?.Invoke("Đã đủ nguyên liệu cho bánh, mai 30 Tết nhớ về nhà phụ mẹ gói nhé.");
            }
            OnNhiemVuHoanThanh?.Invoke();
        }

        if (DaThuThapDuNguQua())
        {
            // Hoàn thành nhiệm vụ mâm Ngũ Quả (chung cho cả ngày 29/30)
            OnThongBao?.Invoke("[ NHIỆM VỤ ] Đã đủ trái cây cho mâm Ngũ Quả.");
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

    public void HienThongBao(string mess)
    {
        OnThongBao?.Invoke(mess);
    }

    // Hàm tiện ích để kiểm tra xem đã xong HẾT nhiệm vụ ở chợ chưa
    public bool DaXongHetNhiemVu()
    {
        return DaThuThapDuNguyenLieu() && DaThuThapDuNguQua() && daMangMaiVeMe;
    }

    public string LayMoTaNhiemVu()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string currentTask = "";

        if (currentDay == TetDay.Day29)
        {
            // currentTask += "<color=yellow><b>[ NGÀY 29 TẾT - CHUẨN BỊ NHÀ CỬA ]</b></color>\n\n";

            if (sceneName == "Day_28_Scene")
            {
                // Ở chợ trong ngày 29: nhiệm vụ chính là mua cây mai (đồ ăn uống sẽ được tính cho ngày 30)
                currentTask += (daMuaMai ? " <color=green>[x]</color> " : " <color=white>[ ]</color> ")
                               + "Mua cây mai chưng Tết\n";
            }
            else if (sceneName == "RoomVillage" || sceneName == "RoomScene" || sceneName == "VillageScene")
            {
                // Ở nhà trong ngày 29: dọn dẹp + mang mai về
                currentTask += (altarCleaned ? " <color=green>[x]</color> " : " <color=white>[ ]</color> ") + "Lau bàn thờ\n" +
                               (yardSwept ? " <color=green>[x]</color> " : " <color=white>[ ]</color> ") + "Quét sân\n" +
                               (daMangMaiVeMe ? " <color=green>[x]</color> " : " <color=white>[ ]</color> ") + "Mang hoa mai về nhà\n";
            }
        }
        else if (currentDay == TetDay.Day30)
        {
            // currentTask += "<color=yellow><b>[ NGÀY 30 TẾT - ĐOÀN TỤ ]</b></color>\n\n";

            // Nhiệm vụ đồ ăn thức uống và nghi lễ của ngày 30
            currentTask += (DaThuThapDuNguyenLieu() ? " <color=green>[x]</color> " : " <color=white>[ ]</color> ")
                           + "Chuẩn bị đủ nguyên liệu gói bánh\n";
            currentTask += (daGoiBanhTet ? " <color=green>[x]</color> " : " <color=white>[ ]</color> ")
                           + "Gói bánh Tét\n";
            currentTask += (daCanhNoiBanh ? " <color=green>[x]</color> " : " <color=white>[ ]</color> ")
                           + "Canh nồi bánh\n";
            currentTask += (DaThuThapDuNguQua() ? " <color=green>[x]</color> " : " <color=white>[ ]</color> ")
                           + "Thu thập đủ 4 loại trái cây mâm Ngũ Quả\n";
        }
        else if (currentDay == TetDay.Mung1)
        {
            // currentTask += "<color=yellow><b>[ MÙNG 1 TẾT - KHỞI ĐẦU ]</b></color>\n\n";
            currentTask += (hasGreetedMe                        ? " <color=green>[x]</color> " : " <color=white>[ ]</color> ") + "Chúc Tết Mẹ\n";
            currentTask += (hasGreetedBo && hasGreetedOngNoi   ? " <color=green>[x]</color> " : " <color=white>[ ]</color> ") + "Chúc Tết Bố và Ông Nội\n";
            currentTask += (daNhanLiXi                          ? " <color=green>[x]</color> " : " <color=white>[ ]</color> ") + "Nhận lì xì\n";
            currentTask += (daChupAnhGiaDinh                    ? " <color=green>[x]</color> " : " <color=white>[ ]</color> ") + "Chụp ảnh gia đình\n";
        }

        if (string.IsNullOrEmpty(currentTask)) return "- Hãy tìm gặp Mẹ để nhận việc.";
        return currentTask;
    }
}
