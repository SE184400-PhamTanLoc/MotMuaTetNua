using UnityEngine;

public enum FlipAxis
{
    X,
    Y,
    Z
}

/// <summary>
/// Điều khiển lật bìa trên của cuốn album 3D.
/// Gắn script này lên GameObject pivot (ví dụ CoverTopPivot) làm parent của mesh bìa trên.
/// Xoay quanh trục được chọn (X/Y/Z local) để đóng/mở bìa.
/// </summary>
public class AlbumCoverFlip : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Tốc độ lật bìa (độ mỗi giây).")]
    public float speed = 120f;

    [Tooltip("True = bìa đang mở (openAngle), False = bìa đang đóng (closedAngle).")]
    public bool isOpen = false;

    [Header("Rotation")]
    [Tooltip("Trục local dùng để lật bìa.")]
    public FlipAxis axis = FlipAxis.Z;

    [Tooltip("Góc đóng (độ) quanh trục đã chọn.")]
    public float closedAngleY = 0f;

    [Tooltip("Góc mở (độ) quanh trục đã chọn.")]
    public float openAngleY = 180f;

    void Update()
    {
        float target = isOpen ? openAngleY : closedAngleY;
        Quaternion targetRot;

        switch (axis)
        {
            case FlipAxis.X:
                targetRot = Quaternion.Euler(target, 0f, 0f);
                break;
            case FlipAxis.Y:
                targetRot = Quaternion.Euler(0f, target, 0f);
                break;
            default: // Z
                targetRot = Quaternion.Euler(0f, 0f, target);
                break;
        }

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            targetRot,
            speed * Time.deltaTime
        );
    }

    /// <summary>
    /// Đảo trạng thái đóng/mở bìa.
    /// Có thể gọi từ animation event, phím, hoặc code khác.
    /// </summary>
    public void Toggle()
    {
        isOpen = !isOpen;
    }

    /// <summary>
    /// Mở bìa (xoay về openAngleY).
    /// </summary>
    public void Open()
    {
        isOpen = true;
    }

    /// <summary>
    /// Đóng bìa (xoay về closedAngleY).
    /// </summary>
    public void Close()
    {
        isOpen = false;
    }
}

