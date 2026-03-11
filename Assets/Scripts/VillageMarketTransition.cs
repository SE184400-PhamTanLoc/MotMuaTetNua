using UnityEngine;
using UnityEngine.SceneManagement;

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
    [Tooltip("Text object to show 'Press O to enter' or similar")]
    public GameObject pressTextUI;
    [Tooltip("Key to press to interact")]
    public KeyCode interactKey = KeyCode.O;

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
        if (playerNear && Input.GetKeyDown(interactKey))
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
            if (pressTextUI != null)
            {
                pressTextUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            if (pressTextUI != null)
            {
                pressTextUI.SetActive(false);
            }
        }
    }
}
