using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Scene bắt đầu gameplay (thường là IntroScene).")]
    public string gameplaySceneName = "IntroScene";

    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    
    [Header("Main Panel Slide")]
    [Tooltip("Bật hiệu ứng slide khi hiện MainPanel.")]
    public bool enableMainPanelSlide = true;
    [Tooltip("Khoảng cách bắt đầu từ bên trái (px).")]
    public float mainPanelSlideDistance = 900f;
    [Tooltip("Thời gian slide (giây).")]
    public float mainPanelSlideDuration = 0.55f;
    [Tooltip("Đợi 1 frame sau khi bật panel để layout ổn định rồi mới animate.")]
    public bool waitOneFrameBeforeSlide = true;

    private RectTransform mainPanelRect;
    private Vector2 mainPanelTargetAnchoredPos;
    private Coroutine mainPanelSlideCoroutine;

    private void Start()
    {
        EnsureMenuInputState();

        // Đảm bảo manager audio tồn tại từ menu.
        AudioSettingsManager.InstanceOrCreate();

        if (mainPanel != null)
        {
            mainPanelRect = mainPanel.GetComponent<RectTransform>();
            if (mainPanelRect != null)
            {
                mainPanelTargetAnchoredPos = mainPanelRect.anchoredPosition;
            }
        }
        
        // Tránh trường hợp frame đầu hiện panel ngay tại vị trí đích
        // khiến hiệu ứng slide không thấy.
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }
        StartCoroutine(ShowMainPanelOnNextFrame());
    }

    private void OnEnable()
    {
        EnsureMenuInputState();
    }

    public void OnClickPlay()
    {
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogWarning("StartMenuController: gameplaySceneName đang rỗng.");
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnClickOpenSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnClickCloseSettings()
    {
        ShowMainPanel();
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        PlayMainPanelSlide();
    }

    private void PlayMainPanelSlide()
    {
        if (!enableMainPanelSlide || mainPanelRect == null)
        {
            return;
        }

        if (mainPanelSlideCoroutine != null)
        {
            StopCoroutine(mainPanelSlideCoroutine);
        }

        mainPanelSlideCoroutine = StartCoroutine(SlideMainPanelInCoroutine());
    }

    private IEnumerator SlideMainPanelInCoroutine()
    {
        if (waitOneFrameBeforeSlide)
        {
            yield return null;
        }

        // Lấy target tại thời điểm animate để tránh bị "teleport"
        // khi layout của UI vừa rebuild xong.
        mainPanelTargetAnchoredPos = mainPanelRect.anchoredPosition;
        Vector2 startPos = mainPanelTargetAnchoredPos + Vector2.left * Mathf.Abs(mainPanelSlideDistance);
        float duration = Mathf.Max(0.01f, mainPanelSlideDuration);
        float elapsed = 0f;

        mainPanelRect.anchoredPosition = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease out mượt hơn
            float eased = Mathf.SmoothStep(0f, 1f, t);
            mainPanelRect.anchoredPosition = Vector2.LerpUnclamped(startPos, mainPanelTargetAnchoredPos, eased);
            yield return null;
        }

        mainPanelRect.anchoredPosition = mainPanelTargetAnchoredPos;
        mainPanelSlideCoroutine = null;
    }
    
    private IEnumerator ShowMainPanelOnNextFrame()
    {
        yield return null;
        EnsureMenuInputState();
        ShowMainPanel();
    }

    private void EnsureMenuInputState()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
