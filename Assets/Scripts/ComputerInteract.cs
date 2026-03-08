using UnityEngine;

public class ComputerInteract : MonoBehaviour
{
    public GameObject hoverText;

    private Outline outline;
    private bool isHovered;

    void Awake()
    {
        outline = GetComponent<Outline>();

        if (outline != null)
            outline.enabled = false;

        if (hoverText != null)
            hoverText.SetActive(false);
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
}
