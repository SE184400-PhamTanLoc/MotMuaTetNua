using UnityEngine;
using UnityEngine.Playables;

public class TimelineSignalReceiver : MonoBehaviour
{
    public IntroManager introManager;
    
    // Các hàm này sẽ được gọi từ Timeline Signal Events
    public void OnPlayAlarm()
    {
        Debug.Log("[Signal] OnPlayAlarm được gọi!");
        if (introManager != null)
            introManager.PlayAlarmSound();
        else
            Debug.LogError("[Signal] IntroManager chưa được kết nối!");
    }
    
    public void OnShowText()
    {
        Debug.Log("[Signal] OnShowText được gọi!");
        if (introManager != null)
            introManager.ShowIntroText();
        else
            Debug.LogError("[Signal] IntroManager chưa được kết nối!");
    }
    
    public void OnPlayRustle()
    {
        Debug.Log("[Signal] OnPlayRustle được gọi!");
        if (introManager != null)
            introManager.PlayRustleSound();
        else
            Debug.LogError("[Signal] IntroManager chưa được kết nối!");
    }
    
    public void OnClickAndStopAlarm()
    {
        Debug.Log("[Signal] OnClickAndStopAlarm được gọi!");
        if (introManager != null)
            introManager.PlayClickSoundAndStopAlarm();
        else
            Debug.LogError("[Signal] IntroManager chưa được kết nối!");
    }
    
    public void OnFadeToRoom()
    {
        Debug.Log("[Signal] OnFadeToRoom được gọi!");
        if (introManager != null)
            introManager.FadeToRoomScene();
        else
            Debug.LogError("[Signal] IntroManager chưa được kết nối!");
    }
}
