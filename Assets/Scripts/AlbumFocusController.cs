using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// Điều khiển mở/đóng album đọc sách:
/// - Nhận lệnh mở từ AlbumInteract (cuốn A trên bàn) → spawn album B trước mặt camera.
/// - Bật overlay mờ, khóa player, đổi state AlbumReading.
/// - Trong AlbumReading: trang tự lật theo chuỗi thoại; phím E chỉ dùng cho dialog box.
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
    public KeyCode closeKey = KeyCode.Escape;
    [Tooltip("Chỉ dùng debug. Mặc định tắt để không thể thoát album bằng ESC.")]
    public bool allowManualCloseBeforeEnding = false;

    [Header("Album B Dialogues (mỗi lần lật 1 trang)")]
    [Tooltip("Thoại tương ứng mỗi lần lật trang (tờ 2 -> tờ 6). Có thể để trống từng dòng nếu không muốn hiện.")]
    [TextArea(1, 4)]
    public string[] flipDialogues = new string[5]
    {
        "...",
        "...",
        "...",
        "...",
        "..."
    };

    [Header("Cover Timing")]
    [Tooltip("Thời gian chờ sau khi lật bìa đầu tiên trước khi bắt đầu thoại.")]
    public float firstCoverFlipDelay = 0.7f;

    [Header("Ending")]
    [Tooltip("Thoại cuối khi đã xem hết các trang. Để trống nếu muốn đóng ngay.")]
    public string endOfAlbumDialogue = "Xem xong rồi.";
    [Tooltip("Sự kiện gọi khi đã đọc xong album. Dùng để nối cutscene/chuyển scene.")]
    public UnityEvent onAlbumReadingCompleted;

    private GameObject currentAlbum;
    private AlbumPageFlipController albumPageFlipController;
    private GameState previousState;
    private int contentFlipCount;
    private bool coverFlipped;
    private bool sequenceCompleted;
    private Coroutine autoSequenceCoroutine;

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
            // Khi dialog đang hiện, mọi input E phải thuộc quyền NarrativeTextController.
            if (narrativeController != null && narrativeController.IsDialogActive)
            {
                previousState = currentState;
                return;
            }

            // Mặc định không cho thoát album bằng ESC.
            if (!sequenceCompleted && allowManualCloseBeforeEnding && Input.GetKeyDown(closeKey))
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

        // Reset tiến trình khi mở album
        contentFlipCount = 0;
        coverFlipped = false;
        sequenceCompleted = false;

        // Bắt đầu luồng tự lật trang + thoại.
        if (autoSequenceCoroutine != null)
        {
            StopCoroutine(autoSequenceCoroutine);
        }
        autoSequenceCoroutine = StartCoroutine(RunAutoAlbumSequence());
    }

    /// <summary>
    /// Đóng album: tắt overlay, hủy album instance, unlock player, đổi state về AlbumInteractable.
    /// </summary>
    public void CloseAlbum()
    {
        if (autoSequenceCoroutine != null)
        {
            StopCoroutine(autoSequenceCoroutine);
            autoSequenceCoroutine = null;
        }

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

    private IEnumerator RunAutoAlbumSequence()
    {
        if (sequenceCompleted) yield break;
        if (albumPageFlipController == null) yield break;

        // Bước đầu: tự lật bìa, không thoại
        if (!coverFlipped)
        {
            bool coverOpened = albumPageFlipController.FlipNextPage();
            if (!coverOpened)
            {
                OnReadingSequenceCompleted();
                yield break;
            }

            coverFlipped = true;
            yield return new WaitForSeconds(firstCoverFlipDelay);
        }

        while (!sequenceCompleted)
        {
            if (GameFlow.Instance == null || !GameFlow.Instance.IsState(GameState.AlbumReading))
                yield break;
            if (currentAlbum == null)
                yield break;

            bool flipped = albumPageFlipController.FlipNextPage();
            if (!flipped)
            {
                break;
            }

            int dialogueIndex = contentFlipCount;
            contentFlipCount++;
            yield return ShowFlipDialogueAndWait(dialogueIndex);
        }

        OnReadingSequenceCompleted();
    }

    private void OnReadingSequenceCompleted()
    {
        if (sequenceCompleted) return;
        sequenceCompleted = true;

        if (narrativeController == null)
            narrativeController = FindFirstObjectByType<NarrativeTextController>();

        if (narrativeController != null && !string.IsNullOrWhiteSpace(endOfAlbumDialogue))
        {
            narrativeController.ShowText(endOfAlbumDialogue, TriggerAlbumReadingCompleted);
            return;
        }

        TriggerAlbumReadingCompleted();
    }

    private void TriggerAlbumReadingCompleted()
    {
        onAlbumReadingCompleted?.Invoke();
        // Không gọi CloseAlbum ở đây.
        // Flow mong muốn: đọc xong -> tự vào cutscene/chuyển scene.
    }

    private IEnumerator ShowFlipDialogueAndWait(int flipIndex)
    {
        if (narrativeController == null)
            narrativeController = FindFirstObjectByType<NarrativeTextController>();

        if (narrativeController == null) yield break;
        if (flipDialogues == null) yield break;
        if (flipIndex < 0 || flipIndex >= flipDialogues.Length) yield break;

        string text = flipDialogues[flipIndex];
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        bool dialogueCompleted = false;
        narrativeController.ShowText(text, () => dialogueCompleted = true);

        while (!dialogueCompleted)
        {
            if (GameFlow.Instance == null || !GameFlow.Instance.IsState(GameState.AlbumReading))
                yield break;
            if (currentAlbum == null)
                yield break;
            yield return null;
        }
    }
}
