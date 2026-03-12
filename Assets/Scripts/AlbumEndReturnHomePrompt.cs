using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Flow riêng cho scene mới:
/// - Nhận event khi đọc xong album.
/// - Tự đóng album spawn.
/// - Hiện thoại "Về nhà thôi."
/// Không cần sửa code cũ; chỉ cần nối UnityEvent trong Inspector.
/// </summary>
public class AlbumEndReturnHomePrompt : MonoBehaviour
{
    [Header("References")]
    public AlbumFocusController albumFocusController;
    public NarrativeTextController narrativeTextController;
    [Tooltip("Album trên bàn (object A) cần chặn raycast khi vào phase đứng dậy.")]
    public Transform deskAlbumRoot;

    [Header("Flow")]
    [TextArea]
    public string returnHomeMessage = "Về nhà thôi.";
    [Tooltip("Tự đóng album spawn trước khi hiện thoại.")]
    public bool closeAlbumOnComplete = true;
    [Tooltip("Giữ nguyên hướng nhìn hiện tại sau khi album biến mất.")]
    public bool keepCurrentViewAfterAlbumClose = true;
    [Tooltip("Số frame giữ cứng góc nhìn để tránh snap do state change.")]
    public int stabilizeViewFramesAfterClose = 6;
    [Tooltip("Delay nhỏ trước khi hiện thoại (giây).")]
    public float delayBeforeDialogue = 0.08f;
    [Tooltip("Tắt raycast album bàn sau khi đọc xong, để không thể tương tác lại.")]
    public bool disableDeskAlbumRaycastAfterComplete = true;

    [Header("Events")]
    [Tooltip("Gọi khi thoại đã đóng xong.")]
    public UnityEvent onReturnHomeDialogueClosed;

    private bool hasHandled;
    private Collider[] deskAlbumColliders = new Collider[0];
    private bool cachedDeskAlbumColliders;

    private void Awake()
    {
        if (albumFocusController == null)
            albumFocusController = FindFirstObjectByType<AlbumFocusController>();
        if (narrativeTextController == null)
            narrativeTextController = FindFirstObjectByType<NarrativeTextController>();
        if (deskAlbumRoot == null)
        {
            AlbumInteract albumInteract = FindFirstObjectByType<AlbumInteract>();
            if (albumInteract != null)
                deskAlbumRoot = albumInteract.transform;
        }

        CacheDeskAlbumColliders();
    }

    /// <summary>
    /// Nối hàm này vào onAlbumReadingCompleted của AlbumFocusController.
    /// </summary>
    public void HandleAlbumReadingCompleted()
    {
        if (hasHandled) return;
        hasHandled = true;
        StartCoroutine(HandleFlowRoutine());
    }

    private IEnumerator HandleFlowRoutine()
    {
        CameraStateController cameraState = FindFirstObjectByType<CameraStateController>();
        Transform camTransform = Camera.main != null ? Camera.main.transform : null;
        Transform bodyTransform = cameraState != null ? cameraState.playerBody : null;
        Quaternion cachedCamLocalRot = camTransform != null ? camTransform.localRotation : Quaternion.identity;
        Quaternion cachedBodyRot = bodyTransform != null ? bodyTransform.rotation : Quaternion.identity;

        if (closeAlbumOnComplete && albumFocusController != null)
        {
            albumFocusController.CloseAlbum();

            if (keepCurrentViewAfterAlbumClose)
                yield return StabilizeViewAfterClose(cameraState, camTransform, bodyTransform, cachedCamLocalRot, cachedBodyRot);
        }

        if (disableDeskAlbumRaycastAfterComplete)
            SetDeskAlbumRaycastEnabled(false);

        if (delayBeforeDialogue > 0f)
            yield return new WaitForSeconds(delayBeforeDialogue);

        if (narrativeTextController == null)
            narrativeTextController = FindFirstObjectByType<NarrativeTextController>();

        if (narrativeTextController == null || string.IsNullOrWhiteSpace(returnHomeMessage))
        {
            onReturnHomeDialogueClosed?.Invoke();
            yield break;
        }

        bool closed = false;
        narrativeTextController.ShowText(returnHomeMessage, () => closed = true);

        while (!closed)
            yield return null;

        onReturnHomeDialogueClosed?.Invoke();
    }

    private IEnumerator StabilizeViewAfterClose(
        CameraStateController cameraState,
        Transform camTransform,
        Transform bodyTransform,
        Quaternion cachedCamLocalRot,
        Quaternion cachedBodyRot)
    {
        int frames = Mathf.Max(1, stabilizeViewFramesAfterClose);
        for (int i = 0; i < frames; i++)
        {
            yield return new WaitForEndOfFrame();

            if (bodyTransform != null)
                bodyTransform.rotation = cachedBodyRot;
            if (camTransform != null)
                camTransform.localRotation = cachedCamLocalRot;

            if (cameraState != null)
                cameraState.SyncCurrentViewAsBaseline(false);
        }
    }

    public void ResetHandledFlag()
    {
        hasHandled = false;
        SetDeskAlbumRaycastEnabled(true);
    }

    private void CacheDeskAlbumColliders()
    {
        if (deskAlbumRoot == null)
        {
            deskAlbumColliders = new Collider[0];
            return;
        }

        deskAlbumColliders = deskAlbumRoot.GetComponentsInChildren<Collider>(true);
        cachedDeskAlbumColliders = true;
    }

    private void SetDeskAlbumRaycastEnabled(bool enabled)
    {
        if (!cachedDeskAlbumColliders)
            CacheDeskAlbumColliders();

        if (deskAlbumColliders == null || deskAlbumColliders.Length == 0)
            return;

        for (int i = 0; i < deskAlbumColliders.Length; i++)
        {
            Collider col = deskAlbumColliders[i];
            if (col == null) continue;
            col.enabled = enabled;
        }
    }
}
