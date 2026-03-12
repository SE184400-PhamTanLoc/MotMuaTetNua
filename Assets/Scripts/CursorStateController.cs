using UnityEngine;

public class CursorStateController : MonoBehaviour
{
    void Update()
    {
        // 1. Kiểm tra HUD Start Panel
        GameHUD hud = UnityEngine.Object.FindFirstObjectByType<GameHUD>();
        if (hud != null && hud.batDauPanel != null && hud.batDauPanel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // 2. Kiểm tra Dialogue hoặc Bảng trả giá
        bool dangHoiThoai = DialogueManager.Instance != null && DialogueManager.Instance.DangHoiThoai;
        bool dangTraGia = DialogueManager.Instance != null && DialogueManager.Instance.bargainPanel != null && DialogueManager.Instance.bargainPanel.activeSelf;

        if (dangHoiThoai || dangTraGia)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // 2.5 Kiểm tra Sòng Bầu Cua
        BauCuaMinigame bc = UnityEngine.Object.FindFirstObjectByType<BauCuaMinigame>();
        if (bc != null && bc.uiPanel != null && bc.uiPanel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // 2.6 Kiểm tra Giếng Nguyện Ước
        GiengNguyenUoc gieng = UnityEngine.Object.FindFirstObjectByType<GiengNguyenUoc>();
        if (gieng != null && gieng.panelUocNguyen != null && gieng.panelUocNguyen.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // 2.7 Kiểm tra Minigame Hứng quả
        if (AutoSetup.IsMinigameActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // 2.8 Kiểm tra Minigame Lau Bàn Thờ
        if (AltarCleaningMinigame.Instance != null && AltarCleaningMinigame.Instance.IsOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // Nếu không có GameFlow (ví dụ scene Day_28 không dùng GameFlow)
        // → Khóa cursor mặc định để FirstPersonController hoạt động
        if (GameFlow.Instance == null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }
        
        GameState currentState = GameFlow.Instance.currentState;
        
        // 3. Logic cho máy tính (ComputerActive)
        if (currentState == GameState.ComputerActive)
        {
            ComputerUIManager uiManager = UnityEngine.Object.FindFirstObjectByType<ComputerUIManager>();
            if (uiManager != null && uiManager.IsNotificationState())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        // 4. Các trạng thái tự do (Free roam)
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
