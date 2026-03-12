using UnityEngine;

public class ChairInteract : MonoBehaviour
{
    public GameObject hoverText;
    public Transform sitPosition; // Vị trí player sẽ ngồi (có thể để null, sẽ tự tính)

    private Outline[] outlines = new Outline[0];
    private bool isHovered;

    void Awake()
    {
        CacheOutlines();
        SetOutlineState(false);

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

        SetOutlineState(true);

        if (hoverText != null)
            hoverText.SetActive(true);
    }

    public void OnHoverExit()
    {
        if (!isHovered) return;
        isHovered = false;

        SetOutlineState(false);

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

    private void CacheOutlines()
    {
        var list = new System.Collections.Generic.List<Outline>();

        Outline self = GetComponent<Outline>();
        if (self != null) list.Add(self);

        Outline parent = GetComponentInParent<Outline>(true);
        if (parent != null && !list.Contains(parent)) list.Add(parent);

        Outline[] children = GetComponentsInChildren<Outline>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Outline child = children[i];
            if (child != null && !list.Contains(child))
            {
                list.Add(child);
            }
        }

        outlines = list.ToArray();
    }

    private void SetOutlineState(bool enabled)
    {
        if (outlines == null || outlines.Length == 0) return;
        for (int i = 0; i < outlines.Length; i++)
        {
            if (outlines[i] != null)
            {
                outlines[i].enabled = enabled;
            }
        }
    }
}
    