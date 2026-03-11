using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ComputerInteract : MonoBehaviour
{
    public GameObject hoverText;
    [Header("Hint Text")]
    public string hoverHintMessage = "Nhấn E để mở máy tính";
    [Tooltip("Bật nếu muốn code tự kéo hint theo anchor khi hover. Mặc định tắt để giữ nguyên vị trí bạn set trong scene.")]
    public bool repositionHintOnHover = false;
    [Tooltip("Điểm neo cho hint. Để trống sẽ dùng transform của object computer.")]
    public Transform hoverHintAnchor;
    [Tooltip("Offset vị trí hint so với điểm neo (world space).")]
    public Vector3 hoverHintOffset = new Vector3(0f, 0.2f, 0f);

    private Outline outline;
    private bool isHovered;
    private TMP_Text hoverTMP;
    private Text hoverUGUI;

    void Awake()
    {
        outline = GetComponent<Outline>();

        if (outline != null)
            outline.enabled = false;

        if (hoverText != null)
        {
            hoverTMP = hoverText.GetComponentInChildren<TMP_Text>(true);
            hoverUGUI = hoverText.GetComponentInChildren<Text>(true);
            hoverText.SetActive(false);
        }
    }

    // Cho phép hover trong SittingAtDesk, ComputerFinished, AlbumFocus và AlbumInteractable
    // KHÔNG hover trong ComputerActive (UI đang mở)
    private bool IsValidStateForHover()
    {
        if (GameFlow.Instance == null) return false;
        
        GameState currentState = GameFlow.Instance.currentState;
        return currentState == GameState.SittingAtDesk || 
               currentState == GameState.ComputerFinished ||
               currentState == GameState.AlbumFocus ||
               currentState == GameState.AlbumInteractable;
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
        if (NarrativeTextController.Instance != null && NarrativeTextController.Instance.IsDialogActive) return;
        
        GameState currentState = GameFlow.Instance.currentState;
        
        if (currentState == GameState.SittingAtDesk)
        {
            // Mở UI máy tính và chuyển sang ComputerActive
            ComputerUIManager uiManager = FindFirstObjectByType<ComputerUIManager>();
            if (uiManager != null)
            {
                uiManager.OpenComputer();
            }
            GameFlow.Instance.ChangeState(GameState.ComputerActive);
        }
        else if (currentState == GameState.ComputerFinished ||
                 currentState == GameState.AlbumFocus ||
                 currentState == GameState.AlbumInteractable)
        {
            // Hiện text "Nghỉ ngơi tí đã" trong dialog box
            NarrativeTextController narrativeController = FindFirstObjectByType<NarrativeTextController>();
            if (narrativeController != null)
            {
                narrativeController.ShowText("Nghỉ ngơi tí đã");
            }
            else
            {
                Debug.Log("Nghỉ ngơi tí đã");
            }
        }
    }

    private void RepositionHoverHint()
    {
        if (hoverText == null) return;
        Transform anchor = hoverHintAnchor != null ? hoverHintAnchor : transform;
        hoverText.transform.position = anchor.position + hoverHintOffset;
    }
}
