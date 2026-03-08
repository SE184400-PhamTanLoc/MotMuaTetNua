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

    [Header("Settings")]
    [Tooltip("Nếu bật: vào scene lab sẽ tự mở bìa.")]
    public bool openCoverOnStart = true;

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
    }

    void Update()
    {
        // Nhấn E để lật tờ tiếp theo (tờ 2→6)
        if (Input.GetKeyDown(KeyCode.E) && albumPageFlipController != null)
        {
            albumPageFlipController.FlipNextPage();
        }
    }
}

