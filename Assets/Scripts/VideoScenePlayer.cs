using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Player đơn giản cho scene video ending:
/// - Vào scene: fade từ đen ra video
/// - Hết video: fade về đen và load scene tiếp theo (optional)
/// </summary>
public class VideoScenePlayer : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;
    public GameObject videoRoot;
    public Image fadeImage;
    [Tooltip("Tự tìm FadeImage nếu chưa gán.")]
    public bool autoFindFadeImage = true;
    [Tooltip("Ép FadeImage nằm trên cùng để luôn che video khi fade.")]
    public bool forceFadeImageOnTop = true;

    [Header("Flow")]
    public bool autoPlayOnStart = true;
    public bool restartFromBeginning = true;
    public bool loadSceneAfterVideo = false;
    public string nextSceneName = "RoomVillage";
    [Tooltip("Bắt đầu fade out trước khi video kết thúc.")]
    public bool fadeOutBeforeVideoEnds = true;
    [Tooltip("Số giây trước khi video kết thúc để bắt đầu fade out.")]
    public float fadeOutLeadTime = 1f;

    [Header("External Audio (MP3)")]
    [Tooltip("Bật để dùng nhạc rời thay cho audio gốc của video.")]
    public bool useExternalAudioTrack = false;
    [Tooltip("AudioSource phát nhạc rời. Nếu để trống sẽ tự tìm/tạo.")]
    public AudioSource externalAudioSource;
    [Tooltip("Clip MP3 sẽ phát cùng video.")]
    public AudioClip externalAudioClip;
    [Range(0f, 1f)] public float externalAudioVolume = 1f;
    [Tooltip("Tắt audio gốc của video khi dùng nhạc rời.")]
    public bool muteVideoOriginalAudio = true;
    [Tooltip("Khi dùng nhạc rời: thời điểm fade out sẽ bám theo thời điểm kết thúc nhạc.")]
    public bool fadeOutFollowExternalAudioEnd = true;
    [Tooltip("Chỉ chuyển scene sau khi nhạc rời phát xong.")]
    public bool waitForExternalAudioToEndBeforeSceneChange = true;

    [Header("Fade")]
    public float fadeFromBlackDuration = 0.75f;
    public float fadeToBlackDuration = 1f;
    public bool useUnscaledTime = true;
    [Tooltip("Giữ màn che lúc mới vào scene trước khi fade in.")]
    public float startOverlayHoldDuration = 0.2f;
    [Tooltip("Số frame chờ thêm trước khi bắt đầu fade in.")]
    public int startFrameDelay = 2;

    [Header("Skip")]
    public bool allowSkip = false;
    public KeyCode skipKey = KeyCode.Space;

    private bool videoFinished;
    private bool transitionStarted;
    private bool transitionCompleted;
    private bool externalAudioStarted;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = FindFirstObjectByType<VideoPlayer>();
        if (autoFindFadeImage && fadeImage == null)
            fadeImage = FindBestFadeImage();
        EnsureExternalAudioSource();
        ForceMuteOriginalVideoAudioIfNeeded();

        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;

        if (videoRoot != null)
            videoRoot.SetActive(true);

        EnsureFadeHierarchyActive();
        EnsureFadeOnTop();
        SetFadeAlpha(1f, true);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void Start()
    {
        StartCoroutine(PlayRoutine());
    }

    private void Update()
    {
        if (!allowSkip || transitionStarted) return;
        if (Input.GetKeyDown(skipKey))
            videoFinished = true;
    }

    private IEnumerator PlayRoutine()
    {
        if (videoPlayer == null)
        {
            Debug.LogWarning("[VideoScenePlayer] Thiếu VideoPlayer.");
            yield return StartEndTransition();
            yield break;
        }

        // Chốt khung đen trước 1 frame để tránh lộ frame đầu.
        EnsureFadeHierarchyActive();
        EnsureFadeOnTop();
        SetFadeAlpha(1f, true);
        int frameDelay = Mathf.Max(1, startFrameDelay);
        for (int i = 0; i < frameDelay; i++)
            yield return null;

        if (startOverlayHoldDuration > 0f)
            yield return WaitForSecondsRealtimeCompat(startOverlayHoldDuration);

        if (restartFromBeginning)
            videoPlayer.time = 0d;

        ForceMuteOriginalVideoAudioIfNeeded();
        StartExternalAudioIfNeeded();

        if (autoPlayOnStart)
            videoPlayer.Play();

        if (fadeFromBlackDuration > 0f)
            yield return FadeAlphaRoutine(1f, 0f, fadeFromBlackDuration);
        else
            SetFadeAlpha(0f, false);

        while (!transitionStarted)
        {
            if (ShouldStartFadeOutNow())
            {
                StartCoroutine(StartEndTransition());
                break;
            }

            if (ShouldEndByVideoNow())
            {
                StartCoroutine(StartEndTransition());
                break;
            }
            yield return null;
        }

        if (!transitionStarted)
            yield return StartEndTransition();
        else
            yield return new WaitUntil(() => transitionCompleted);
    }

    private IEnumerator StartEndTransition()
    {
        if (transitionStarted) yield break;
        transitionStarted = true;

        if (fadeToBlackDuration > 0f)
            yield return FadeAlphaRoutine(0f, 1f, fadeToBlackDuration);
        else
            SetFadeAlpha(1f, true);

        if (videoPlayer != null)
            videoPlayer.Stop();

        if (useExternalAudioTrack && waitForExternalAudioToEndBeforeSceneChange)
            yield return WaitUntilExternalAudioFinished();

        if (loadSceneAfterVideo && !string.IsNullOrWhiteSpace(nextSceneName))
            SceneManager.LoadScene(nextSceneName);

        transitionCompleted = true;
    }

    private void OnVideoFinished(VideoPlayer _)
    {
        videoFinished = true;
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
        SetFadeAlpha(to, to > 0f);
    }

    private IEnumerator WaitForSecondsRealtimeCompat(float duration)
    {
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0f, duration);
        while (elapsed < safeDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }

    private void SetFadeAlpha(float alpha, bool forceVisible)
    {
        if (fadeImage == null) return;
        if (forceVisible && !fadeImage.gameObject.activeSelf)
            fadeImage.gameObject.SetActive(true);

        Color c = fadeImage.color;
        c.a = Mathf.Clamp01(alpha);
        fadeImage.color = c;

        if (!forceVisible && Mathf.Approximately(c.a, 0f))
            fadeImage.gameObject.SetActive(false);
    }

    private void EnsureFadeHierarchyActive()
    {
        if (fadeImage == null) return;
        Transform t = fadeImage.transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf)
                t.gameObject.SetActive(true);
            t = t.parent;
        }
    }

    private void EnsureFadeOnTop()
    {
        if (!forceFadeImageOnTop || fadeImage == null) return;
        fadeImage.transform.SetAsLastSibling();
    }

    private Image FindBestFadeImage()
    {
        // Ưu tiên object tên chứa "fade".
        Image[] allImages = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < allImages.Length; i++)
        {
            Image img = allImages[i];
            if (img == null) continue;
            string n = img.gameObject.name;
            if (!string.IsNullOrEmpty(n) && n.ToLower().Contains("fade"))
                return img;
        }

        // Fallback: lấy image đầu tiên có scale lớn/fullscreen thường dùng làm overlay.
        return allImages.Length > 0 ? allImages[allImages.Length - 1] : null;
    }

    private bool TryGetRemainingVideoSeconds(out double remainingSeconds)
    {
        remainingSeconds = double.MaxValue;
        if (videoPlayer == null) return false;

        double duration = videoPlayer.length;
        if (duration <= 0d && videoPlayer.frameCount > 0 && videoPlayer.frameRate > 0f)
            duration = videoPlayer.frameCount / videoPlayer.frameRate;
        if (duration <= 0d) return false;

        double current = videoPlayer.time;
        if (current < 0d) current = 0d;
        remainingSeconds = duration - current;
        return remainingSeconds >= 0d;
    }

    private void EnsureExternalAudioSource()
    {
        if (externalAudioSource != null) return;
        externalAudioSource = GetComponent<AudioSource>();
        if (externalAudioSource == null && useExternalAudioTrack)
            externalAudioSource = gameObject.AddComponent<AudioSource>();
        if (externalAudioSource == null) return;

        externalAudioSource.playOnAwake = false;
        externalAudioSource.loop = false;
        externalAudioSource.spatialBlend = 0f;
    }

    private void ForceMuteOriginalVideoAudioIfNeeded()
    {
        if (videoPlayer == null) return;
        if (!useExternalAudioTrack || !muteVideoOriginalAudio) return;

        // 1) Khóa output mode về None (không route audio ra hệ thống).
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        // 2) Mute toàn bộ direct audio track (nếu có).
        ushort trackCount = videoPlayer.audioTrackCount;
        for (ushort i = 0; i < trackCount; i++)
        {
            videoPlayer.SetDirectAudioMute(i, true);
            videoPlayer.SetDirectAudioVolume(i, 0f);
        }

        // 3) Nếu trước đó video route qua AudioSource thì ép volume về 0 luôn.
        for (ushort i = 0; i < trackCount; i++)
        {
            AudioSource target = videoPlayer.GetTargetAudioSource(i);
            if (target == null) continue;
            target.volume = 0f;
            if (target.isPlaying)
                target.Stop();
        }
    }

    private void StartExternalAudioIfNeeded()
    {
        if (!useExternalAudioTrack) return;
        if (externalAudioSource == null) return;
        if (externalAudioClip == null) return;

        externalAudioSource.clip = externalAudioClip;
        externalAudioSource.volume = Mathf.Clamp01(externalAudioVolume);
        externalAudioSource.time = 0f;
        externalAudioSource.Play();
        externalAudioStarted = true;
    }

    private bool ShouldStartFadeOutNow()
    {
        if (transitionStarted) return false;

        if (useExternalAudioTrack && fadeOutFollowExternalAudioEnd && TryGetRemainingExternalAudioSeconds(out double remainingAudio))
            return remainingAudio <= Mathf.Max(0.01f, fadeToBlackDuration);

        if (fadeOutBeforeVideoEnds && TryGetRemainingVideoSeconds(out double remainingVideo))
            return remainingVideo <= Mathf.Max(0.01f, fadeOutLeadTime);

        return false;
    }

    private bool ShouldEndByVideoNow()
    {
        if (!videoFinished) return false;
        if (useExternalAudioTrack && waitForExternalAudioToEndBeforeSceneChange)
            return false;
        return true;
    }

    private bool TryGetRemainingExternalAudioSeconds(out double remainingSeconds)
    {
        remainingSeconds = double.MaxValue;
        if (!useExternalAudioTrack) return false;
        if (externalAudioClip == null) return false;
        if (externalAudioSource == null) return false;

        // Nếu đã bắt đầu phát và source đã dừng -> coi như nhạc đã kết thúc.
        if (externalAudioStarted && !externalAudioSource.isPlaying)
        {
            remainingSeconds = 0d;
            return true;
        }

        int clipSamples = externalAudioClip.samples;
        int sampleRate = externalAudioClip.frequency;
        if (clipSamples > 0 && sampleRate > 0)
        {
            int currentSamples = Mathf.Max(0, externalAudioSource.timeSamples);
            double remainingBySamples = (clipSamples - currentSamples) / (double)sampleRate;
            remainingSeconds = remainingBySamples < 0d ? 0d : remainingBySamples;
            return true;
        }

        float currentTime = Mathf.Max(0f, externalAudioSource.time);
        remainingSeconds = Mathf.Max(0f, externalAudioClip.length - currentTime);
        return true;
    }

    private IEnumerator WaitUntilExternalAudioFinished()
    {
        if (externalAudioClip == null) yield break;
        float timeout = Mathf.Max(1f, externalAudioClip.length + 2f);
        float elapsed = 0f;
        while (elapsed < timeout)
        {
            if (!TryGetRemainingExternalAudioSeconds(out double remaining) || remaining <= 0.02d)
                yield break;

            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }
}
