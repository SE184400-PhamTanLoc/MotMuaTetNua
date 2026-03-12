using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Tương tác với cuốn sách (Album) trên bàn: highlight khi hover, nhấn E hiện "Nghỉ ngơi tí đã".
/// Cần gắn component Outline (QuickOutline) trên cùng object hoặc con; gán hoverText nếu muốn.
/// </summary>
public class AlbumInteract : MonoBehaviour
{
    [Header("UI")]
    public GameObject hoverText;
    [Header("Hint Text")]
    public string hoverHintMessage = "Nhấn E để mở album";
    [Tooltip("Bật nếu muốn code tự kéo hint theo anchor khi hover. Mặc định tắt để giữ nguyên vị trí bạn set trong scene.")]
    public bool repositionHintOnHover = false;
    [Tooltip("Điểm neo cho hint. Để trống sẽ dùng transform của object album.")]
    public Transform hoverHintAnchor;
    [Tooltip("Offset vị trí hint so với điểm neo (world space).")]
    public Vector3 hoverHintOffset = new Vector3(0f, 0.2f, 0f);

    [Header("Controller")]
    [Tooltip("AlbumFocusController để mở album B (spawn trước mặt, bật overlay).")]
    public AlbumFocusController albumFocusController;
    [Header("Auto Setup")]
    [Tooltip("Tự đảm bảo Album có collider để raycast hover/Interact.")]
    public bool autoSetupCollider = true;
    [Tooltip("Nới collider thêm chút để raycast dễ bắt hơn.")]
    public float colliderPadding = 0.02f;

    private Outline outline;
    private bool isHovered;
    private TMP_Text hoverTMP;
    private Text hoverUGUI;

    void Awake()
    {
        if (autoSetupCollider)
            EnsureInteractionCollider();

        EnsureRaycastableLayer();

        // Ưu tiên dùng hint nằm trong chính hierarchy của Album để tránh trỏ nhầm
        AssignLocalHoverHintIfAvailable();

        outline = GetComponent<Outline>();
        if (outline == null)
            outline = GetComponentInChildren<Outline>();

        if (outline != null)
            outline.enabled = false;

        if (hoverText != null)
        {
            hoverTMP = hoverText.GetComponentInChildren<TMP_Text>(true);
            hoverUGUI = hoverText.GetComponentInChildren<Text>(true);
            hoverText.SetActive(false);
        }
    }

    private void AssignLocalHoverHintIfAvailable()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < allChildren.Length; i++)
        {
            Transform child = allChildren[i];
            if (child == null) continue;
            if (!string.Equals(child.name, "Hint")) continue;

            hoverText = child.gameObject;
            return;
        }
    }

    private void EnsureRaycastableLayer()
    {
        // Layer 2 = Ignore Raycast, sẽ khiến hover không bao giờ bắt trúng.
        if (gameObject.layer == 2)
            gameObject.layer = 0;
    }

    private void EnsureInteractionCollider()
    {
        Collider existing = GetComponentInChildren<Collider>(true);
        if (existing != null)
            return;

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
            return;

        Bounds worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            worldBounds.Encapsulate(renderers[i].bounds);

        BoxCollider box = gameObject.GetComponent<BoxCollider>();
        if (box == null)
            box = gameObject.AddComponent<BoxCollider>();

        Vector3 lossy = transform.lossyScale;
        float sx = Mathf.Abs(lossy.x) < 0.0001f ? 1f : Mathf.Abs(lossy.x);
        float sy = Mathf.Abs(lossy.y) < 0.0001f ? 1f : Mathf.Abs(lossy.y);
        float sz = Mathf.Abs(lossy.z) < 0.0001f ? 1f : Mathf.Abs(lossy.z);

        box.center = transform.InverseTransformPoint(worldBounds.center);
        box.size = new Vector3(
            worldBounds.size.x / sx + colliderPadding,
            worldBounds.size.y / sy + colliderPadding,
            worldBounds.size.z / sz + colliderPadding
        );
        box.isTrigger = false;
    }

    private bool IsValidStateForHover()
    {
        if (GameFlow.Instance == null) return false;
        GameState s = GameFlow.Instance.currentState;
        return s == GameState.ComputerFinished || s == GameState.AlbumFocus || s == GameState.AlbumInteractable;
    }

    public void OnHoverEnter()
    {
        if (!IsValidStateForHover()) return;
        if (isHovered) return;
        isHovered = true;

        if (outline != null)
            outline.enabled = true;

        if (hoverText != null)
        {
            if (hoverTMP != null) hoverTMP.text = hoverHintMessage;
            else if (hoverUGUI != null) hoverUGUI.text = hoverHintMessage;
            if (repositionHintOnHover)
                RepositionHoverHint();
            hoverText.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        if (!isHovered) return;
        isHovered = false;

        if (outline != null)
            outline.enabled = false;

        if (hoverText != null)
            hoverText.SetActive(false);
    }

    public void Interact()
    {
        if (!IsValidStateForHover()) return;
        if (GameFlow.Instance == null) return;

        // Nếu chưa gán bằng Inspector, thử tìm
        if (albumFocusController == null)
            albumFocusController = FindBestAlbumFocusController();

        // Mở thẳng album B (không thoại)
        if (albumFocusController != null)
            albumFocusController.OpenAlbum();
    }

    private AlbumFocusController FindBestAlbumFocusController()
    {
        AlbumFocusController[] controllers = FindObjectsByType<AlbumFocusController>(FindObjectsSortMode.None);
        if (controllers == null || controllers.Length == 0)
            return null;

        for (int i = 0; i < controllers.Length; i++)
        {
            AlbumFocusController candidate = controllers[i];
            if (candidate == null) continue;

            // Ưu tiên controller đã được cấu hình đầy đủ cho RoomScene.
            if (candidate.albumPrefab != null && candidate.playerCamera != null && candidate.overlay != null)
                return candidate;
        }

        return controllers[0];
    }

    private void RepositionHoverHint()
    {
        if (hoverText == null) return;
        Transform anchor = hoverHintAnchor != null ? hoverHintAnchor : transform;
        hoverText.transform.position = anchor.position + hoverHintOffset;
    }
}
