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

    [Header("Shared Audio For Elements")]
    [Tooltip("Bật để phát 1 clip chung xuyên suốt một đoạn element liên tiếp.")]
    public bool enableSharedPageRangeAudio = false;
    [Tooltip("AudioSource phát clip chung cho nhiều element (để trống sẽ tự tạo runtime).")]
    public AudioSource sharedPageRangeAudioSource;
    [Tooltip("Clip chung cho đoạn element (ví dụ element 2 -> 4).")]
    public AudioClip sharedPageRangeClip;
    [Tooltip("Element bắt đầu (đánh số từ 1, không tính bìa).")]
    public int sharedAudioStartElement = 3;
    [Tooltip("Element kết thúc (đánh số từ 1, không tính bìa). Đặt <= 0 để tự hiểu là trang cuối.")]
    public int sharedAudioEndElement = 0;
    [Tooltip("Âm lượng clip chung.")]
    [Range(0f, 1f)]
    public float sharedPageRangeVolume = 0.85f;
    [Tooltip("Fade in cho clip chung (giây).")]
    public float sharedPageRangeFadeInDuration = 0.2f;
    [Tooltip("Fade out cho clip chung (giây).")]
    public float sharedPageRangeFadeOutDuration = 0.25f;

    [Header("Album Spawn Intro Audio")]
    [Tooltip("Nhạc nền chạy từ lúc Album B vừa spawn.")]
    public bool enableSpawnIntroAudio = false;
    [Tooltip("AudioSource cho nhạc intro của Album B (để trống sẽ tự tạo runtime).")]
    public AudioSource spawnIntroAudioSource;
    [Tooltip("Clip intro chạy từ lúc spawn Album B.")]
    public AudioClip spawnIntroAudioClip;
    [Tooltip("Âm lượng nhạc intro.")]
    [Range(0f, 1f)]
    public float spawnIntroAudioVolume = 0.75f;
    [Tooltip("Fade in nhạc intro (giây).")]
    public float spawnIntroFadeInDuration = 0.2f;
    [Tooltip("Fade out nhạc intro (giây).")]
    public float spawnIntroFadeOutDuration = 0.25f;
    [Tooltip("Trang sẽ tắt intro (đếm theo trang hiển thị, không tính bìa). <=0 sẽ tự dùng Shared Audio Start Element.")]
    public int spawnIntroStopAtElement = 0;

    [Header("Ending")]
    [Tooltip("Thoại cuối khi đã xem hết các trang. Để trống nếu muốn đóng ngay.")]
    public string endOfAlbumDialogue = "Xem xong rồi.";
    [Tooltip("Sự kiện gọi khi đã đọc xong album. Dùng để nối cutscene/chuyển scene.")]
    public UnityEvent onAlbumReadingCompleted;

    [Header("Ending Zoom Transition")]
    [Tooltip("Bật để zoom vào trang cuối trước khi chạy sự kiện kết thúc album.")]
    public bool enableFinalPageZoom = true;
    [Tooltip("Thời lượng zoom vào trang cuối (giây).")]
    public float finalPageZoomDuration = 1.1f;
    [Tooltip("Offset zoom theo local space của camera (z dương = tiến gần camera).")]
    public Vector3 finalPageZoomCameraOffset = new Vector3(0f, 0.03f, 0.2f);
    [Tooltip("Nới scale nhẹ để tạo cảm giác tiến vào tấm ảnh.")]
    public float finalPageZoomScaleMultiplier = 1.1f;
    [Tooltip("Tùy chọn chỉnh góc album ở pha zoom (theo euler world).")]
    public Vector3 finalPageZoomRotationOffsetEuler = Vector3.zero;
    [Tooltip("Curve nội suy zoom.")]
    public AnimationCurve finalPageZoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Bật để rung màn hình nhẹ trong lúc zoom cuối.")]
    public bool enableFinalZoomScreenShake = true;
    [Tooltip("Thời lượng rung màn hình (giây).")]
    public float finalZoomShakeDuration = 0.45f;
    [Tooltip("Tần số rung (cao hơn = rung nhanh hơn).")]
    public float finalZoomShakeFrequency = 22f;
    [Tooltip("Biên độ rung vị trí camera (đơn vị world/local rất nhỏ).")]
    public float finalZoomShakePositionAmplitude = 0.008f;
    [Tooltip("Biên độ rung xoay camera (độ).")]
    public float finalZoomShakeRotationAmplitude = 0.7f;
    [Tooltip("Curve giảm rung theo thời gian (thường từ 1 về 0).")]
    public AnimationCurve finalZoomShakeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Final Shake Audio")]
    [Tooltip("Phát thêm SFX khi bắt đầu rung cuối.")]
    public bool playFinalShakeSfx = false;
    [Tooltip("AudioSource phát SFX rung cuối (để trống sẽ tự tạo runtime).")]
    public AudioSource finalShakeAudioSource;
    [Tooltip("Clip SFX rung cuối.")]
    public AudioClip finalShakeClip;
    [Tooltip("Âm lượng SFX rung cuối.")]
    [Range(0f, 1f)]
    public float finalShakeSfxVolume = 1f;
    [Tooltip("Pitch bắt đầu của SFX 'bị hút vào'.")]
    public float finalShakeStartPitch = 1f;
    [Tooltip("Pitch kết thúc của SFX 'bị hút vào'.")]
    public float finalShakeEndPitch = 1.25f;
    [Tooltip("Bật để tăng pitch theo thời gian. Tắt nếu muốn giữ pitch cố định.")]
    public bool enableFinalShakePitchRamp = false;
    [Tooltip("Fade in SFX rung cuối (giây).")]
    public float finalShakeSfxFadeInDuration = 0.08f;
    [Tooltip("Fade out SFX rung cuối (giây).")]
    public float finalShakeSfxFadeOutDuration = 0.12f;
    [Tooltip("Bật để giữ SFX 'bị hút vào' đến lúc bắt đầu cutscene.")]
    public bool keepFinalShakeSfxUntilCutscene = true;

    private GameObject currentAlbum;
    private AlbumPageFlipController albumPageFlipController;
    private GameState previousState;
    private int contentFlipCount;
    private bool coverFlipped;
    private bool sequenceCompleted;
    private Coroutine autoSequenceCoroutine;
    private bool endingTransitionTriggered;
    private bool readingCompletedTriggered;
    private bool isApplyingFinalShake;
    private Vector3 originalCameraLocalPosition;
    private Quaternion originalCameraLocalRotation;
    private float shakeSeedX;
    private float shakeSeedY;
    private float shakeSeedZ;
    private bool isSharedPageRangeAudioPlaying;
    private Coroutine sharedPageAudioFadeCoroutine;
    private bool isFinalShakeSfxPlaying;
    private Coroutine finalShakeSfxFadeCoroutine;
    private AudioSource runtimeFinalShakeAudioSource;
    private bool isSpawnIntroAudioPlaying;
    private Coroutine spawnIntroAudioFadeCoroutine;

    void Start()
    {
        if (narrativeController == null)
            narrativeController = FindFirstObjectByType<NarrativeTextController>();
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        PrepareConfiguredAudioSource(sharedPageRangeAudioSource, true);
        PrepareConfiguredAudioSource(spawnIntroAudioSource, true);
        PrepareConfiguredAudioSource(finalShakeAudioSource, false);
        EnsureSeparatedFinalShakeAudioSourceIfNeeded();
        StopSharedPageRangeAudio();
        StopSpawnIntroAudio();
        StopFinalShakeSfx();
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
        endingTransitionTriggered = false;
        readingCompletedTriggered = false;
        StopSharedPageRangeAudio();
        StopSpawnIntroAudio();
        TryStartSpawnIntroAudio();

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

        StopSharedPageRangeAudio();
        StopSpawnIntroAudio();
        StopFinalScreenShake();
        StopFinalShakeSfx();

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

            // Lần lật nội dung đầu tiên tương ứng trang 2 (không tính bìa),
            // nên số trang hiển thị sau mỗi lần lật = contentFlipCount + 2.
            int currentElementNumber = contentFlipCount + 2;
            UpdateSpawnIntroAudioForElement(currentElementNumber);
            UpdateSharedPageAudioForElement(currentElementNumber);

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
        StopSpawnIntroAudio();
        StopFinalShakeSfx();

        if (narrativeController == null)
            narrativeController = FindFirstObjectByType<NarrativeTextController>();

        if (narrativeController != null && !string.IsNullOrWhiteSpace(endOfAlbumDialogue))
        {
            narrativeController.ShowText(endOfAlbumDialogue, BeginEndingTransition);
            return;
        }

        BeginEndingTransition();
    }

    private void BeginEndingTransition()
    {
        if (endingTransitionTriggered) return;
        endingTransitionTriggered = true;
        StopSharedPageRangeAudio();
        StartCoroutine(PlayFinalPageZoomThenComplete());
    }

    private IEnumerator PlayFinalPageZoomThenComplete()
    {
        if (!enableFinalPageZoom ||
            currentAlbum == null ||
            playerCamera == null ||
            finalPageZoomDuration <= 0f)
        {
            TriggerAlbumReadingCompleted();
            yield break;
        }

        Transform albumTransform = currentAlbum.transform;
        Vector3 startPos = albumTransform.position;
        Quaternion startRot = albumTransform.rotation;
        Vector3 startScale = albumTransform.localScale;

        // Quy ước: z dương = tiến gần camera (zoom in), nên trục z dùng -forward.
        Vector3 cameraLocalOffset =
            playerCamera.right * finalPageZoomCameraOffset.x +
            playerCamera.up * finalPageZoomCameraOffset.y +
            (-playerCamera.forward) * finalPageZoomCameraOffset.z;
        Vector3 targetPos = startPos + cameraLocalOffset;
        Quaternion targetRot = startRot * Quaternion.Euler(finalPageZoomRotationOffsetEuler);
        Vector3 targetScale = startScale * Mathf.Max(0.01f, finalPageZoomScaleMultiplier);
        TryStartFinalScreenShake();

        float elapsed = 0f;
        while (elapsed < finalPageZoomDuration)
        {
            // Dùng unscaled time để hiệu ứng vẫn chạy ổn nếu timeScale thay đổi.
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / finalPageZoomDuration);
            float easedT = finalPageZoomCurve != null ? finalPageZoomCurve.Evaluate(t) : t;

            albumTransform.position = Vector3.LerpUnclamped(startPos, targetPos, easedT);
            albumTransform.rotation = Quaternion.SlerpUnclamped(startRot, targetRot, easedT);
            albumTransform.localScale = Vector3.LerpUnclamped(startScale, targetScale, easedT);
            UpdateFinalShakeSfx(elapsed);

            if (isApplyingFinalShake)
            {
                if (elapsed <= finalZoomShakeDuration)
                {
                    ApplyFinalScreenShake(elapsed);
                }
                else
                {
                    StopFinalScreenShake();
                }
            }
            yield return null;
        }

        albumTransform.position = targetPos;
        albumTransform.rotation = targetRot;
        albumTransform.localScale = targetScale;
        StopFinalScreenShake();

        TriggerAlbumReadingCompleted();
    }

    private void TryStartFinalScreenShake()
    {
        if (!enableFinalZoomScreenShake) return;
        if (playerCamera == null) return;
        if (finalZoomShakeDuration <= 0f) return;
        if (finalZoomShakeFrequency <= 0f) return;
        if (finalZoomShakePositionAmplitude <= 0f && finalZoomShakeRotationAmplitude <= 0f) return;

        originalCameraLocalPosition = playerCamera.localPosition;
        originalCameraLocalRotation = playerCamera.localRotation;
        shakeSeedX = Random.Range(0f, 1000f);
        shakeSeedY = Random.Range(0f, 1000f);
        shakeSeedZ = Random.Range(0f, 1000f);
        isApplyingFinalShake = true;
        TryStartFinalShakeSfx();
    }

    private void ApplyFinalScreenShake(float elapsed)
    {
        if (playerCamera == null)
        {
            isApplyingFinalShake = false;
            return;
        }

        float normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, finalZoomShakeDuration));
        float damping = finalZoomShakeCurve != null ? finalZoomShakeCurve.Evaluate(normalized) : 1f - normalized;
        float time = Time.unscaledTime * finalZoomShakeFrequency;

        float nx = Mathf.PerlinNoise(shakeSeedX, time) * 2f - 1f;
        float ny = Mathf.PerlinNoise(shakeSeedY, time) * 2f - 1f;
        float nz = Mathf.PerlinNoise(shakeSeedZ, time) * 2f - 1f;

        Vector3 posOffset = new Vector3(nx, ny, 0f) * (finalZoomShakePositionAmplitude * damping);
        float rotZ = nz * finalZoomShakeRotationAmplitude * damping;

        playerCamera.localPosition = originalCameraLocalPosition + posOffset;
        playerCamera.localRotation = originalCameraLocalRotation * Quaternion.Euler(0f, 0f, rotZ);
        UpdateFinalShakeSfx(elapsed);
    }

    private void StopFinalScreenShake()
    {
        if (!isApplyingFinalShake) return;
        isApplyingFinalShake = false;

        if (playerCamera == null) return;
        playerCamera.localPosition = originalCameraLocalPosition;
        playerCamera.localRotation = originalCameraLocalRotation;
        if (!keepFinalShakeSfxUntilCutscene)
            StopFinalShakeSfx();
    }

    private void UpdateSharedPageAudioForElement(int elementNumber)
    {
        if (!enableSharedPageRangeAudio || sharedPageRangeClip == null)
        {
            StopSharedPageRangeAudio();
            return;
        }

        int start = Mathf.Max(1, sharedAudioStartElement);
        int lastElement = GetTotalContentElementCount();
        int end = sharedAudioEndElement <= 0
            ? lastElement
            : Mathf.Max(start, sharedAudioEndElement);
        bool shouldPlay = elementNumber >= start && elementNumber <= end;

        if (shouldPlay)
        {
            PlaySharedPageRangeAudio();
        }
        else
        {
            StopSharedPageRangeAudio();
        }
    }

    private void PlaySharedPageRangeAudio()
    {
        sharedPageRangeAudioSource = EnsureRuntimeAudioSource(sharedPageRangeAudioSource, true);
        if (sharedPageRangeAudioSource == null) return;

        if (sharedPageRangeAudioSource.clip != sharedPageRangeClip)
            sharedPageRangeAudioSource.clip = sharedPageRangeClip;
        sharedPageRangeAudioSource.loop = true;
        if (!sharedPageRangeAudioSource.isPlaying)
        {
            sharedPageRangeAudioSource.volume = 0f;
            sharedPageRangeAudioSource.Play();
        }

        EnsureSfxChannelVolume(sharedPageRangeAudioSource, sharedPageRangeVolume);
        StartSharedPageAudioFade(sharedPageRangeVolume, sharedPageRangeFadeInDuration);
        isSharedPageRangeAudioPlaying = true;
    }

    private void StopSharedPageRangeAudio()
    {
        if (sharedPageRangeAudioSource != null && sharedPageRangeAudioSource.isPlaying)
            StartSharedPageAudioFade(0f, sharedPageRangeFadeOutDuration, true);

        isSharedPageRangeAudioPlaying = false;
    }

    private void TryStartSpawnIntroAudio()
    {
        if (!enableSpawnIntroAudio) return;
        if (spawnIntroAudioClip == null) return;

        spawnIntroAudioSource = EnsureRuntimeAudioSource(spawnIntroAudioSource, true);
        if (spawnIntroAudioSource == null) return;

        if (spawnIntroAudioSource.clip != spawnIntroAudioClip)
            spawnIntroAudioSource.clip = spawnIntroAudioClip;
        spawnIntroAudioSource.loop = true;

        if (!spawnIntroAudioSource.isPlaying)
        {
            spawnIntroAudioSource.volume = 0f;
            spawnIntroAudioSource.Play();
        }

        EnsureSfxChannelVolume(spawnIntroAudioSource, spawnIntroAudioVolume);
        StartSpawnIntroAudioFade(spawnIntroAudioVolume, spawnIntroFadeInDuration);
        isSpawnIntroAudioPlaying = true;
    }

    private void UpdateSpawnIntroAudioForElement(int elementNumber)
    {
        if (!isSpawnIntroAudioPlaying) return;

        int stopAt = spawnIntroStopAtElement > 0
            ? spawnIntroStopAtElement
            : Mathf.Max(1, sharedAudioStartElement);

        // Tắt intro khi vừa tới trang mốc (ví dụ setup cũ bắt đầu từ trang 4).
        if (elementNumber >= stopAt)
            StopSpawnIntroAudio();
    }

    private void StopSpawnIntroAudio()
    {
        if (spawnIntroAudioSource != null && spawnIntroAudioSource.isPlaying)
            StartSpawnIntroAudioFade(0f, spawnIntroFadeOutDuration, true);

        isSpawnIntroAudioPlaying = false;
    }

    private void TryStartFinalShakeSfx()
    {
        if (!playFinalShakeSfx) return;
        if (finalShakeClip == null) return;

        AudioSource shakeSource = ResolveFinalShakeAudioSource();
        if (shakeSource == null) return;

        finalShakeAudioSource = shakeSource;
        finalShakeAudioSource.clip = finalShakeClip;
        finalShakeAudioSource.loop = true;
        finalShakeAudioSource.pitch = enableFinalShakePitchRamp ? finalShakeStartPitch : 1f;
        finalShakeAudioSource.volume = 0f;
        EnsureSfxChannelVolume(finalShakeAudioSource, finalShakeSfxVolume);
        finalShakeAudioSource.Play();
        isFinalShakeSfxPlaying = true;
    }

    private AudioSource EnsureRuntimeAudioSource(AudioSource source, bool loop)
    {
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }

        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.loop = loop;
        return source;
    }

    private void PrepareConfiguredAudioSource(AudioSource source, bool loop)
    {
        if (source == null) return;
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
    }

    private void EnsureSeparatedFinalShakeAudioSourceIfNeeded()
    {
        if (finalShakeAudioSource == null) return;
        if (sharedPageRangeAudioSource == null) return;
        if (finalShakeAudioSource != sharedPageRangeAudioSource) return;

        if (runtimeFinalShakeAudioSource == null)
            runtimeFinalShakeAudioSource = gameObject.AddComponent<AudioSource>();

        PrepareConfiguredAudioSource(runtimeFinalShakeAudioSource, false);
    }

    private AudioSource ResolveFinalShakeAudioSource()
    {
        // Nếu shared và final dùng chung source, tạo source riêng cho final để tránh bị shared fade-out cắt âm.
        if (finalShakeAudioSource != null &&
            sharedPageRangeAudioSource != null &&
            finalShakeAudioSource == sharedPageRangeAudioSource)
        {
            if (runtimeFinalShakeAudioSource == null)
                runtimeFinalShakeAudioSource = gameObject.AddComponent<AudioSource>();

            return EnsureRuntimeAudioSource(runtimeFinalShakeAudioSource, false);
        }

        return EnsureRuntimeAudioSource(finalShakeAudioSource, false);
    }

    private void EnsureSfxChannelVolume(AudioSource source, float baseVolume = 1f)
    {
        if (source == null) return;

        AudioChannelVolume channelVolume = source.GetComponent<AudioChannelVolume>();
        if (channelVolume == null)
        {
            channelVolume = source.gameObject.AddComponent<AudioChannelVolume>();
        }

        channelVolume.channel = AudioChannelType.Sfx;
        channelVolume.useAudioSourceVolumeAsBaseOnAwake = false;
        channelVolume.baseVolume = Mathf.Clamp01(baseVolume);
        channelVolume.ApplyCurrentVolume();
    }

    private void UpdateFinalShakeSfx(float elapsed)
    {
        if (!isFinalShakeSfxPlaying) return;
        if (finalShakeAudioSource == null) return;
        if (!finalShakeAudioSource.isPlaying) return;

        float duration = GetFinalShakeSfxTargetDuration();
        if (enableFinalShakePitchRamp)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float targetPitch = Mathf.Lerp(finalShakeStartPitch, finalShakeEndPitch, t);
            finalShakeAudioSource.pitch = targetPitch;
        }
        else
        {
            finalShakeAudioSource.pitch = 1f;
        }

        float fadeInT = finalShakeSfxFadeInDuration <= 0f
            ? 1f
            : Mathf.Clamp01(elapsed / finalShakeSfxFadeInDuration);

        float remaining = duration - elapsed;
        float fadeOutT = finalShakeSfxFadeOutDuration <= 0f
            ? 1f
            : Mathf.Clamp01(remaining / finalShakeSfxFadeOutDuration);

        float envelope = Mathf.Min(fadeInT, fadeOutT);
        finalShakeAudioSource.volume = finalShakeSfxVolume * envelope;
    }

    private void StopFinalShakeSfx()
    {
        if (!isFinalShakeSfxPlaying) return;
        isFinalShakeSfxPlaying = false;

        if (finalShakeAudioSource == null) return;
        if (finalShakeAudioSource.isPlaying)
            finalShakeAudioSource.Stop();

        finalShakeAudioSource.volume = 0f;
        finalShakeAudioSource.pitch = 1f;
        finalShakeAudioSource.loop = false;
    }

    private float GetFinalShakeSfxTargetDuration()
    {
        if (keepFinalShakeSfxUntilCutscene)
            return Mathf.Max(0.0001f, finalPageZoomDuration);

        return Mathf.Max(0.0001f, finalZoomShakeDuration);
    }

    private void StartSharedPageAudioFade(float targetVolume, float duration, bool stopAfterFade = false)
    {
        if (sharedPageRangeAudioSource == null) return;

        if (sharedPageAudioFadeCoroutine != null)
            StopCoroutine(sharedPageAudioFadeCoroutine);

        sharedPageAudioFadeCoroutine = StartCoroutine(FadeSharedPageAudioRoutine(targetVolume, duration, stopAfterFade));
    }

    private void StartSpawnIntroAudioFade(float targetVolume, float duration, bool stopAfterFade = false)
    {
        if (spawnIntroAudioSource == null) return;

        if (spawnIntroAudioFadeCoroutine != null)
            StopCoroutine(spawnIntroAudioFadeCoroutine);

        spawnIntroAudioFadeCoroutine = StartCoroutine(FadeSpawnIntroAudioRoutine(targetVolume, duration, stopAfterFade));
    }

    private IEnumerator FadeSharedPageAudioRoutine(float targetVolume, float duration, bool stopAfterFade)
    {
        if (sharedPageRangeAudioSource == null)
            yield break;

        float startVolume = sharedPageRangeAudioSource.volume;
        float safeDuration = Mathf.Max(0.0001f, duration);
        float elapsed = 0f;

        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            sharedPageRangeAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        if (sharedPageRangeAudioSource != null)
        {
            sharedPageRangeAudioSource.volume = targetVolume;
            if (stopAfterFade && sharedPageRangeAudioSource.isPlaying)
                sharedPageRangeAudioSource.Stop();
        }

        sharedPageAudioFadeCoroutine = null;
    }

    private IEnumerator FadeSpawnIntroAudioRoutine(float targetVolume, float duration, bool stopAfterFade)
    {
        if (spawnIntroAudioSource == null)
            yield break;

        float startVolume = spawnIntroAudioSource.volume;
        float safeDuration = Mathf.Max(0.0001f, duration);
        float elapsed = 0f;

        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            spawnIntroAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        if (spawnIntroAudioSource != null)
        {
            spawnIntroAudioSource.volume = targetVolume;
            if (stopAfterFade && spawnIntroAudioSource.isPlaying)
                spawnIntroAudioSource.Stop();
        }

        spawnIntroAudioFadeCoroutine = null;
    }

    private int GetTotalContentElementCount()
    {
        if (albumPageFlipController != null &&
            albumPageFlipController.pageFlips != null &&
            albumPageFlipController.pageFlips.Length > 0)
        {
            // pageFlips là số lần lật (tờ 2..6), còn số trang theo user-facing sẽ +1.
            return albumPageFlipController.pageFlips.Length + 1;
        }

        if (flipDialogues != null && flipDialogues.Length > 0)
            return flipDialogues.Length + 1;

        return 1;
    }

    private void TriggerAlbumReadingCompleted()
    {
        if (readingCompletedTriggered) return;
        readingCompletedTriggered = true;
        onAlbumReadingCompleted?.Invoke();
        if (keepFinalShakeSfxUntilCutscene)
            FadeOutAndStopFinalShakeSfx();
        // Không gọi CloseAlbum ở đây.
        // Flow mong muốn: đọc xong -> tự vào cutscene/chuyển scene.
    }

    private void FadeOutAndStopFinalShakeSfx()
    {
        if (!isFinalShakeSfxPlaying || finalShakeAudioSource == null)
        {
            StopFinalShakeSfx();
            return;
        }

        if (finalShakeSfxFadeCoroutine != null)
            StopCoroutine(finalShakeSfxFadeCoroutine);

        finalShakeSfxFadeCoroutine = StartCoroutine(FadeOutFinalShakeSfxRoutine());
    }

    private IEnumerator FadeOutFinalShakeSfxRoutine()
    {
        if (finalShakeAudioSource == null)
            yield break;

        float startVolume = finalShakeAudioSource.volume;
        float duration = Mathf.Max(0.0001f, finalShakeSfxFadeOutDuration);
        float elapsed = 0f;

        while (elapsed < duration && finalShakeAudioSource != null && finalShakeAudioSource.isPlaying)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            finalShakeAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        StopFinalShakeSfx();
        finalShakeSfxFadeCoroutine = null;
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
