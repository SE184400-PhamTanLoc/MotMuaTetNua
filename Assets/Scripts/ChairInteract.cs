using UnityEngine;

public class ChairInteract : MonoBehaviour
{
    public GameObject hoverText;
    public Transform sitPosition; // Vị trí player sẽ ngồi (có thể để null, sẽ tự tính)

    private Outline outline;
    private bool isHovered;

    void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline == null)
            outline = GetComponentInChildren<Outline>();

        if (outline != null)
            outline.enabled = false;

        if (hoverText != null)
            hoverText.SetActive(false);
    }

    // Helper method để tạo sitPosition trong Editor (có thể gọi từ Inspector nếu cần)
    [ContextMenu("Tạo SitPosition mặc định")]
    void CreateDefaultSitPosition()
    {
        if (sitPosition == null)
        {
            GameObject sitPosObj = new GameObject("SitPosition");
            sitPosObj.transform.SetParent(transform);
            sitPosObj.transform.localPosition = new Vector3(0, 0, 0.3f);
            sitPosition = sitPosObj.transform;
            
            #if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(sitPosObj, "Tạo SitPosition");
            UnityEditor.Selection.activeGameObject = sitPosObj;
            #endif
        }
    }

    private bool IsValidState()
    {
        return GameFlow.Instance != null && 
               GameFlow.Instance.IsState(GameState.State1_FreeOnlyChair);
    }

    public void OnHoverEnter()
    {
        if (!IsValidState()) return;
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
        if (!IsValidState()) return;

        // Kiểm tra sitPosition đã được gán chưa
        if (sitPosition == null)
        {
            Debug.LogError("ChairInteract: sitPosition chưa được gán! Vui lòng tạo một GameObject và gán vào sitPosition trong Inspector.");
            return;
        }

        // Tìm Player GameObject
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            // Nếu không có tag, tìm bằng tên
            player = GameObject.Find("Player");
        }

        if (player != null)
        {
            TeleportPlayerToChair(player);
        }

        Debug.Log("Ngồi xuống ghế");
        
        if (GameFlow.Instance != null)
        {
            GameFlow.Instance.ChangeState(GameState.SittingAtDesk);
        }
    }

    /// <summary>
    /// Chỉ teleport player đến vị trí ghế + set sitting rotation, không đổi state.
    /// Dùng cho debug nhảy thẳng AlbumFocus (GameFlow gọi) hoặc logic khác.
    /// </summary>
    public void TeleportPlayerToChair(GameObject player = null)
    {
        if (sitPosition == null) return;
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
        }
        if (player == null) return;

        Vector3 targetPosition = sitPosition.position;
        player.transform.position = targetPosition;

        Vector3 forwardDirection = transform.right;
        forwardDirection.y = 0;
        if (forwardDirection != Vector3.zero)
            player.transform.rotation = Quaternion.LookRotation(forwardDirection.normalized);

        CameraStateController cameraController = player.GetComponentInChildren<CameraStateController>();
        if (cameraController == null && Camera.main != null)
            cameraController = Camera.main.GetComponent<CameraStateController>();
        if (cameraController != null)
            cameraController.SetSittingRotation(player.transform.rotation.eulerAngles.y);
    }
}
    