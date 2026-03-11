using UnityEngine;

public class CursorStateController : MonoBehaviour
{
    void Update()
    {
        if (GameFlow.Instance == null) return;
        
        GameState currentState = GameFlow.Instance.currentState;
        
        // ComputerActive: Kiểm tra xem có đang ở Notification state không
        if (currentState == GameState.ComputerActive)
        {
            ComputerUIManager uiManager = FindFirstObjectByType<ComputerUIManager>();
            if (uiManager != null && uiManager.IsNotificationState())
            {
                // Khi ở Notification state: Khóa cursor (chỉ cho phép Space)
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                // ComputerActive nhưng không phải Notification: Cursor tự do
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        // Các state khác: Cursor khóa (FPS mode)
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
