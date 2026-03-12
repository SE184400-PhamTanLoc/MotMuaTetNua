using UnityEngine;

public class CameraStateController : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;
    public Transform albumTransform; // Gán Album GameObject vào đây trong Inspector
    public NarrativeTextController narrativeController; // Để check dialog box có đang hiện không
    
    [Header("Settings")]
    public float mouseSensitivity = 100f;
    public float albumFocusLerpSpeed = 2f; // Tốc độ lia camera sang album
    [Range(1f, 60f)] public float mouseSmoothing = 24f;
    
    private Camera cam;
    private MouseLook mouseLook;
    
    // Camera rotation
    public float XRotation { get => xRotation; set => xRotation = value; }
    private float xRotation = 0f;
    private float sittingBaseYRotation = 0f; // Góc nhìn ban đầu khi ngồi
    
    // Album focus
    private bool isFocusingAlbum = false;
    private bool shouldFocusAlbum = false;
    private Quaternion albumFocusRotation;
    private float albumFocusYaw;   // Góc ngang (playerBody)
    private float albumFocusPitch; // Góc lên/xuống (camera local X)
    private Vector2 smoothedMouseDelta;

    /// <summary>
    /// Buộc tính lại góc nhìn tới album ở lần HandleAlbumFocus tiếp theo
    /// (dùng sau khi thoại \"Chán quá...\" kết thúc để bắt đầu lia sang album).
    /// </summary>
    public void RecalculateAlbumFocus()
    {
        isFocusingAlbum = false;
        shouldFocusAlbum = true;
    }
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
            
        mouseLook = GetComponent<MouseLook>();
        if (mouseLook == null)
            mouseLook = FindFirstObjectByType<MouseLook>();
        
        // Tìm NarrativeTextController nếu chưa gán
        if (narrativeController == null)
        {
            narrativeController = FindFirstObjectByType<NarrativeTextController>();
        }
    }
    
    private GameState previousState;
    
    private Vector2 ReadSmoothedMouseDelta()
    {
        Vector2 rawDelta = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );
        float dt = Time.unscaledDeltaTime;
        // Đồng nhất với MouseLook: dùng 0.5f scale và không nhân dt ở đây.
        Vector2 targetDelta = rawDelta * (mouseSensitivity * 0.5f); 
        float blend = 1f - Mathf.Exp(-mouseSmoothing * dt);
        smoothedMouseDelta = Vector2.Lerp(smoothedMouseDelta, targetDelta, blend);
        return smoothedMouseDelta;
    }

    void LateUpdate()
    {
        if (GameFlow.Instance == null) return;
        
        GameState currentState = GameFlow.Instance.currentState;
        
        // Reset về góc nhìn ban đầu khi chuyển sang AlbumInteractable
        if (currentState == GameState.AlbumInteractable && 
            previousState != GameState.AlbumInteractable)
        {
            ResetToSittingRotation();
        }

        // ĐỒNG BỘ X-ROTATION khi chuyển từ Free sang Limited (hoặc ngược lại nếu cần)
        if (currentState != GameState.State1_FreeOnlyChair && previousState == GameState.State1_FreeOnlyChair)
        {
            if (mouseLook != null)
            {
                xRotation = mouseLook.XRotation;
                Debug.Log($"[CameraStateController] Đồng bộ X từ MouseLook: {xRotation}");
            }
        }
        else if (currentState == GameState.State1_FreeOnlyChair && previousState != GameState.State1_FreeOnlyChair)
        {
             if (mouseLook != null)
             {
                 mouseLook.XRotation = xRotation;
                 Debug.Log($"[CameraStateController] Trả lại X cho MouseLook: {xRotation}");
             }
        }
        
        // Xử lý theo từng state
        switch (currentState)
        {
            case GameState.SittingAtDesk:
            case GameState.AlbumInteractable:
                HandleLimitedRotation();
                break;

            case GameState.ComputerFinished:
                if (shouldFocusAlbum)
                {
                    // Lia sang album khi đang ở hộp thoại "..."
                    HandleAlbumFocus();
                }
                else
                {
                    // Sau khi lia xong: cho xoay giới hạn như bình thường
                    HandleLimitedRotation();
                }
                break;

            case GameState.ComputerActive:
                // Khóa camera hoàn toàn
                break;
                
            case GameState.AlbumFocus:
                if (shouldFocusAlbum)
                {
                    // Trong phase "...": vẫn lia sang album A
                    HandleAlbumFocus();
                }
                else
                {
                    HandleLimitedRotation();
                }
                break;

            case GameState.AlbumReading:
                // Đang đọc album B: camera look được khóa/mở bởi AlbumFocusController (playerLook),
                // không tự lia về album bàn nữa.
                break;
                
            default:
                // State1_FreeOnlyChair: xoay tự do (MouseLook xử lý)
                smoothedMouseDelta = Vector2.zero;
                break;
        }
        
        previousState = currentState;
    }
    
    private void HandleLimitedRotation()
    {
        // Nếu dialog box đang hiện → khóa camera hoàn toàn
        if (IsDialogActive())
        {
            smoothedMouseDelta = Vector2.zero;
            return;
        }
        
        // Giới hạn xoay 180 độ từ góc nhìn ban đầu
        Vector2 mouseDelta = ReadSmoothedMouseDelta();
        float mouseX = mouseDelta.x * Time.unscaledDeltaTime;
        float mouseY = mouseDelta.y * Time.unscaledDeltaTime;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        if (playerBody != null)
        {
            float currentYRotation = playerBody.rotation.eulerAngles.y;
            float rotationDelta = Mathf.DeltaAngle(sittingBaseYRotation, currentYRotation);
            
            float newRotationDelta = rotationDelta + mouseX;
            newRotationDelta = Mathf.Clamp(newRotationDelta, -90f, 90f);
            
            float targetYRotation = sittingBaseYRotation + newRotationDelta;
            playerBody.rotation = Quaternion.Euler(0f, targetYRotation, 0f);
        }
    }
    
    private bool IsDialogActive()
    {
        if (narrativeController != null)
        {
            return narrativeController.IsDialogActive;
        }
        return false;
    }
    
    private void HandleAlbumFocus()
    {
        smoothedMouseDelta = Vector2.zero;
        // Lần đầu vào focus album (AlbumFocus hoặc ComputerFinished): tính góc nhìn từ CAMERA tới album
        if (!isFocusingAlbum && albumTransform != null)
        {
            isFocusingAlbum = true;
            Vector3 camPos = transform.position;
            Vector3 toAlbum = albumTransform.position - camPos;
            if (toAlbum.sqrMagnitude > 0.001f)
            {
                Quaternion lookAtAlbum = Quaternion.LookRotation(toAlbum.normalized);
                Vector3 euler = lookAtAlbum.eulerAngles;
                albumFocusYaw = euler.y;
                albumFocusPitch = NormalizePitch(euler.x);
                albumFocusRotation = Quaternion.Euler(0f, albumFocusYaw, 0f);
            }
        }
        
        if (isFocusingAlbum && playerBody != null)
        {
            playerBody.rotation = Quaternion.Slerp(
                playerBody.rotation,
                albumFocusRotation,
                albumFocusLerpSpeed * Time.deltaTime
            );
            xRotation = Mathf.Lerp(xRotation, albumFocusPitch, albumFocusLerpSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Nếu đã gần bằng mục tiêu: kết thúc lia, chuẩn hóa góc base để user xoay giới hạn bình thường
            float angle = Quaternion.Angle(playerBody.rotation, albumFocusRotation);
            float pitchDelta = Mathf.Abs(Mathf.DeltaAngle(xRotation, albumFocusPitch));
            if (angle < 2f && pitchDelta < 2f) // nới ngưỡng để thoát lia dễ hơn
            {
                isFocusingAlbum = false;
                shouldFocusAlbum = false;
            }
        }
    }
    
    private static float NormalizePitch(float x)
    {
        if (x > 180f) return x - 360f;
        return x;
    }
    
    // Được gọi từ ChairInteract khi ngồi vào ghế
    public void SetSittingRotation(float baseYRotation)
    {
        sittingBaseYRotation = baseYRotation;
        // KHÔNG reset xRotation về 0, để giữ hướng nhìn hiện tại của người chơi cho mượt
        Vector3 currentRot = transform.localRotation.eulerAngles;
        xRotation = NormalizePitch(currentRot.x);
        isFocusingAlbum = false; 
        Debug.Log($"[CameraStateController] SetSittingRotation: {baseYRotation}, X preserved: {xRotation}");
    }
    
    // Được gọi khi chuyển sang AlbumInteractable để quay lại góc nhìn ban đầu
    public void ResetToSittingRotation()
    {
        if (playerBody != null)
        {
            playerBody.rotation = Quaternion.Euler(0f, sittingBaseYRotation, 0f);
        }
        // Khi reset về vị trí ngồi chuẩn, có thể lướt nhẹ về 0 hoặc giữ nguyên.
        // Ở đây ta giữ nguyên để tránh bị "giật" ngược lại.
        isFocusingAlbum = false;
        Debug.Log("[CameraStateController] ResetToSittingRotation (Pitch kept)");
    }
}
