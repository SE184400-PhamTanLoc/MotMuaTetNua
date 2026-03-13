using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using System;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject cutsceneOverlay;
    public RawImage videoDisplay;
    public VideoPlayer videoPlayer;
    public CanvasGroup overlayCanvasGroup;

    private Action onCompleteCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (cutsceneOverlay != null) cutsceneOverlay.SetActive(false);
        if (overlayCanvasGroup != null) overlayCanvasGroup.alpha = 0;

        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
        
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }

    public void PlayCutscene(string videoName, Action onComplete)
    {
        VideoClip clip = Resources.Load<VideoClip>($"video/{videoName}");
        if (clip == null)
        {
            Debug.LogError($"[CutsceneManager] Không tìm thấy video: video/{videoName}");
            onComplete?.Invoke();
            return;
        }

        onCompleteCallback = onComplete;
        videoPlayer.clip = clip;
        
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        cutsceneOverlay.SetActive(true);
        
        // Fade in overlay
        float elapsed = 0;
        float duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            overlayCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
            yield return null;
        }
        overlayCanvasGroup.alpha = 1;

        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        if (videoDisplay != null) videoDisplay.texture = vp.texture;
        vp.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(StopRoutine());
    }

    private IEnumerator StopRoutine()
    {
        // Fade out overlay
        float elapsed = 0;
        float duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            overlayCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / duration);
            yield return null;
        }
        overlayCanvasGroup.alpha = 0;
        cutsceneOverlay.SetActive(false);

        onCompleteCallback?.Invoke();
        onCompleteCallback = null;
    }

    // Tiện ích để tìm hoặc tạo CutsceneManager trong scene
    public static void EnsureInstance()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("CutsceneManager");
            Instance = go.AddComponent<CutsceneManager>();
            
            // Tạo UI cơ bản nếp cần thiết (thường nên có sẵn trong prefab)
            // Ở đây giả định người dùng sẽ gán prefab vào một scene manager nào đó 
            // hoặc tôi sẽ setup trong AutoSetup nếp có thể.
        }
    }
}
