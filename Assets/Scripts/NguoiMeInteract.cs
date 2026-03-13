using UnityEngine;

public class NguoiMeInteract : NPCBase
{
    private void FreezeAllRigidbodies()
    {
        // Đã có trong NPCBase
    }

    protected override void Start()
    {
        base.Start();
        tenNPC = "Mẹ";
        hanhDongTuongTac = "nói chuyện";
        coTheTuongTac = true;
    }

    // Awake và OnEnable đã có trong NPCBase gọi FreezeAllRigidbodies()

    protected override void OnTuongTac()
    {
        // base.BatDauTuongTac() đã gọi FreezeAllRigidbodies()
        if (GameManager.Instance != null && GameManager.Instance.currentDay == GameManager.TetDay.Mung1)
        {
            if (!Mung1Manager.Instance.hasGreetedMe)
            {
                var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                    new DialogueManager.DialogueNode { tenNguoiNoi = "Bản thân", noiDung = "Con chúc Mẹ năm mới luôn trẻ đẹp, mạnh khỏe và hạnh phúc ạ!" },
                    new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Cảm ơn con trai. Mẹ cũng chúc con năm mới vạn sự như ý!" }
                };
                DialogueManager.Instance.BatDauHoiThoai(nodes, () => {
                    Mung1Manager.Instance.Greet("Mẹ");
                    KetThucTuongTac();
                });
            }
            else
            {
                DialogueManager.Instance.BatDauHoiThoai(new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                    new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Năm mới vui vẻ nhé con!" }
                });
                KetThucTuongTac();
            }
            return;
        }

        // Ngày 30: tất cả nhiệm vụ gói bánh / canh nồi / chuẩn bị Tết đều phải được Mẹ giao
        if (GameManager.Instance != null && GameManager.Instance.currentDay == GameManager.TetDay.Day30)
        {
            if (!GameManager.Instance.daNhanNhiemVuNgay30TuMe)
            {
                var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                    new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Hôm nay 30 Tết rồi, nhà mình phải gói bánh Tét, canh nồi bánh và sửa soạn mâm cúng cho chu đáo." },
                    new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Con phụ Ba gói bánh, rồi canh nồi bánh với Ông Nội, xong xuôi thì chuẩn bị mâm cúng nghe chưa." }
                };
                DialogueManager.Instance.BatDauHoiThoai(nodes, () =>
                {
                    GameManager.Instance.daNhanNhiemVuNgay30TuMe = true;
                    GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
                    KetThucTuongTac();
                });
            }
            else
            {
                // Kiểm tra xem đã làm xong HẾT nhiệm vụ Ngày 30 chưa
                bool duNguyenLieu = GameManager.Instance.DaThuThapDuNguyenLieu();
                bool goiBanhXong = GameManager.Instance.daGoiBanhTet;
                bool canhNoiXong = GameManager.Instance.daCanhNoiBanh;
                bool duNguQua = GameManager.Instance.DaThuThapDuNguQua();
                bool hoanThanhNgay30 = duNguyenLieu && goiBanhXong && canhNoiXong && duNguQua;

                if (!hoanThanhNgay30)
                {
                    var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Nhớ gói bánh, canh nồi và chuẩn bị đồ đạc cho ngày Tết thật chu đáo đó con." }
                    };
                    DialogueManager.Instance.BatDauHoiThoai(nodes, KetThucTuongTac);
                }
                else
                {
                    // Tất cả việc ngày 30 đã xong → dẫn sang khoảnh khắc giao thừa, rồi Mùng 1
                    var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Giỏi lắm, bánh cũng xong, nồi cũng canh, đồ cúng cũng đủ cả rồi." },
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Đêm nay cả nhà mình cùng đón giao thừa cho thiệt vui nha con." }
                    };
                    DialogueManager.Instance.BatDauHoiThoai(nodes, () =>
                    {
                        if (NewYearsEveManager.Instance != null)
                        {
                            NewYearsEveManager.Instance.StartCountdown();
                        }
                        else
                        {
                            // Fallback: nếu thiếu manager giao thừa thì chuyển thẳng sang Mùng 1
                            GameManager.Instance.currentDay = GameManager.TetDay.Mung1;
                            GameManager.Instance.HienThongBao("Sáng Mùng 1 Tết... Trời đất giao hòa.");
                            GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
                        }
                        KetThucTuongTac();
                    });
                }
            }
            return;
        }

        if (RoomVillageManager.Instance != null)
        {
            // Đồng bộ trạng thái nhiệm vụ ngày 29 dựa trên dữ liệu hiện tại
            // LƯU Ý: RoomVillageManager được tạo mới theo scene, nên trạng thái thật phải đọc thêm từ GameManager
            bool altarCleaned = RoomVillageManager.Instance.altarCleaned || GameManager.Instance.altarCleaned;
            bool yardSwept = RoomVillageManager.Instance.yardSwept || GameManager.Instance.yardSwept;
            bool maiDone = GameManager.Instance.daMangMaiVeMe;
            // Điều kiện hoàn thành Ngày 29 chỉ cần: lau bàn thờ + quét sân + mang mai về nhà
            bool hoanThanhNgay29 = altarCleaned && yardSwept && maiDone;

            if (!RoomVillageManager.Instance.missionActive)
            {
                // Chưa chính thức nhận nhiệm vụ từ Mẹ
                if (!altarCleaned)
                {
                    // Bước 1: giao nhiệm vụ lau bàn thờ
                    var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Vào nhà lau bàn thờ giúp mẹ." }
                    };
                    DialogueManager.Instance.BatDauHoiThoai(nodes);
                    RoomVillageManager.Instance.StartMission();
                }
                else if (altarCleaned && (!yardSwept || !maiDone))
                {
                    // Người chơi đã tự lau xong bàn thờ trước khi nói chuyện, bỏ qua bước 1 và giao luôn bước 2
                    var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Con lau bàn thờ gọn gàng rồi đó, giỏi lắm." },
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Giờ con ra ngoài sân quét bớt lá khô giúp mẹ nhé. Mẹ đưa cho 1 triệu ra chợ sắm cây mai thiệt đẹp mang về chưng Tết nghe con." }
                    };
                    DialogueManager.Instance.BatDauHoiThoai(nodes, () => {
                        if (GameManager.Instance != null && !GameManager.Instance.daNhanTienTuMe)
                        {
                            GameManager.Instance.daNhanTienTuMe = true; // Set flag BEFORE adding money
                            GameManager.Instance.ThemTien(1000);
                            GameManager.Instance.daNhanNhiemVu1 = true;
                            GameManager.Instance.daNhanNhiemVu2 = true;
                            GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
                        }
                    });
                    RoomVillageManager.Instance.missionActive = true;
                    GameManager.Instance.daNhanNhiemVuQuetSan = true;
                    GameManager.Instance.isVillagePhase = true; 
                    GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
                    GameManager.Instance.OnThongBao?.Invoke("[ NHIỆM VỤ ] Quét lá và mang mai về.");
                }
                else if (hoanThanhNgay29)
                {
                    // Người chơi đã làm hết mọi thứ ngày 29 (cả ở chợ lẫn ở nhà) trước khi báo mẹ → chuyển luôn sang ngày 30
                    var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Con chu đáo quá, mọi thứ cho ngày 29 Tết mẹ nhờ đều xong cả rồi." },
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Con đi nghỉ sớm đi, mai 30 Tết còn nhiều việc lắm đó." }
                    };
                    DialogueManager.Instance.BatDauHoiThoai(nodes, () => {
                        if (CutsceneManager.Instance != null)
                        {
                            CutsceneManager.Instance.PlayCutscene("video_boc_lic", () => {
                                GameManager.Instance.currentDay = GameManager.TetDay.Day30;
                                GameManager.Instance.OnThongBao?.Invoke("Ngày 29 trôi qua... Sáng 30 Tết nhộn nhịp đã đến!");
                                GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
                            });
                        }
                        else
                        {
                            GameManager.Instance.currentDay = GameManager.TetDay.Day30;
                            GameManager.Instance.OnThongBao?.Invoke("Ngày 29 trôi qua... Sáng 30 Tết nhộn nhịp đã đến!");
                            GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
                        }
                    });
                }
            }
            else // missionActive == true (đã chính thức nhận việc ngày 29)
            {
                if (!altarCleaned)
                {
                    var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Con lau bàn thờ xong chưa?" }
                    };
                    DialogueManager.Instance.BatDauHoiThoai(nodes);
                }
                else if (!yardSwept || !maiDone)
                {
                    string missing = "";
                    if (!yardSwept) missing += " quét sân";
                    if (!maiDone) missing += (missing != "" ? " và" : "") + " mang mai về";
                    
                    var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = $"Con {missing} giúp mẹ nhé. Bàn thờ lau sạch rồi đó." }
                    };

                    // MỚI: Nếu chưa nhận tiền thì mẹ đưa luôn lúc này
                    if (GameManager.Instance != null && !GameManager.Instance.daNhanTienTuMe)
                    {
                        nodes.Add(new DialogueManager.DialogueNode { 
                            tenNguoiNoi = tenNPC, 
                            noiDung = "Mẹ đưa cho 1 triệu ra chợ sắm cây mai thiệt đẹp mang về nghe con." 
                        });

                        DialogueManager.Instance.BatDauHoiThoai(nodes, () => {
                            if (GameManager.Instance != null && !GameManager.Instance.daNhanTienTuMe)
                            {
                                GameManager.Instance.daNhanTienTuMe = true;
                                GameManager.Instance.ThemTien(1000);
                                GameManager.Instance.daNhanNhiemVu1 = true;
                                GameManager.Instance.daNhanNhiemVu2 = true;
                                GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
                            }
                        });
                    }
                    else
                    {
                        DialogueManager.Instance.BatDauHoiThoai(nodes);
                    }
                }
                else // hoanThanhNgay29 == true
                {
                    // Hoàn thành hết Day 29 (cả chợ lẫn ở nhà)
                    var nodes = new System.Collections.Generic.List<DialogueManager.DialogueNode> {
                        new DialogueManager.DialogueNode { tenNguoiNoi = tenNPC, noiDung = "Mọi thứ đã sẵn sàng rồi. Con đi nghỉ đi, mai 30 Tết rồi, nhiều việc lắm đó!" }
                    };
                    DialogueManager.Instance.BatDauHoiThoai(nodes, () => {
                        if (CutsceneManager.Instance != null)
                        {
                            CutsceneManager.Instance.PlayCutscene("video_boc_lic", () => {
                                // Chuyển sang Ngày 30
                                GameManager.Instance.currentDay = GameManager.TetDay.Day30;
                                GameManager.Instance.OnThongBao?.Invoke("Ngày 29 trôi qua... Sáng 30 Tết nhộn nhịp đã đến!");
                                GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
                            });
                        }
                        else
                        {
                            // Chuyển sang Ngày 30
                            GameManager.Instance.currentDay = GameManager.TetDay.Day30;
                            GameManager.Instance.OnThongBao?.Invoke("Ngày 29 trôi qua... Sáng 30 Tết nhộn nhịp đã đến!");
                            GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
                        }
                        // Ở đây có thể gọi FadeController để chuyển cảnh hoặc đổi visual
                    });
                }
            }
        }
        KetThucTuongTac();
    }
}
