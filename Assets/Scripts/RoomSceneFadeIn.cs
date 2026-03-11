using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoomSceneFadeIn : MonoBehaviour
{
    [Header("References")]
    public Image fadeImage; // Image để fade màn hình (màn hình đen)
    public Camera mainCamera; // Camera chính của RoomScene
    
    [Header("Settings")]
    public float fadeInDuration = 2f; // Thời gian fade in camera view
    
    private void Start()
    {
        // Tìm camera nếu chưa gán
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        // Tìm fade image nếu chưa gán
        if (fadeImage == null)
        {
            // Tìm trong Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                fadeImage = canvas.GetComponentInChildren<Image>();
            }
        }
        
        // KHÔNG chuyển state ngay lập tức
        // Giữ state Intro để player không thể di chuyển trong lúc fade
        // Sẽ chuyển state sau khi fade xong
        
        // Bắt đầu fade in
        StartCoroutine(FadeInCameraView());
    }
    
    private IEnumerator FadeInCameraView()
    {
        // Đảm bảo màn hình đen ban đầu
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f; // Màn hình đen hoàn toàn
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
        
        // Đợi một frame để đảm bảo scene đã load xong
        yield return null;
        
        // Fade in camera view (fade out màn hình đen)
        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color c = fadeImage.color;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
                fadeImage.color = c;
                yield return null;
            }
            
            // Đảm bảo màn hình trong suốt hoàn toàn
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
        
        // Fade xong, cho phép player di chuyển
        // Chuyển GameFlow state sang State1_FreeOnlyChair
        if (GameFlow.Instance != null)
        {
            GameFlow.Instance.ChangeState(GameState.State1_FreeOnlyChair);
        }
    }
}
