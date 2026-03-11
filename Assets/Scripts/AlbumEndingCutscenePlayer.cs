using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class AlbumEndingCutscenePlayer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Canvas chứa RawImage hiển thị video (ban đầu để inactive).")]
    public GameObject cutsceneCanvas;
    [Tooltip("VideoPlayer đã setup clip + render texture + audio source.")]
    public VideoPlayer videoPlayer;
    [Tooltip("Optional: dùng để đóng album nếu không chuyển scene.")]
    public AlbumFocusController albumFocusController;

    [Header("Flow")]
    [Tooltip("Bật để tự chuyển scene khi video kết thúc.")]
    public bool loadSceneAfterCutscene = true;
    [Tooltip("Tên scene sẽ load sau cutscene.")]
    public string nextSceneName = "RoomScene";
    [Tooltip("Nếu false thì giữ nguyên timeline playback speed.")]
    public bool forceNormalPlaybackSpeed = true;
    [Tooltip("Phát video từ đầu mỗi lần trigger.")]
    public bool restartFromBeginning = true;

    private bool isPlaying;

    /// <summary>
    /// Gọi hàm này từ UnityEvent onAlbumReadingCompleted trong AlbumFocusController.
    /// </summary>
    public void PlayCutsceneFromAlbumEnd()
    {
        if (isPlaying) return;
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        isPlaying = true;

        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.SetActive(true);
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
        videoPlayer.Play();

        while (!finished)
        {
            yield return null;
        }

        videoPlayer.loopPointReached -= OnLoopPointReached;

        yield return EndFlow();
    }

    private IEnumerator EndFlow()
    {
        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.SetActive(false);
        }

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

        isPlaying = false;
        yield return null;
    }
}
