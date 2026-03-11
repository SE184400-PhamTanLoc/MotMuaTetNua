using UnityEngine;

/// <summary>
/// Điều khiển mở/đóng album đọc sách:
/// - Nhận lệnh mở từ AlbumInteract (cuốn A trên bàn) → spawn album B trước mặt camera.
/// - Bật overlay mờ, khóa player, đổi state AlbumReading.
/// - Nhấn E khi đang AlbumReading = lật trang (AlbumPageFlipController trên instance vừa spawn).
/// - Nhấn ESC (hoặc gọi CloseAlbum) = tắt overlay, hủy album B, unlock player, đổi state AlbumInteractable.
/// </summary>
public class AlbumFocusController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Prefab album B (có logic lật trang, nội dung) sẽ được spawn.")]
    public GameObject albumPrefab;

    [Tooltip("Camera của player, dùng để đặt album trước mặt.")]
    public Transform playerCamera;

    [Tooltip("Overlay mờ full màn hình (Canvas/Image). Bật/tắt khi mở album.")]
    public GameObject overlay;

    [Tooltip("Tùy chọn: khóa/mở movement bằng cách enable/disable script điều khiển di chuyển.")]
    public MonoBehaviour playerMovement;

    [Tooltip("Tùy chọn: khóa/mở camera look bằng cách enable/disable script điều khiển nhìn.")]
    public MonoBehaviour playerLook;

    [Tooltip("Tùy chọn: NarrativeTextController nếu muốn hiển thị thoại ngắn khi mở/đóng.")]
    public NarrativeTextController narrativeController;

    [Header("Spawn Settings")]
    [Tooltip("Khoảng cách từ camera đến album B khi spawn (local forward).")]
    public float spawnDistance = 0.7f;

    [Tooltip("Offset local (theo camera) khi spawn.")]
    public Vector3 spawnOffset = new Vector3(0f, -0.2f, 0f);

    [Tooltip("Rotation offset khi spawn (dùng nếu album quay ngược).")]
    public Vector3 spawnRotationOffsetEuler = Vector3.zero;

    [Header("Input")]
    public KeyCode nextPageKey = KeyCode.E;
    public KeyCode closeKey = KeyCode.Escape;

    private GameObject currentAlbum;
    private AlbumPageFlipController albumPageFlipController;
    private GameState previousState;

    void Start()
    {
        if (narrativeController == null)
            narrativeController = FindFirstObjectByType<NarrativeTextController>();
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    void Update()
    {
        if (GameFlow.Instance == null) return;

        GameState currentState = GameFlow.Instance.currentState;

        // Khi chuyển sang ComputerFinished: hiện thoại ("chán quá...", sau đó "...")
        if (currentState == GameState.ComputerFinished &&
            previousState != GameState.ComputerFinished)
        {
            OnEnterComputerFinished();
        }

        // Khi vào AlbumReading lần đầu
        if (currentState == GameState.AlbumReading && previousState != GameState.AlbumReading)
        {
            OnEnterAlbumReading();
        }

        // Input trong AlbumReading
        if (currentState == GameState.AlbumReading)
        {
            if (albumPageFlipController != null && Input.GetKeyDown(nextPageKey))
            {
                albumPageFlipController.FlipNextPage();
            }

            if (Input.GetKeyDown(closeKey))
            {
                CloseAlbum();
            }
        }

        previousState = currentState;
    }

    private void OnEnterComputerFinished()
    {
        // Thoại 1: "Chán quá, nghỉ mắt tí...", sau đó thoại 2: "..."
        if (narrativeController == null)
            narrativeController = FindFirstObjectByType<NarrativeTextController>();

        // Chuẩn bị controller camera (để lia sang album ở bước thoại thứ 2)
        CameraStateController camCtrl = FindFirstObjectByType<CameraStateController>();

        if (narrativeController != null)
        {
            narrativeController.ShowText("Chán quá, nghỉ mắt tí...", () =>
            {
                // Bắt đầu lia camera sang album trước khi hiện "..."
                if (camCtrl != null)
                    camCtrl.RecalculateAlbumFocus();

                // Sang phase AlbumFocus (chờ người chơi ấn E vào album A)
                if (GameFlow.Instance != null)
                    GameFlow.Instance.ChangeState(GameState.AlbumFocus);

                narrativeController.ShowText("...", null);
            });
        }
        else
        {
            Debug.Log("Chán quá, nghỉ mắt tí...");
            Debug.Log("...");
            if (camCtrl != null)
                camCtrl.RecalculateAlbumFocus();
            if (GameFlow.Instance != null)
                GameFlow.Instance.ChangeState(GameState.AlbumFocus);
        }
        // Tới AlbumFocus: player cần nhấn E vào Album A để mở AlbumReading
    }

    /// <summary>
    /// Gọi từ AlbumInteract (cuốn A trên bàn) khi nhấn E.
    /// </summary>
    public void OpenAlbum()
    {
        if (GameFlow.Instance != null)
            GameFlow.Instance.ChangeState(GameState.AlbumReading);
        else
            OnEnterAlbumReading(); // fallback nếu không có GameFlow
    }

    private void OnEnterAlbumReading()
    {
        // Bật overlay
        if (overlay != null) overlay.SetActive(true);

        // Khóa điều khiển player/camera (nếu gán)
        SetPlayerControl(false);

        // Spawn album B nếu chưa có
        if (currentAlbum == null && albumPrefab != null && playerCamera != null)
        {
            Vector3 pos = playerCamera.position
                          + playerCamera.forward * spawnDistance
                          + playerCamera.TransformVector(spawnOffset);

            Quaternion rot = playerCamera.rotation * Quaternion.Euler(spawnRotationOffsetEuler);

            currentAlbum = Instantiate(albumPrefab, pos, rot);
            albumPageFlipController = currentAlbum.GetComponentInChildren<AlbumPageFlipController>();
            if (albumPageFlipController != null)
                albumPageFlipController.ResetPages();
        }
    }

    /// <summary>
    /// Đóng album: tắt overlay, hủy album instance, unlock player, đổi state về AlbumInteractable.
    /// </summary>
    public void CloseAlbum()
    {
        if (overlay != null) overlay.SetActive(false);

        if (currentAlbum != null)
        {
            Destroy(currentAlbum);
            currentAlbum = null;
            albumPageFlipController = null;
        }

        SetPlayerControl(true);

        if (GameFlow.Instance != null)
            GameFlow.Instance.ChangeState(GameState.AlbumInteractable);
    }

    private void SetPlayerControl(bool enabled)
    {
        if (playerMovement != null) playerMovement.enabled = enabled;
        if (playerLook != null) playerLook.enabled = enabled;
    }
}
