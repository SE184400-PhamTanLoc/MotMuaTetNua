using UnityEngine;
using System.Collections;
using TMPro;

public class NewYearsEveManager : MonoBehaviour
{
    public static NewYearsEveManager Instance;

    [Header("UI")]
    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;

    [Header("FX")]
    public GameObject fireworksEffect; // Prefab or object to enable
    public AudioSource fireworksSound;

    private void Awake()
    {
        Instance = this;
        if (countdownPanel != null) countdownPanel.SetActive(false);
    }

    public void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        // 1. Fade to night if not already
        GameManager.Instance.HienThongBao("Thời khắc giao thừa đang đến gần...");
        yield return new WaitForSeconds(2f);

        if (countdownPanel != null) countdownPanel.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null) countdownText.text = i.ToString();
            // Có thể thêm tiếng kêu bíp
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null) countdownText.text = "CHÚC MỪNG NĂM MỚI!";
        
        // 2. Fireworks
        if (fireworksEffect != null) fireworksEffect.SetActive(true);
        if (fireworksSound != null) fireworksSound.Play();

        yield return new WaitForSeconds(5f);

        // 2.5 Video Pháo Hoa
        if (CutsceneManager.Instance != null)
        {
            float holdTime = 0;
            CutsceneManager.Instance.PlayCutscene("video_phao_hoa", () => {
                holdTime = -1; // Flag complete
            });
            yield return new WaitUntil(() => holdTime < 0);
        }

        // 3. Transition to Mùng 1
        GameManager.Instance.currentDay = GameManager.TetDay.Mung1;
        GameManager.Instance.HienThongBao("Sáng Mùng 1 Tết... Trời đất giao hòa.");
        if (countdownPanel != null) countdownPanel.SetActive(false);
        GameManager.Instance.OnNhiemVuThayDoi?.Invoke();

        // Reveal Mùng 1 visuals
        // Đổi scene hoặc bật NPCs Mùng 1
    }
}
