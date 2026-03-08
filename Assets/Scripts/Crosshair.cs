using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("UI References")]
    public Image crosshairImage; // Image của crosshair (có thể là dot, circle, cross, v.v.)
    
    [Header("Settings")]
    public bool showInAllStates = true; // Hiển thị trong tất cả state
    public bool hideInComputerActive = true; // Ẩn khi đang dùng máy tính
    
    void Update()
    {
        if (crosshairImage == null) return;
        
        bool shouldShow = showInAllStates;
        
        // Ẩn khi đang ở IntroScene
        if (GameFlow.Instance != null)
        {
            if (GameFlow.Instance.IsState(GameState.Intro))
            {
                shouldShow = false;
            }
        }
        
        // Ẩn khi đang dùng máy tính (nếu bật)
        if (hideInComputerActive && GameFlow.Instance != null)
        {
            if (GameFlow.Instance.IsState(GameState.ComputerActive))
            {
                shouldShow = false;
            }
        }
        
        crosshairImage.gameObject.SetActive(shouldShow);
    }
}
