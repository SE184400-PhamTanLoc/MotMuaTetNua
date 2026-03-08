using UnityEngine;

/// <summary>
/// Tương tác với cuốn sách (Album) trên bàn: highlight khi hover, nhấn E hiện "Nghỉ ngơi tí đã".
/// Cần gắn component Outline (QuickOutline) trên cùng object hoặc con; gán hoverText nếu muốn.
/// </summary>
public class AlbumInteract : MonoBehaviour
{
    [Header("UI")]
    public GameObject hoverText;

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

    void Awake()
    {
        if (autoSetupCollider)
            EnsureInteractionCollider();

        EnsureRaycastableLayer();

        outline = GetComponent<Outline>();
        if (outline == null)
            outline = GetComponentInChildren<Outline>();

        if (outline != null)
            outline.enabled = false;

        if (hoverText != null)
            hoverText.SetActive(false);
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
            hoverText.SetActive(true);
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
            albumFocusController = FindFirstObjectByType<AlbumFocusController>();

        // Mở thẳng album B (không thoại)
        if (albumFocusController != null)
            albumFocusController.OpenAlbum();
    }
}
