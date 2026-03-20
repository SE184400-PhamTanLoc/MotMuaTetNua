using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// PlayerInteraction - Gắn vào Player để phát hiện và tương tác với NPC
/// Sử dụng Input System mới
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("=== CÀI ĐẶT TƯƠNG TÁC ===")]
    public float khoangCachTuongTac = 3f; // Tầm nhìn ~3m như yêu cầu
    public LayerMask npcLayer;

    [Header("=== UI ===")]
    public GameObject goiYTuongTacUI;

    private Camera _mainCamera;
    private NPCBase _npcHienTai;

    private void Start()
    {
        _mainCamera = Camera.main;
        if (goiYTuongTacUI != null)
            goiYTuongTacUI.SetActive(false);
        else
        {
            // Fallback: Tìm UI gợi ý trong Canvas
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Transform t = canvas.transform.Find("InteractionPrompt");
                if (t != null) goiYTuongTacUI = t.gameObject;
                else
                {
                    // Tìm bất kỳ object nào có tên chứa Interaction hoặc GoiY
                    foreach (Transform child in canvas.transform)
                    {
                        if (child.name.ToLower().Contains("interaction") || child.name.ToLower().Contains("goiy"))
                        {
                            goiYTuongTacUI = child.gameObject;
                            break;
                        }
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.DangHoiThoai)
        {
            AnGoiY();
            return;
        }

        KiemTraNPC();
        XuLyTuongTac();
    }

    private void KiemTraNPC()
    {
        // Sử dụng Physics.OverlapSphere để kiểm tra các collider xung quanh trong layer NPC.
        // Bán kính quét rộng hơn một chút theo chiều dọc để bắt được các object có pivot ở trên cao (cổng, bảng hiệu...),
        // nhưng khi so sánh khoảng cách thì chỉ tính theo mặt phẳng ngang (XZ) để đúng với "tầm 3m" quanh người chơi.
        float physicsRadius = khoangCachTuongTac + 2f;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, physicsRadius, npcLayer);
        
        if (hitColliders.Length > 0 && Time.frameCount % 60 == 0) 
            Debug.Log($"[PlayerInteraction] Thấy {hitColliders.Length} vật thể trên Layer 6 xung quanh.");

        NPCBase npcGanNhat = null;
        float khoangCachGanNhat = khoangCachTuongTac;

        foreach (var hitCollider in hitColliders)
        {
            NPCBase npc = hitCollider.GetComponentInParent<NPCBase>();
            if (npc == null || !npc.coTheTuongTac) 
            {
                if (npc == null && Time.frameCount % 60 == 0)
                    Debug.Log($"[PlayerInteraction] Vật thể {hitCollider.name} không có NPCBase.");
                continue;
            }

            // Khoảng cách theo mặt phẳng ngang (bỏ qua chênh lệch độ cao)
            Vector2 playerXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 npcXZ = new Vector2(npc.transform.position.x, npc.transform.position.z);
            float khoangCach = Vector2.Distance(playerXZ, npcXZ);
            if (khoangCach < khoangCachGanNhat)
            {
                khoangCachGanNhat = khoangCach;
                npcGanNhat = npc;
            }
        }

        if (npcGanNhat != null)
        {
            if (_npcHienTai != npcGanNhat)
            {
                _npcHienTai = npcGanNhat;
                Debug.Log($"[PlayerInteraction] 🎯 Phát hiện NPC mới: {npcGanNhat.tenNPC} ở khoảng cách {khoangCachGanNhat:F2}");
                HienGoiY(npcGanNhat.tenNPC);

                // Đồng thời bắn một thông báo xanh ở trên (giống các chỗ khác)
                if (GameManager.Instance != null)
                {
                    string msg = null;

                    if (npcGanNhat is YardInteract)
                    {
                        msg = "Nhấn <color=yellow><b>E</b></color> để quét sân cùng Cái Chổi";
                    }
                    else if (npcGanNhat is DoorEnter || npcGanNhat is DoorExit)
                    {
                        // Dùng đúng câu chữ theo cấu hình tenNPC + hanhDongTuongTac
                        msg = $"Nhấn <color=yellow><b>E</b></color> để <b>{npcGanNhat.hanhDongTuongTac} {npcGanNhat.tenNPC}</b>";
                    }
                    else if ((npcGanNhat is OngNoiInteract || npcGanNhat is NguoiBoInteract) && GameManager.Instance.currentDay == GameManager.TetDay.Mung1)
                    {
                         msg = $"Nhấn <color=yellow><b>E</b></color> để <b>{npcGanNhat.hanhDongTuongTac}</b> với <b>{npcGanNhat.tenNPC}</b>";
                    }

                    if (!string.IsNullOrEmpty(msg))
                    {
                        GameManager.Instance.HienThongBao(msg);
                    }
                }
            }
        }
        else
        {
            if (_npcHienTai != null)
            {
                _npcHienTai = null;
                AnGoiY();
            }
        }
    }

    private void XuLyTuongTac()
    {
        if (_npcHienTai != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            _npcHienTai.BatDauTuongTac();
            AnGoiY();
        }
    }

    private void HienGoiY(string tenNPC)
    {
        // Với mọi cửa (vào nhà / ra sân / cổng làng đi chợ), chỉ dùng banner xanh ở trên,
        // KHÔNG hiện ô đen giữa màn hình.
        if (_npcHienTai is DoorEnter || _npcHienTai is DoorExit)
        {
            return;
        }

        if (goiYTuongTacUI != null)
        {
            goiYTuongTacUI.SetActive(true);
            var tmp = goiYTuongTacUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmp != null && _npcHienTai != null)
            {
                // Sử dụng màu vàng đậm và outline đen để dễ đọc trên nền sáng/tối
                if (tenNPC.Contains("ra sân") || tenNPC.Contains("vào nhà") || tenNPC.Contains("Cửa")
                    || tenNPC.Contains("chợ") || tenNPC.Contains("làng"))
                {
                    tmp.text = $"Nhấn <color=yellow><b>E</b></color> để <b>{_npcHienTai.hanhDongTuongTac} {tenNPC}</b>";
                }
                else
                {
                    tmp.text = $"Nhấn <color=yellow><b>E</b></color> để <b>{_npcHienTai.hanhDongTuongTac}</b> với <b>{tenNPC}</b>";
                }
            }
        }
    }


    private void AnGoiY()
    {
        if (goiYTuongTacUI != null)
            goiYTuongTacUI.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, khoangCachTuongTac);
    }
}
