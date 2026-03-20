using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FamilyPhotoMinigame : MonoBehaviour
{
    public static FamilyPhotoMinigame Instance;

    public GameObject panel;
    public TextMeshProUGUI countdownText;
    public Image flashOverlay;

    private void Awake()
    {
        // Luôn gán Instance mới nhất, AutoSetup sẽ dọn dẹp cái cũ
        Instance = this;

        if (panel != null) panel.SetActive(false);
        if (flashOverlay != null) flashOverlay.color = new Color(1, 1, 1, 0);
        if (countdownText != null)
        {
            countdownText.enableWordWrapping = false;
            countdownText.enableAutoSizing = true;
            countdownText.fontSizeMin = 20;
            countdownText.fontSizeMax = 200; 
            countdownText.alignment = TextAlignmentOptions.Center;
            countdownText.overflowMode = TextOverflowModes.Overflow;
            
            // Đảm bảo RectTransform phủ kín Panel để căn giữa chuẩn
            RectTransform rt = countdownText.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void StartMinigame()
    {
        if (panel == null)
        {
            Debug.LogError("[FamilyPhotoMinigame] Panel reference is missing!");
            return;
        }
        panel.SetActive(true);
        StartCoroutine(PhotoRoutine());
    }

    private IEnumerator PhotoRoutine()
    {
        if (GameManager.Instance == null) yield break;
        
        GameManager.Instance.HienThongBao("Cả nhà đứng vào vị trí nào! 3... 2... 1...");
        
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null) 
        {
            countdownText.text = "<color=#FFD700><b>SMILE!</b></color>";
            // Animation: Punch scale
            StartCoroutine(SmilePunchEffect());
        }
        yield return new WaitForSeconds(0.5f);

        // Flash
        if (flashOverlay != null)
        {
            float t = 0;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                flashOverlay.color = new Color(1, 1, 1, t * 10f);
                yield return null;
            }
            
            // Take photo (conceptually)
            GameManager.Instance.daChupAnhGiaDinh = true;
            GameManager.Instance.HienThongBao("Tách! Một tấm ảnh thật đẹp.");
            GameManager.Instance.OnNhiemVuThayDoi?.Invoke();

            while (t > 0)
            {
                t -= Time.deltaTime;
                flashOverlay.color = new Color(1, 1, 1, t * 2f);
                yield return null;
            }
        }
        else
        {
            // Vẫn hoàn thành nhiệm vụ dù thiếu flash
            GameManager.Instance.daChupAnhGiaDinh = true;
            GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
        }

        yield return new WaitForSeconds(2f);
        if (panel != null) panel.SetActive(false);
        
        // Ending logic
        GameManager.Instance.HienThongBao("Chúc mừng! Bạn đã hoàn thành một mùa Tết ý nghĩa bên gia đình.");
        
        // Chờ thêm 3 giây để người chơi đọc thông báo rồi chuyển cảnh Ending
        yield return new WaitForSeconds(3.5f);
        SceneManager.LoadScene("Ending");
    }

    private IEnumerator SmilePunchEffect()
    {
        if (countdownText == null) yield break;

        Vector3 originalScale = Vector3.one; // Assuming base scale is 1
        
        // Punch up
        float elapsed = 0;
        float duration = 0.15f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            countdownText.transform.localScale = originalScale * Mathf.Lerp(1f, 1.5f, t);
            yield return null;
        }

        // Settle back slightly
        elapsed = 0;
        duration = 0.1f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            countdownText.transform.localScale = originalScale * Mathf.Lerp(1.5f, 1.2f, t);
            yield return null;
        }
    }
}
