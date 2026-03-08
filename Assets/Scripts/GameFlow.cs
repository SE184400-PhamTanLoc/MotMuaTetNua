using UnityEngine;

public enum GameState
{
    Intro,
    State1_FreeOnlyChair,
    SittingAtDesk,
    ComputerActive,
    ComputerFinished,
    AlbumFocus,
    AlbumReading,
    AlbumInteractable
}

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance;

    public GameState currentState;

    [Header("Debug - nhảy state (chỉ để test)")]
    [Tooltip("Gán phím (vd F3) để nhảy đến bàn: ngồi ghế + SittingAtDesk. Sau đó ấn E = mở máy tính.")]
    public KeyCode jumpToDeskKey = KeyCode.None;
    [Tooltip("Gán phím (vd F4) để nhảy tới AlbumFocus: sau máy tính, camera nhìn album bàn, chờ người chơi ấn E.")]
    public KeyCode jumpToAlbumFocusKey = KeyCode.None;
    [Tooltip("Gán phím (vd F5) để nhảy tới ComputerFinished (đã làm xong máy tính).")]
    public KeyCode jumpToAfterComputerKey = KeyCode.None;
    [Tooltip("Khi nhảy sẽ teleport player đến ghế. Để trống = tự tìm ChairInteract.")]
    public ChairInteract debugChairForJump;

    void Awake()
    {
        Instance = this;
        currentState = GameState.Intro;
    }

    void Update()
    {
        ChairInteract chair = debugChairForJump != null ? debugChairForJump : FindFirstObjectByType<ChairInteract>();

        if (jumpToDeskKey != KeyCode.None && Input.GetKeyDown(jumpToDeskKey))
        {
            if (chair != null)
                chair.TeleportPlayerToChair(null);
            ChangeState(GameState.SittingAtDesk);
            return;
        }

        if (jumpToAlbumFocusKey != KeyCode.None && Input.GetKeyDown(jumpToAlbumFocusKey))
        {
            if (chair != null)
                chair.TeleportPlayerToChair(null);
            ChangeState(GameState.AlbumFocus);
        }

        if (jumpToAfterComputerKey != KeyCode.None && Input.GetKeyDown(jumpToAfterComputerKey))
        {
            if (chair != null)
                chair.TeleportPlayerToChair(null);
            // Nhảy tới trạng thái đã xong máy tính; AlbumFocusController sẽ hiện text "chán quá..." rồi tự chuyển AlbumFocus.
            ChangeState(GameState.ComputerFinished);
        }
    }

    public bool IsState(GameState state)
    {
        return currentState == state;
    }

    public void ChangeState(GameState next)
    {
        currentState = next;
    }
}
