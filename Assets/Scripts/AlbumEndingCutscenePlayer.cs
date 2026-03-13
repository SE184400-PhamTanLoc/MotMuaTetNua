using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class AlbumEndingCutscenePlayer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Canvas chứa RawImage hiển thị video (ban đầu để inactive).")]
    public GameObject cutsceneCanvas;
    [Tooltip("VideoPlayer đã setup clip + render texture + audio source.")]
    public VideoPlayer videoPlayer;
    [Tooltip("RawImage đang hiển thị target texture của video (optional).")]
    public RawImage cutsceneRawImage;
    [Tooltip("Optional: dùng để đóng album nếu không chuyển scene.")]
    public AlbumFocusController albumFocusController;

    [Header("Flow")]
    [Tooltip("Bật để tự chuyển scene khi video kết thúc.")]
    public bool loadSceneAfterCutscene = true;
    [Tooltip("Tên scene sẽ load sau cutscene.")]
    public string nextSceneName = "RoomVillage";
    [Tooltip("Nếu false thì giữ nguyên timeline playback speed.")]
    public bool forceNormalPlaybackSpeed = true;
    [Tooltip("Phát video từ đầu mỗi lần trigger.")]
    public bool restartFromBeginning = true;

    private bool isPlaying;

    private void Awake()
    {
        ResetCutsceneVisual();
    }

    private void OnDestroy()
    {
        // Cleanup khi object bị hủy.
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        ClearVideoTargetTexture();
    }

    /// <summary>
    /// Gọi hàm này từ UnityEvent onAlbumReadingCompleted trong AlbumFocusController.
    /// </summary>
    public void PlayCutsceneFromAlbumEnd()
    {
        if (isPlaying) return;

        if (cutsceneCanvas != null && !cutsceneCanvas.activeInHierarchy)
        {
            cutsceneCanvas.SetActive(true);
        }

        // UnityEvent vẫn có thể gọi method trên object đang inactive,
        // nhưng MonoBehaviour inactive thì không StartCoroutine được.
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        if (!isActiveAndEnabled)
        {
            Debug.LogWarning("AlbumEndingCutscenePlayer: Không thể phát cutscene vì object/component vẫn đang inactive.");
            return;
        }

        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        isPlaying = true;

        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.SetActive(true);
        }
        if (cutsceneRawImage != null)
        {
            cutsceneRawImage.enabled = true;
            cutsceneRawImage.color = Color.white;
        }

        if (videoPlayer == null)
        {
            Debug.LogWarning("AlbumEndingCutscenePlayer: VideoPlayer chưa được gán.");
            yield return EndFlow();
            yield break;
        }

        if (forceNormalPlaybackSpeed)
        {
            videoPlayer.playbackSpeed = 1f;
        }

        if (restartFromBeginning)
        {
            videoPlayer.time = 0;
        }

        bool finished = false;
        void OnLoopPointReached(VideoPlayer _) => finished = true;

        videoPlayer.loopPointReached += OnLoopPointReached;
        videoPlayer.errorReceived += (vp, msg) => {
            Debug.LogError($"[AlbumEndingCutscenePlayer] Video error: {msg}");
            finished = true;
        };
        videoPlayer.Play();

        // Safety Timeout logic
        float duration = (float)videoPlayer.length;
        if (duration <= 0) duration = 30f; // Fallback 30s
        float timeout = duration + 5f;
        float elapsed = 0;

        while (!finished && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!finished)
        {
            Debug.LogWarning("[AlbumEndingCutscenePlayer] Safety Timeout triggered - forcing end flow.");
        }

        videoPlayer.loopPointReached -= OnLoopPointReached;

        yield return EndFlow();
    }

    private IEnumerator EndFlow()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        ClearVideoTargetTexture();

        if (loadSceneAfterCutscene)
        {
            if (string.IsNullOrWhiteSpace(nextSceneName))
            {
                Debug.LogWarning("AlbumEndingCutscenePlayer: nextSceneName đang rỗng.");
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else if (albumFocusController != null)
        {
            albumFocusController.CloseAlbum();
        }

        if (cutsceneCanvas != null && !loadSceneAfterCutscene)
        {
            cutsceneCanvas.SetActive(false);
        }

        isPlaying = false;
        yield return null;
    }

    private void ResetCutsceneVisual()
    {
        isPlaying = false;
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0d;
        }

        ClearVideoTargetTexture();

        if (cutsceneRawImage != null)
        {
            cutsceneRawImage.color = Color.clear;
            cutsceneRawImage.enabled = false;
        }

        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.SetActive(false);
        }
    }

    private void ClearVideoTargetTexture()
    {
        if (videoPlayer == null || videoPlayer.targetTexture == null) return;

        RenderTexture rt = videoPlayer.targetTexture;
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = prev;
    }
}
