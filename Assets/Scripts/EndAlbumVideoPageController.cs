using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Điều khiển video theo trang trong End_Album:
/// - Khi lật tới trang có video -> tự Play.
/// - Khi rời trang -> Pause.
/// - Khi video chạy hết -> giữ frame cuối và dừng.
/// </summary>
public class EndAlbumVideoPageController : MonoBehaviour
{
    [Serializable]
    public class PageVideoEntry
    {
        [Tooltip("Số trang hiển thị (đếm từ 1).")]
        public int visiblePageNumber = 1;

        [Tooltip("VideoPlayer chính (ví dụ mặt Front).")]
        public VideoPlayer videoPlayer;
        [Tooltip("VideoPlayer phụ (ví dụ mặt Back). Để trống nếu chỉ dùng 1 video.")]
        public VideoPlayer secondaryVideoPlayer;

        [Tooltip("Bật để luôn phát lại từ đầu khi quay lại trang.")]
        public bool restartFromBeginningOnShow = true;

        [Tooltip("Giữ frame cuối khi video kết thúc.")]
        public bool holdLastFrameWhenFinished = true;

        [Tooltip("Transform dùng để kiểm tra trang có đang trong tầm nhìn camera không (thường gán CanvasFront/RawImage của trang).")]
        public Transform visibilityTarget;
        [Tooltip("Visibility target cho video phụ (ví dụ CanvasBack).")]
        public Transform secondaryVisibilityTarget;
    }

    [Header("Album Page Source")]
    [Tooltip("Nguồn trạng thái lật tờ 2..6.")]
    public AlbumPageFlipController pageFlipController;

    [Header("Video Mapping")]
    [Tooltip("Map trang -> VideoPlayer. Ví dụ trang 2,3,4...")]
    public PageVideoEntry[] pageVideos;

    [Header("Debug")]
    public bool debugLog;

    [Header("Visibility Gating")]
    [Tooltip("Chỉ render/phát video khi trang đang nằm trong khung nhìn camera.")]
    public bool requireCameraVisibilityToRender = true;
    [Tooltip("Camera dùng để kiểm tra visibility. Để trống sẽ tự dùng Camera.main.")]
    public Camera viewCamera;
    [Tooltip("Nếu bật, trang phải quay mặt về camera thì mới được tính là visible.")]
    public bool requireFacingCamera = true;

    private readonly Dictionary<VideoPlayer, VideoPlayer.EventHandler> loopEndHandlers = new Dictionary<VideoPlayer, VideoPlayer.EventHandler>();
    private readonly Dictionary<VideoPlayer, bool> finishedByPlayer = new Dictionary<VideoPlayer, bool>();
    private readonly Dictionary<VideoPlayer, bool> wasVisibleByPlayer = new Dictionary<VideoPlayer, bool>();
    private int currentVisiblePage = -1;

    private void Awake()
    {
        if (pageFlipController == null)
            pageFlipController = GetComponentInChildren<AlbumPageFlipController>(true);

        PrepareVideoPlayers();
    }

    private void OnEnable()
    {
        // Force refresh ngay khi bật component.
        currentVisiblePage = -1;
    }

    private void OnDisable()
    {
        PauseAllVideos();
    }

    private void OnDestroy()
    {
        UnsubscribeAllLoopEvents();
    }

    private void Update()
    {
        if (viewCamera == null && Camera.main != null)
            viewCamera = Camera.main;

        int visiblePage = ResolveCurrentVisiblePageNumber();
        if (visiblePage != currentVisiblePage)
        {
            currentVisiblePage = visiblePage;
            if (debugLog)
                Debug.Log($"[EndAlbumVideoPageController] Visible page -> {visiblePage}");
        }

        UpdateVideosForCurrentPageWindow(currentVisiblePage);
    }

    private PageVideoEntry FindEntryByVisiblePage(int pageNumber)
    {
        if (pageVideos == null) return null;
        for (int i = 0; i < pageVideos.Length; i++)
        {
            PageVideoEntry entry = pageVideos[i];
            if (entry == null) continue;
            if (entry.visiblePageNumber == pageNumber) return entry;
        }
        return null;
    }

    private void PrepareVideoPlayers()
    {
        if (pageVideos == null) return;

        for (int i = 0; i < pageVideos.Length; i++)
        {
            PageVideoEntry entry = pageVideos[i];
            if (entry == null) continue;
            PrepareSingleVideoPlayer(entry.videoPlayer, entry.holdLastFrameWhenFinished);
            PrepareSingleVideoPlayer(entry.secondaryVideoPlayer, entry.holdLastFrameWhenFinished);
        }
    }

    private void PrepareSingleVideoPlayer(VideoPlayer vp, bool holdLastFrameWhenFinished)
    {
        if (vp == null) return;

        vp.playOnAwake = false;
        vp.isLooping = false;
        vp.waitForFirstFrame = true;
        vp.Pause();
        MarkVideoFinished(vp, false);

        if (!holdLastFrameWhenFinished) return;
        if (loopEndHandlers.ContainsKey(vp)) return;

        VideoPlayer.EventHandler handler = _ => HoldLastFrame(vp);
        vp.loopPointReached += handler;
        loopEndHandlers[vp] = handler;
    }

    private void HoldLastFrame(VideoPlayer vp)
    {
        if (vp == null) return;

        // Dừng ở frame cuối, không reset về frame đầu.
        if (vp.frameCount > 1)
            vp.frame = (long)vp.frameCount - 1;

        vp.Pause();
        MarkVideoFinished(vp, true);
    }

    private void PauseAllVideos()
    {
        if (pageVideos == null) return;

        for (int i = 0; i < pageVideos.Length; i++)
        {
            PageVideoEntry entry = pageVideos[i];
            if (entry == null) continue;
            if (entry.videoPlayer != null) entry.videoPlayer.Pause();
            if (entry.secondaryVideoPlayer != null) entry.secondaryVideoPlayer.Pause();
        }
    }

    private bool IsVideoVisible(PageVideoEntry entry, VideoPlayer selectedVideo)
    {
        Transform target = ResolveVisibilityTarget(entry, selectedVideo);
        if (target == null) return true;
        Vector3 viewport = viewCamera.WorldToViewportPoint(target.position);
        bool inFront = viewport.z > 0f;
        bool inViewport = viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
        if (!inFront || !inViewport) return false;

        if (!requireFacingCamera) return true;

        Vector3 toCamera = (viewCamera.transform.position - target.position).normalized;
        return Vector3.Dot(target.forward, toCamera) > 0f;
    }

    /// <summary>
    /// Trả về true nếu trang hiện tại không có video, hoặc video trang hiện tại đã chạy xong.
    /// Dùng để gate input E lật trang tiếp theo.
    /// </summary>
    public bool CanFlipNextPageNow()
    {
        if (pageVideos == null || pageVideos.Length == 0) return true;

        int page = currentVisiblePage > 0 ? currentVisiblePage : ResolveCurrentVisiblePageNumber();
        for (int i = 0; i < pageVideos.Length; i++)
        {
            PageVideoEntry entry = pageVideos[i];
            if (!IsEntryInCurrentWindow(entry, page)) continue;
            if (entry == null) continue;

            if (entry.videoPlayer != null && IsVideoCurrentlyBlockingFlip(entry, entry.videoPlayer))
                return false;
            if (entry.secondaryVideoPlayer != null && IsVideoCurrentlyBlockingFlip(entry, entry.secondaryVideoPlayer))
                return false;
        }

        return true;
    }

    private bool IsVideoFinished(VideoPlayer vp)
    {
        if (vp == null) return true;
        return finishedByPlayer.TryGetValue(vp, out bool finished) && finished;
    }

    private void MarkVideoFinished(VideoPlayer vp, bool finished)
    {
        if (vp == null) return;
        finishedByPlayer[vp] = finished;
    }

    private Transform ResolveVisibilityTarget(PageVideoEntry entry, VideoPlayer selectedVideo)
    {
        if (entry == null || selectedVideo == null) return null;
        if (selectedVideo == entry.videoPlayer) return entry.visibilityTarget;
        if (selectedVideo == entry.secondaryVideoPlayer) return entry.secondaryVisibilityTarget;
        return null;
    }


    private int ResolveCurrentVisiblePageNumber()
    {
        if (pageFlipController == null || pageFlipController.pageFlips == null || pageFlipController.pageFlips.Length == 0)
            return 1;

        int openedSheets = 0;
        AlbumCoverFlip[] flips = pageFlipController.pageFlips;
        for (int i = 0; i < flips.Length; i++)
        {
            if (flips[i] != null && flips[i].isOpen)
                openedSheets++;
        }

        return Mathf.Max(1, openedSheets + 1);
    }

    private bool IsEntryInCurrentWindow(PageVideoEntry entry, int page)
    {
        if (entry == null) return false;
        // Cho phép 2 tờ kề nhau cùng render khi đang lật: trang hiện tại và trang trước.
        return entry.visiblePageNumber == page || entry.visiblePageNumber == page - 1;
    }

    private void UpdateVideosForCurrentPageWindow(int page)
    {
        if (pageVideos == null) return;

        for (int i = 0; i < pageVideos.Length; i++)
        {
            PageVideoEntry entry = pageVideos[i];
            if (entry == null) continue;

            bool inWindow = IsEntryInCurrentWindow(entry, page);

            ProcessVideoSlot(entry, entry.videoPlayer, inWindow);
            ProcessVideoSlot(entry, entry.secondaryVideoPlayer, inWindow);
        }
    }

    private void ProcessVideoSlot(PageVideoEntry entry, VideoPlayer vp, bool inWindow)
    {
        if (vp == null) return;

        if (!inWindow)
        {
            if (vp.isPlaying) vp.Pause();
            wasVisibleByPlayer[vp] = false;
            return;
        }

        bool visibleNow = !requireCameraVisibilityToRender || IsVideoVisible(entry, vp);
        bool wasVisible = wasVisibleByPlayer.TryGetValue(vp, out bool prev) && prev;

        if (visibleNow)
        {
            if (!wasVisible)
            {
                if (entry.restartFromBeginningOnShow)
                {
                    vp.time = 0d;
                    MarkVideoFinished(vp, false);
                }
                else if (IsVideoFinished(vp))
                {
                    HoldLastFrame(vp);
                    wasVisibleByPlayer[vp] = true;
                    return;
                }
            }

            if (!IsVideoFinished(vp) && !vp.isPlaying)
                vp.Play();
        }
        else
        {
            if (vp.isPlaying)
                vp.Pause();
        }

        wasVisibleByPlayer[vp] = visibleNow;
    }

    private bool IsVideoCurrentlyBlockingFlip(PageVideoEntry entry, VideoPlayer vp)
    {
        if (vp == null) return false;

        bool inWindow = IsEntryInCurrentWindow(entry, currentVisiblePage);
        if (!inWindow) return false;

        bool visibleNow = !requireCameraVisibilityToRender || IsVideoVisible(entry, vp);
        if (!visibleNow) return false;

        return !IsVideoFinished(vp);
    }

    private void UnsubscribeAllLoopEvents()
    {
        foreach (KeyValuePair<VideoPlayer, VideoPlayer.EventHandler> pair in loopEndHandlers)
        {
            if (pair.Key != null)
                pair.Key.loopPointReached -= pair.Value;
        }
        loopEndHandlers.Clear();
    }
}
