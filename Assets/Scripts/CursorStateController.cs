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

        // 1.5 Kiểm tra Settings
        if (InGameSettingsPanelController.IsAnySettingsOpen)
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

        // 2.8.5 Kiểm tra Minigame Quét Sân
        if (YardSweepingMinigame.Instance != null && YardSweepingMinigame.Instance.IsOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
            return;
        }

        // 2.9 Kiểm tra Panel Hoàn Thành
        if (hud != null && hud.hoanThanhPanel != null && hud.hoanThanhPanel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // Nếu không có GameFlow (ví dụ scene Day_28 không dùng GameFlow)
        // → Mặc định coi như đang FreeRoam
        if (GameFlow.Instance != null && GameFlow.Instance.currentState == GameState.ComputerActive)
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
        else
        {
            // Trạng thái tự do: Khóa cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
