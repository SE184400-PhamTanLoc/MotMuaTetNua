using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public enum PortalType
{
    ToMarket,
    ToVillage
}

public class VillageMarketTransition : MonoBehaviour
{
    public PortalType portalType;
    public string sceneNameToLoad;
    
    [Header("UI Message Settings")]
    [Tooltip("Text object to show hint (optional, có thể để trống để chỉ dùng banner xanh)")]
    public GameObject pressTextUI;
    [Tooltip("Key to press to interact (giữ lại cho tương thích, mặc định E)")]
    public KeyCode interactKey = KeyCode.E;

    private bool playerNear = false;

    void Start()
    {
        if (pressTextUI != null)
        {
            pressTextUI.SetActive(false);
        }
    }

    void Update()
    {
        if (!playerNear) return;

        // Hỗ trợ cả Input System mới (Keyboard.current) và Input cũ
        bool pressedE = (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                        || Input.GetKeyDown(KeyCode.E)
                        || Input.GetKeyDown(interactKey);

        if (pressedE)
        {
            TryTransition();
        }
    }

    private void TryTransition()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null! Loading scene directly as fallback.");
            SceneManager.LoadScene(sceneNameToLoad);
            return;
        }

        if (portalType == PortalType.ToMarket)
        {
            // Đi từ Làng ra Chợ
            if (GameManager.Instance.daKetThucDiCho)
            {
                // Đã mua xong đồ, không cho ra chợ nữa
                GameManager.Instance.OnThongBao?.Invoke("Mọi thứ đã chuẩn bị xong, hãy ở nhà đón Tết cùng mẹ nhé!");
            }
            else
            {
                // Chưa đi hoặc đi chưa xong, cho qua
                SceneManager.LoadScene(sceneNameToLoad);
            }
        }
        else if (portalType == PortalType.ToVillage)
        {
            // Đi từ Chợ về Làng
            if (GameManager.Instance.DaXongHetNhiemVu())
            {
                // Hoàn thành hết nhiệm vụ ở chợ -> Đánh dấu kết thúc việc đi chợ -> Cho về Làng
                GameManager.Instance.daKetThucDiCho = true;
                SceneManager.LoadScene(sceneNameToLoad);
            }
            else
            {
                // Chưa xong nhiệm vụ, bắt quay lại mua tiếp
                GameManager.Instance.OnThongBao?.Invoke("Cậu chưa mua đủ đồ Tết mà! Hãy kiểm tra lại danh sách nhiệm vụ nhé.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;

            // Ẩn UI cũ nếu có, ưu tiên dùng banner xanh của GameManager
            if (pressTextUI != null) pressTextUI.SetActive(false);

            if (GameManager.Instance != null)
            {
                string msg = null;
                var day = GameManager.Instance.currentDay;

                if (portalType == PortalType.ToMarket)
                {
                    // Tùy ngày mà gợi ý câu khác nhau cho hợp nhiệm vụ
                    if (day == GameManager.TetDay.Day29)
                    {
                        msg = "Nhấn <color=yellow><b>E</b></color> để ra chợ Tết mua mai và chuẩn bị đồ Tết.";
                    }
                    else
                    {
                        msg = "Nhấn <color=yellow><b>E</b></color> để ra chợ Tết.";
                    }
                }
                else if (portalType == PortalType.ToVillage)
                {
                    msg = "Nhấn <color=yellow><b>E</b></color> để quay lại làng.";
                }

                if (!string.IsNullOrEmpty(msg))
                    GameManager.Instance.HienThongBao(msg);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            if (pressTextUI != null) pressTextUI.SetActive(false);
        }
    }
}
