using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Bootstrap riêng cho scene "tỉnh dậy":
/// - Fade in từ đen.
/// - Teleport player vào ghế ngay khi scene bắt đầu.
/// - Chỉ khóa control trong lúc fade (hoặc giữ khóa nếu muốn).
/// Không thay đổi flow của scene cũ nếu script này không được gắn vào scene đó.
/// </summary>
public class RoomWakeupBootstrap : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Ảnh full-screen để fade (alpha 1 -> 0).")]
    public Image fadeImage;
    [Tooltip("Ghế dùng làm vị trí tỉnh dậy.")]
    public ChairInteract chairInteract;
    [Tooltip("Player root. Để trống sẽ tự tìm bằng tag Player hoặc FirstPersonController.")]
    public GameObject playerRoot;
    [Tooltip("Camera của player để làm hiệu ứng tỉnh dậy. Để trống sẽ tự tìm Camera.main hoặc camera con của player.")]
    public Transform playerCameraTransform;
    [Tooltip("Transform cuốn album để focus sau khi quét camera.")]
    public Transform albumFocusTarget;
    [Tooltip("Tắt RoomSceneFadeIn cũ trong scene để tránh chồng fade/state.")]
    public bool disableLegacyRoomSceneFadeIn = true;
    [Tooltip("Khóa mở settings panel trong lúc wakeup intro để tránh tự bật.")]
    public bool blockSettingsToggleDuringWakeup = true;

    [Header("Control Lock")]
    [Tooltip("Các component control cần disable trong lúc fade (vd: PlayerMovement, MouseLook, PlayerInteractor).")]
    public MonoBehaviour[] controlsToDisableDuringFade;
    [Tooltip("Giữ khóa control sau fade. Mặc định tắt vì yêu cầu chỉ khóa lúc fade.")]
    public bool keepControlsLockedAfterFade = false;

    [Header("Flow")]
    [Tooltip("Thời lượng fade in (giây).")]
    public float fadeInDuration = 1.8f;
    [Tooltip("State sau khi fade xong.")]
    public GameState stateAfterFade = GameState.SittingAtDesk;
    [Tooltip("Số frame giữ cứng pose camera ở cuối để chống giật do script khác ghi đè.")]
    public int stabilizePoseFramesAfterWakeup = 8;
    [Tooltip("Gọi khi toàn bộ wakeup sequence đã hoàn tất.")]
    public UnityEvent onWakeupSequenceCompleted;

    [Header("Wakeup Effect - Blink")]
    [Tooltip("Bật hiệu ứng chớp mắt sau khi fade in xong.")]
    public bool playWakeupBlink = true;
    [Tooltip("Số nhịp chớp.")]
    public int wakeupBlinkCount = 3;
    [Tooltip("Alpha tối đa của mỗi nhịp chớp.")]
    [Range(0f, 1f)]
    public float wakeupBlinkMaxAlpha = 0.85f;
    [Tooltip("Màu chớp mắt (thường là đen).")]
    public Color wakeupBlinkColor = Color.black;
    [Tooltip("Thời gian mắt khép lại cho mỗi nhịp.")]
    public float wakeupBlinkCloseDuration = 0.12f;
    [Tooltip("Thời gian mở mắt lại cho mỗi nhịp.")]
    public float wakeupBlinkOpenDuration = 0.16f;
    [Tooltip("Khoảng nghỉ ngắn giữa các nhịp chớp.")]
    public float wakeupBlinkInterval = 0.05f;

    [Header("Wakeup Effect - Camera Sweep")]
    [Tooltip("Bật hiệu ứng xoay ngang trái/phải rồi trả về góc nhìn ban đầu.")]
    public bool playWakeupCameraDip = true;
    [Tooltip("Biên độ xoay ngang mỗi bên (độ).")]
    public float wakeupLookDownAngle = 38f;
    [Tooltip("Thời gian xoay sang bên đầu tiên.")]
    public float wakeupLookDownDuration = 0.35f;
    [Tooltip("Giữ ở mỗi đầu xoay trong bao lâu.")]
    public float wakeupLookDownHoldDuration = 0.12f;
    [Tooltip("Thời gian xoay từ bên này sang bên kia.")]
    public float wakeupLookAcrossDuration = 0.55f;
    [Tooltip("Thời gian quay về góc ban đầu.")]
    public float wakeupLookReturnDuration = 0.45f;
    [Tooltip("Curve cho chuyển động camera.")]
    public AnimationCurve wakeupLookCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Wakeup Dialogue On Sweep")]
    [Tooltip("Hiện thoại khi camera quét trái/phải.")]
    public bool showDialogueOnSweep = true;
    [Tooltip("Thoại khi camera quét sang trái.")]
    public string leftSweepDialogue = "Ơ...";
    [Tooltip("Thoại khi camera quét sang phải.")]
    public string rightSweepDialogue = "Chỉ là mơ thôi hả...";
    [Tooltip("Nếu không tìm được NarrativeTextController thì bỏ qua thoại, không chặn flow.")]
    public bool skipDialogueIfControllerMissing = true;

    [Header("Album Focus After Sweep")]
    [Tooltip("Sau 2 nhịp trái/phải sẽ focus về album.")]
    public bool focusAlbumAfterSweep = true;
    [Tooltip("Thời gian lia camera về album.")]
    public float albumFocusDuration = 0.6f;
    [Tooltip("Thoại khi đã focus vào album.")]
    public string albumFocusDialogue = "Những bức ảnh...";

    private bool[] cachedControlEnabledStates;
    private InGameSettingsPanelController settingsController;
    private bool cachedAllowKeyboardToggle;
    private NarrativeTextController narrativeController;

    public bool HasWakeupCompleted { get; private set; }

    private void Awake()
    {
        // Chặn script fade legacy càng sớm càng tốt để tránh nó đổi state về FreeOnlyChair.
        DisableLegacyFadeIfNeeded();
    }

    private void Start()
    {
        StartCoroutine(BootstrapRoutine());
    }

    private IEnumerator BootstrapRoutine()
    {
        ResolveReferences();
        SetupSettingsBlockBeforeWakeup();
        CacheControlStates();
        SetControlsEnabled(false);

        // Giữ Intro trong lúc fade để tránh input chen vào.
        if (GameFlow.Instance != null)
            GameFlow.Instance.ChangeState(GameState.Intro);

        TeleportPlayerToChair();
        PrepareFadeVisual();

        yield return null;
        yield return FadeIn();
        yield return PlayWakeupEffects();
        SyncCameraControllersToCurrentPose();

        if (!keepControlsLockedAfterFade)
            SetControlsEnabled(true);

        if (GameFlow.Instance != null)
            GameFlow.Instance.ChangeState(stateAfterFade);

        yield return StabilizeFinalCameraPose();
        RestoreSettingsToggleAfterWakeup();
        HasWakeupCompleted = true;
        onWakeupSequenceCompleted?.Invoke();
    }

    private void ResolveReferences()
    {
        if (chairInteract == null)
            chairInteract = FindFirstObjectByType<ChairInteract>();

        if (playerRoot == null)
        {
            playerRoot = GameObject.FindGameObjectWithTag("Player");
            if (playerRoot == null)
            {
                StarterAssets.FirstPersonController fps = FindFirstObjectByType<StarterAssets.FirstPersonController>();
                if (fps != null)
                    playerRoot = fps.gameObject;
            }
        }

        if (playerCameraTransform == null)
        {
            if (Camera.main != null)
                playerCameraTransform = Camera.main.transform;
            else if (playerRoot != null)
            {
                Camera camInChildren = playerRoot.GetComponentInChildren<Camera>(true);
                if (camInChildren != null)
                    playerCameraTransform = camInChildren.transform;
            }
        }

        DisableLegacyFadeIfNeeded();

        if (settingsController == null)
            settingsController = FindFirstObjectByType<InGameSettingsPanelController>();

        if (narrativeController == null)
            narrativeController = FindFirstObjectByType<NarrativeTextController>();

        if (albumFocusTarget == null)
        {
            AlbumInteract albumInteract = FindFirstObjectByType<AlbumInteract>();
            if (albumInteract != null)
                albumFocusTarget = albumInteract.transform;
        }
    }

    private void TeleportPlayerToChair()
    {
        if (chairInteract == null)
        {
            Debug.LogWarning("RoomWakeupBootstrap: Chưa gán ChairInteract.");
            return;
        }

        chairInteract.TeleportPlayerToChair(playerRoot);
    }

    private void CacheControlStates()
    {
        if (controlsToDisableDuringFade == null)
        {
            cachedControlEnabledStates = new bool[0];
            return;
        }

        cachedControlEnabledStates = new bool[controlsToDisableDuringFade.Length];
        for (int i = 0; i < controlsToDisableDuringFade.Length; i++)
        {
            cachedControlEnabledStates[i] = controlsToDisableDuringFade[i] != null && controlsToDisableDuringFade[i].enabled;
        }
    }

    private void SetControlsEnabled(bool enabled)
    {
        if (controlsToDisableDuringFade == null) return;

        for (int i = 0; i < controlsToDisableDuringFade.Length; i++)
        {
            MonoBehaviour control = controlsToDisableDuringFade[i];
            if (control == null) continue;

            if (enabled)
            {
                bool wasEnabled = i < cachedControlEnabledStates.Length && cachedControlEnabledStates[i];
                control.enabled = wasEnabled;
            }
            else
            {
                control.enabled = false;
            }
        }
    }

    private void PrepareFadeVisual()
    {
        if (fadeImage == null) return;

        Transform t = fadeImage.transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf)
                t.gameObject.SetActive(true);
            t = t.parent;
        }

        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(true);
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage == null)
            yield break;

        float safeDuration = Mathf.Max(0.0001f, fadeInDuration);
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < safeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            c.a = Mathf.Lerp(1f, 0f, t);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(false);
    }

    private IEnumerator PlayWakeupEffects()
    {
        if (playWakeupBlink)
            yield return PlayBlinkPulseRoutine();

        if (playWakeupCameraDip)
            yield return PlayCameraSweepRoutine();

        if (focusAlbumAfterSweep)
            yield return FocusAlbumAndShowDialogueRoutine();
    }

    private IEnumerator PlayBlinkPulseRoutine()
    {
        if (fadeImage == null)
            yield break;

        int count = Mathf.Max(2, wakeupBlinkCount);
        float closeDuration = Mathf.Max(0.0001f, wakeupBlinkCloseDuration);
        float openDuration = Mathf.Max(0.0001f, wakeupBlinkOpenDuration);
        float interval = Mathf.Max(0f, wakeupBlinkInterval);
        float maxAlpha = Mathf.Clamp01(wakeupBlinkMaxAlpha);

        EnsureFadeHierarchyActive();
        fadeImage.raycastTarget = false;

        Color c = fadeImage.color;
        c.r = wakeupBlinkColor.r;
        c.g = wakeupBlinkColor.g;
        c.b = wakeupBlinkColor.b;
        c.a = 0f;
        fadeImage.color = c;

        for (int i = 0; i < count; i++)
        {
            yield return LerpFadeAlpha(0f, maxAlpha, closeDuration);
            yield return LerpFadeAlpha(maxAlpha, 0f, openDuration);
            if (i < count - 1 && interval > 0f)
                yield return new WaitForSecondsRealtime(interval);
        }

        c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(false);
    }

    private IEnumerator PlayCameraSweepRoutine()
    {
        if (playerCameraTransform == null)
            yield break;

        Transform body = playerRoot != null ? playerRoot.transform : null;
        Quaternion originalCameraLocalRotation = playerCameraTransform.localRotation;
        float originalBodyY = body != null ? body.rotation.eulerAngles.y : 0f;

        float amplitude = Mathf.Abs(wakeupLookDownAngle);
        float toFirstSideDuration = Mathf.Max(0.0001f, wakeupLookDownDuration);
        float holdDuration = Mathf.Max(0f, wakeupLookDownHoldDuration);
        float acrossDuration = Mathf.Max(0.0001f, wakeupLookAcrossDuration);
        float returnDuration = Mathf.Max(0.0001f, wakeupLookReturnDuration);
        float leftY = originalBodyY - amplitude;
        float rightY = originalBodyY + amplitude;

        float elapsed = 0f;
        while (elapsed < toFirstSideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / toFirstSideDuration);
            float eased = wakeupLookCurve != null ? wakeupLookCurve.Evaluate(t) : t;
            if (body != null)
            {
                float y = Mathf.LerpAngle(originalBodyY, leftY, eased);
                body.rotation = Quaternion.Euler(0f, y, 0f);
            }
            yield return null;
        }

        if (body != null)
            body.rotation = Quaternion.Euler(0f, leftY, 0f);

        yield return ShowSweepDialogueAndWait(leftSweepDialogue);

        if (holdDuration > 0f)
            yield return new WaitForSecondsRealtime(holdDuration);

        elapsed = 0f;
        while (elapsed < acrossDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / acrossDuration);
            float eased = wakeupLookCurve != null ? wakeupLookCurve.Evaluate(t) : t;
            if (body != null)
            {
                float y = Mathf.LerpAngle(leftY, rightY, eased);
                body.rotation = Quaternion.Euler(0f, y, 0f);
            }
            yield return null;
        }

        if (body != null)
            body.rotation = Quaternion.Euler(0f, rightY, 0f);

        yield return ShowSweepDialogueAndWait(rightSweepDialogue);

        if (holdDuration > 0f)
            yield return new WaitForSecondsRealtime(holdDuration);

        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            float eased = wakeupLookCurve != null ? wakeupLookCurve.Evaluate(t) : t;
            if (body != null)
            {
                float y = Mathf.LerpAngle(rightY, originalBodyY, eased);
                body.rotation = Quaternion.Euler(0f, y, 0f);
            }
            yield return null;
        }

        if (body != null)
            body.rotation = Quaternion.Euler(0f, originalBodyY, 0f);
        playerCameraTransform.localRotation = originalCameraLocalRotation;
    }

    private IEnumerator LerpFadeAlpha(float from, float to, float duration)
    {
        if (fadeImage == null)
            yield break;

        float elapsed = 0f;
        Color c = fadeImage.color;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(from, to, t);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }

    private void EnsureFadeHierarchyActive()
    {
        if (fadeImage == null) return;

        Transform t = fadeImage.transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf)
                t.gameObject.SetActive(true);
            t = t.parent;
        }
    }

    private void SyncCameraControllersToCurrentPose()
    {
        if (playerCameraTransform == null) return;

        CameraStateController cameraState = playerCameraTransform.GetComponent<CameraStateController>();
        if (cameraState == null)
            cameraState = playerCameraTransform.GetComponentInParent<CameraStateController>();
        if (cameraState == null)
            cameraState = playerCameraTransform.GetComponentInChildren<CameraStateController>(true);
        if (cameraState == null && Camera.main != null)
            cameraState = Camera.main.GetComponent<CameraStateController>();
        if (cameraState != null)
            cameraState.SyncCurrentViewAsBaseline(false);

        MouseLook mouseLook = playerCameraTransform.GetComponent<MouseLook>();
        if (mouseLook == null)
            mouseLook = playerCameraTransform.GetComponentInParent<MouseLook>();
        if (mouseLook == null)
            mouseLook = playerCameraTransform.GetComponentInChildren<MouseLook>(true);
        if (mouseLook != null)
            mouseLook.SyncCurrentPitchFromTransform();
    }

    private IEnumerator StabilizeFinalCameraPose()
    {
        if (stabilizePoseFramesAfterWakeup <= 0) yield break;
        if (playerCameraTransform == null) yield break;

        Quaternion targetLocalRotation = playerCameraTransform.localRotation;
        Transform body = playerRoot != null ? playerRoot.transform : null;
        float targetBodyY = body != null ? body.rotation.eulerAngles.y : 0f;

        int frames = Mathf.Max(1, stabilizePoseFramesAfterWakeup);
        for (int i = 0; i < frames; i++)
        {
            yield return new WaitForEndOfFrame();

            if (body != null)
                body.rotation = Quaternion.Euler(0f, targetBodyY, 0f);

            playerCameraTransform.localRotation = targetLocalRotation;
            SyncCameraControllersToCurrentPose();
        }
    }

    private IEnumerator ShowSweepDialogueAndWait(string text)
    {
        if (!showDialogueOnSweep) yield break;
        if (string.IsNullOrWhiteSpace(text)) yield break;

        if (narrativeController == null)
            narrativeController = FindFirstObjectByType<NarrativeTextController>();

        if (narrativeController == null)
        {
            if (!skipDialogueIfControllerMissing)
                Debug.LogWarning("RoomWakeupBootstrap: Không tìm thấy NarrativeTextController.");
            yield break;
        }

        bool completed = false;
        narrativeController.ShowText(text, () => completed = true);

        while (!completed)
            yield return null;
    }

    private IEnumerator FocusAlbumAndShowDialogueRoutine()
    {
        if (playerCameraTransform == null) yield break;
        if (albumFocusTarget == null) yield break;

        Transform body = playerRoot != null ? playerRoot.transform : null;
        if (body == null) yield break;

        Vector3 toAlbum = albumFocusTarget.position - playerCameraTransform.position;
        if (toAlbum.sqrMagnitude <= 0.0001f) yield break;

        Quaternion lookAtAlbum = Quaternion.LookRotation(toAlbum.normalized);
        float targetYaw = lookAtAlbum.eulerAngles.y;
        float targetPitch = NormalizePitch(lookAtAlbum.eulerAngles.x);

        float startYaw = body.rotation.eulerAngles.y;
        float startPitch = NormalizePitch(playerCameraTransform.localEulerAngles.x);
        float duration = Mathf.Max(0.0001f, albumFocusDuration);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = wakeupLookCurve != null ? wakeupLookCurve.Evaluate(t) : t;

            float y = Mathf.LerpAngle(startYaw, targetYaw, eased);
            float x = Mathf.Lerp(startPitch, targetPitch, eased);
            body.rotation = Quaternion.Euler(0f, y, 0f);
            playerCameraTransform.localRotation = Quaternion.Euler(x, 0f, 0f);
            yield return null;
        }

        body.rotation = Quaternion.Euler(0f, targetYaw, 0f);
        playerCameraTransform.localRotation = Quaternion.Euler(targetPitch, 0f, 0f);

        yield return ShowSweepDialogueAndWait(albumFocusDialogue);
    }

    private static float NormalizePitch(float x)
    {
        if (x > 180f) return x - 360f;
        return x;
    }

    private void SetupSettingsBlockBeforeWakeup()
    {
        if (!blockSettingsToggleDuringWakeup) return;
        if (settingsController == null) return;

        cachedAllowKeyboardToggle = settingsController.allowKeyboardToggle;
        settingsController.allowKeyboardToggle = false;
        settingsController.CloseSettings();
    }

    private void RestoreSettingsToggleAfterWakeup()
    {
        if (!blockSettingsToggleDuringWakeup) return;
        if (settingsController == null) return;

        settingsController.allowKeyboardToggle = cachedAllowKeyboardToggle;
        settingsController.CloseSettings();
    }

    private void DisableLegacyFadeIfNeeded()
    {
        if (!disableLegacyRoomSceneFadeIn) return;

        RoomSceneFadeIn[] legacyFades = FindObjectsByType<RoomSceneFadeIn>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (legacyFades == null || legacyFades.Length == 0) return;

        for (int i = 0; i < legacyFades.Length; i++)
        {
            RoomSceneFadeIn legacyFade = legacyFades[i];
            if (legacyFade == null) continue;

            // Dừng coroutine đang chạy để tránh callback đổi state khi fade xong.
            legacyFade.StopAllCoroutines();
            legacyFade.enabled = false;
        }
    }
}
