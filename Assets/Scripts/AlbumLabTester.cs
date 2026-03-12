using UnityEngine;

/// <summary>
/// Script test dùng trong scene lab để lật lần lượt tờ 2 → 6 bằng phím E,
/// không phụ thuộc GameFlow hay NarrativeTextController.
/// </summary>
public class AlbumLabTester : MonoBehaviour
{
    [Header("References")]
    [Tooltip("AlbumPageFlipController trên object Album (root).")]
    public AlbumPageFlipController albumPageFlipController;

    [Tooltip("AlbumCoverFlip của bìa (CoverTopPivot), nếu muốn mở bìa khi bắt đầu.")]
    public AlbumCoverFlip coverFlip;

    [Tooltip("Optional: gate lật trang theo tiến độ render video.")]
    public EndAlbumVideoPageController endAlbumVideoPageController;

    [Header("Settings")]
    [Tooltip("Nếu bật: vào scene lab sẽ tự mở bìa.")]
    public bool openCoverOnStart = true;
    [Tooltip("Chỉ cho lật khi video trang hiện tại đã chạy xong.")]
    public bool requireCurrentVideoFinishedBeforeFlip = true;
    [Tooltip("In log khi đang bị chặn lật trang.")]
    public bool debugBlockedFlip;

    void Start()
    {
        // Reset tờ 2→6 về trạng thái đóng
        if (albumPageFlipController != null)
        {
            albumPageFlipController.ResetPages();
        }

        // Mở bìa nếu cần
        if (openCoverOnStart && coverFlip != null)
        {
            coverFlip.Open();
        }

        if (endAlbumVideoPageController == null)
            endAlbumVideoPageController = FindFirstObjectByType<EndAlbumVideoPageController>();
    }

    void Update()
    {
        // Nhấn E để lật tờ tiếp theo (tờ 2→6)
        if (Input.GetKeyDown(KeyCode.E) && albumPageFlipController != null)
        {
            if (requireCurrentVideoFinishedBeforeFlip &&
                endAlbumVideoPageController != null &&
                !endAlbumVideoPageController.CanFlipNextPageNow())
            {
                if (debugBlockedFlip)
                    Debug.Log("[AlbumLabTester] Chưa cho lật: video trang hiện tại chưa chạy xong.");
                return;
            }

            albumPageFlipController.FlipNextPage();
        }
    }
}

