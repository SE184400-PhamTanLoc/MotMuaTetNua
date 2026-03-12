using UnityEngine;

/// <summary>
/// Dùng cho scene wakeup mới:
/// - Chờ RoomWakeupBootstrap hoàn tất.
/// - Bật state AlbumFocus để người chơi có thể nhấn E mở album.
/// Không đụng flow scene cũ nếu không gắn script này.
/// </summary>
public class WakeupAlbumPromptGate : MonoBehaviour
{
    [Header("References")]
    public RoomWakeupBootstrap wakeupBootstrap;

    [Header("Settings")]
    [Tooltip("State sẽ bật sau khi wakeup sequence xong.")]
    public GameState stateAfterWakeup = GameState.AlbumFocus;
    [Tooltip("Tự tìm RoomWakeupBootstrap nếu chưa gán.")]
    public bool autoFindBootstrap = true;

    private bool applied;

    private void Start()
    {
        if (autoFindBootstrap && wakeupBootstrap == null)
            wakeupBootstrap = FindFirstObjectByType<RoomWakeupBootstrap>();

        if (wakeupBootstrap == null)
        {
            ApplyStateNow();
            return;
        }

        if (wakeupBootstrap.HasWakeupCompleted)
        {
            ApplyStateNow();
            return;
        }

        wakeupBootstrap.onWakeupSequenceCompleted.AddListener(ApplyStateNow);
    }

    private void OnDestroy()
    {
        if (wakeupBootstrap != null)
            wakeupBootstrap.onWakeupSequenceCompleted.RemoveListener(ApplyStateNow);
    }

    private void ApplyStateNow()
    {
        if (applied) return;
        applied = true;

        if (GameFlow.Instance != null)
            GameFlow.Instance.ChangeState(stateAfterWakeup);
    }
}
