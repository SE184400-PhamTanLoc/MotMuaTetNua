using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Biến thể DoorExit có fade đen trước khi chuyển scene.
/// Dùng cho flow mới, không ảnh hưởng DoorExit cũ ở scene khác.
/// </summary>
public class DoorExitWithFade : DoorExit
{
    [Header("Fade Transition")]
    [Tooltip("Ảnh full-screen màu đen để fade (UI Overlay).")]
    public Image fadeImage;
    [Tooltip("Thời gian fade sang đen trước khi load scene.")]
    public float fadeToBlackDuration = 1f;
    [Tooltip("Giữ màn hình đen một nhịp trước khi load scene.")]
    public float holdBlackDuration = 0.1f;
    [Tooltip("Dùng unscaled time để fade vẫn chạy khi timeScale thay đổi.")]
    public bool useUnscaledTime = true;

    [Header("Optional Safety")]
    [Tooltip("Disable các control trong lúc chuyển cảnh để tránh input lặp.")]
    public Behaviour[] controlsToDisableDuringTransition;

    private bool isTransitioning;

    protected override void Start()
    {
        base.Start();
        SetFadeAlpha(0f, false);
    }

    protected override void OnTuongTac()
    {
        if (isTransitioning)
            return;

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;
        coTheTuongTac = false;

        SetControlsEnabled(false);

        if (fadeImage != null && fadeToBlackDuration > 0f)
            yield return FadeAlphaRoutine(0f, 1f, fadeToBlackDuration);
        else
            SetFadeAlpha(1f, true);

        if (holdBlackDuration > 0f)
            yield return Wait(holdBlackDuration);

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("[DoorExitWithFade] sceneName đang rỗng, không thể load scene.");
            SetControlsEnabled(true);
            coTheTuongTac = true;
            isTransitioning = false;
            KetThucTuongTac();
            yield break;
        }

        SceneManager.LoadScene(sceneName);
        KetThucTuongTac();
    }

    private void SetControlsEnabled(bool enabled)
    {
        if (controlsToDisableDuringTransition == null) return;
        for (int i = 0; i < controlsToDisableDuringTransition.Length; i++)
        {
            if (controlsToDisableDuringTransition[i] != null)
                controlsToDisableDuringTransition[i].enabled = enabled;
        }
    }

    private IEnumerator FadeAlphaRoutine(float from, float to, float duration)
    {
        if (fadeImage == null)
            yield break;

        SetFadeAlpha(from, true);
        float elapsed = 0f;
        float clampedDuration = Mathf.Max(0.0001f, duration);

        while (elapsed < clampedDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / clampedDuration);
            SetFadeAlpha(Mathf.Lerp(from, to, t), true);
            yield return null;
        }

        SetFadeAlpha(to, true);
    }

    private IEnumerator Wait(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }

    private void SetFadeAlpha(float alpha, bool forceVisible)
    {
        if (fadeImage == null) return;
        if (forceVisible && !fadeImage.gameObject.activeInHierarchy)
            fadeImage.gameObject.SetActive(true);

        Color c = fadeImage.color;
        c.a = Mathf.Clamp01(alpha);
        fadeImage.color = c;

        if (!forceVisible && Mathf.Approximately(c.a, 0f))
            fadeImage.gameObject.SetActive(false);
    }
}
