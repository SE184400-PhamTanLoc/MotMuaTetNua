using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float rayDistance = 2.5f;
    public KeyCode interactKey = KeyCode.E;

    private ChairInteract currentChair;
    private ComputerInteract currentComputer;
    private AlbumInteract currentAlbum;
    private NarrativeTextController narrativeController;
    private Camera cachedCamera;

    void Start()
    {
        // Tìm NarrativeTextController một lần để tránh FindFirstObjectByType mỗi frame
        narrativeController = FindFirstObjectByType<NarrativeTextController>();
        cachedCamera = Camera.main;
    }

    void Update()
    {
        // QUY TẮC: UI và narrative luôn ưu tiên hơn gameplay
        // 1. Nếu dialog box đang hiện, PlayerInteractor phải dừng hoàn toàn
        if (narrativeController != null && narrativeController.IsDialogActive)
        {
            // Clear hover state nếu có
            ClearHoverStates();
            return;
        }

        // 2. Nếu đang ở ComputerActive (UI mode), PlayerInteractor phải bị disable
        if (GameFlow.Instance != null && 
            GameFlow.Instance.IsState(GameState.ComputerActive))
        {
            // Clear hover state nếu có
            ClearHoverStates();
            return;
        }

        // Chỉ chạy raycast và interact khi không có UI/narrative đang active
        if (cachedCamera == null)
            cachedCamera = Camera.main;

        Ray ray = cachedCamera != null
            ? cachedCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))
            : new Ray(transform.position, transform.forward);
        
        // Tìm object tương tác dựa trên state hiện tại
        ChairInteract hitChair = null;
        ComputerInteract hitComputer = null;
        AlbumInteract hitAlbum = null;

        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, ~0, QueryTriggerInteraction.Collide);
        if (hits.Length > 1)
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        if (hits.Length > 0)
        {
            if (GameFlow.Instance == null) return;
            
            GameState currentState = GameFlow.Instance.currentState;

            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (currentState == GameState.State1_FreeOnlyChair)
                {
                    hitChair = hit.collider.GetComponentInParent<ChairInteract>();
                    if (hitChair != null)
                        break;
                }
                else if (currentState == GameState.SittingAtDesk ||
                         currentState == GameState.ComputerFinished ||
                         currentState == GameState.AlbumFocus ||
                         currentState == GameState.AlbumInteractable)
                {
                    if (hitComputer == null)
                        hitComputer = hit.collider.GetComponentInParent<ComputerInteract>();
                    if (hitAlbum == null)
                        hitAlbum = hit.collider.GetComponentInParent<AlbumInteract>();

                    if (hitComputer != null || hitAlbum != null)
                        break;
                }
            }
        }

        // Xử lý ChairInteract hover
        if (hitChair != currentChair)
        {
            if (currentChair != null)
                currentChair.OnHoverExit();

            currentChair = hitChair;

            if (currentChair != null)
                currentChair.OnHoverEnter();
        }

        // Xử lý ComputerInteract hover
        if (hitComputer != currentComputer)
        {
            if (currentComputer != null)
                currentComputer.OnHoverExit();

            currentComputer = hitComputer;

            if (currentComputer != null)
                currentComputer.OnHoverEnter();
        }

        // Xử lý AlbumInteract (cuốn sách) hover
        if (hitAlbum != currentAlbum)
        {
            if (currentAlbum != null)
                currentAlbum.OnHoverExit();

            currentAlbum = hitAlbum;

            if (currentAlbum != null)
                currentAlbum.OnHoverEnter();
        }

        // Xử lý input - chỉ khi không có UI/narrative đang active
        if (currentChair != null && Input.GetKeyDown(interactKey))
        {
            currentChair.Interact();
        }
        else if (currentComputer != null && Input.GetKeyDown(interactKey))
        {
            currentComputer.Interact();
        }
        else if (currentAlbum != null && Input.GetKeyDown(interactKey))
        {
            currentAlbum.Interact();
        }
    }

    // Helper method để clear hover states khi bị disable
    private void ClearHoverStates()
    {
        if (currentChair != null)
        {
            currentChair.OnHoverExit();
            currentChair = null;
        }

        if (currentComputer != null)
        {
            currentComputer.OnHoverExit();
            currentComputer = null;
        }

        if (currentAlbum != null)
        {
            currentAlbum.OnHoverExit();
            currentAlbum = null;
        }
    }
}
