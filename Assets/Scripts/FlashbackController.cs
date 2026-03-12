using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Quản lý hiệu ứng Flashback hiện ảnh gia đình
/// </summary>
public class FlashbackController : MonoBehaviour
{
    public static FlashbackController Instance { get; private set; }

    [Header("=== UI REFERENCES ===")]
    public GameObject flashbackPanel;
    public Image flashbackImage;
    public CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartFlashback()
    {
        if (flashbackPanel != null)
        {
            flashbackPanel.SetActive(true);
            
            // Nếu component này bị tắt hoặc GameObject không active, 
            // ta có thể nhờ RoomVillageManager chạy giùm Coroutine
            if (RoomVillageManager.Instance != null)
            {
                RoomVillageManager.Instance.StartCoroutine(FlashbackRoutine());
            }
            else
            {
                StartCoroutine(FlashbackRoutine());
            }
        }
        else
        {
            Debug.LogWarning("[FlashbackController] flashbackPanel is null!");
        }
    }

    private IEnumerator FlashbackRoutine()
    {
        // Đã SetActive ở trên rồi
        
        // Hiện text nếu cần
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnThongBao?.Invoke("Một ký ức hiện về...");
        }

        // Fade In
        float elapsed = 0f;
        float duration = 2f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
            yield return null;
        }

        // Chờ 4 giây
        yield return new WaitForSeconds(4f);

        // Fade Out
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / duration);
            yield return null;
        }

        flashbackPanel.SetActive(false);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnThongBao?.Invoke("Cảm thấy thật ấm áp.");
            GameManager.Instance.OnNhiemVuHoanThanh?.Invoke();
        }
    }
}
