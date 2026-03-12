using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class FamilyPhotoMinigame : MonoBehaviour
{
    public static FamilyPhotoMinigame Instance;

    public GameObject panel;
    public TextMeshProUGUI countdownText;
    public Image flashOverlay;

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
        if (flashOverlay != null) flashOverlay.color = new Color(1, 1, 1, 0);
    }

    public void StartMinigame()
    {
        panel.SetActive(true);
        StartCoroutine(PhotoRoutine());
    }

    private IEnumerator PhotoRoutine()
    {
        GameManager.Instance.HienThongBao("Cả nhà đứng vào vị trí nào! 3... 2... 1...");
        
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null) countdownText.text = "SMILE!";
        yield return new WaitForSeconds(0.5f);

        // Flash
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

        yield return new WaitForSeconds(2f);
        panel.SetActive(false);
        
        // Ending logic
        GameManager.Instance.HienThongBao("Chúc mừng! Bạn đã hoàn thành một mùa Tết ý nghĩa bên gia đình.");
    }
}
