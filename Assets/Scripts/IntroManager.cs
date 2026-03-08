using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("Timeline References")]
    public PlayableDirector timelineDirector;
    public TimelineAsset introTimeline;
    
    [Header("Audio References")]
    public AudioSource alarmAudioSource; // Tiếng báo thức
    public AudioClip alarmSound;
    public AudioClip rustleSound; // Tiếng sột soạt
    public AudioClip clickSound; // Tiếng kịch
    
    [Header("UI References")]
    public GameObject fadeCanvas; // Canvas chứa fade image
    public UnityEngine.UI.Image fadeImage; // Image để fade màn hình
    public GameObject introTextPanel; // Panel chứa text intro
    public TMPro.TextMeshProUGUI introText; // Text hiển thị "ahh mình ghét cảnh này"
    
    [Header("Settings")]
    public float fadeDuration = 1f; // Thời gian fade
    public float typewriterSpeed = 0.05f; // Tốc độ typewriter
    [TextArea(2, 5)]
    public string introTextContent = "ahh mình ghét cảnh này"; // Nội dung text intro (có thể chỉnh sửa trong Inspector)
    
    private bool isIntroPlaying = false;
    
    void Start()
    {
        // Bắt đầu intro sequence
        StartIntro();
    }
    
    public void StartIntro()
    {
        if (isIntroPlaying) return;
        
        isIntroPlaying = true;
        
        // Đảm bảo màn hình đen ban đầu
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
        
        // Ẩn text panel ban đầu
        if (introTextPanel != null)
        {
            introTextPanel.SetActive(false);
        }
        
        // Bắt đầu timeline nếu có
        if (timelineDirector != null && introTimeline != null)
        {
            timelineDirector.playableAsset = introTimeline;
            timelineDirector.Play();
        }
        else
        {
            // Nếu không có timeline, chạy sequence bằng code
            StartCoroutine(IntroSequenceCoroutine());
        }
    }
    
    // Sequence được điều khiển bằng code (backup nếu không dùng Timeline)
    private IEnumerator IntroSequenceCoroutine()
    {
        // Giây 0: Màn hình đen (đã setup ở Start)
        yield return new WaitForSeconds(1f);
        
        // Giây 1: Tiếng báo thức fade in
        if (alarmAudioSource != null && alarmSound != null)
        {
            alarmAudioSource.clip = alarmSound;
            alarmAudioSource.volume = 0f;
            alarmAudioSource.Play();
            StartCoroutine(FadeAudioIn(alarmAudioSource, 1f));
        }
        
        yield return new WaitForSeconds(2f);
        
        // Giây 3: Text hiện lên với typewriter
        if (introTextPanel != null && introText != null)
        {
            introTextPanel.SetActive(true);
            introText.text = "";
            StartCoroutine(TypewriterText(introText, introTextContent));
        }
        
        yield return new WaitForSeconds(2f);
        
        // Giây 5: Tiếng sột soạt
        if (alarmAudioSource != null && rustleSound != null)
        {
            alarmAudioSource.PlayOneShot(rustleSound);
        }
        
        yield return new WaitForSeconds(2f);
        
        // Giây 7: Tiếng kịch, tắt báo thức
        if (alarmAudioSource != null && clickSound != null)
        {
            alarmAudioSource.PlayOneShot(clickSound);
            StartCoroutine(FadeAudioOut(alarmAudioSource, 0.5f));
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Fade out màn hình đen và fade in cảnh Room
        yield return StartCoroutine(FadeOutBlackScreen());
        
        // Load scene Room
        SceneManager.LoadScene("RoomScene");
    }
    
    private IEnumerator TypewriterText(TMPro.TextMeshProUGUI textComponent, string fullText)
    {
        textComponent.text = "";
        for (int i = 0; i < fullText.Length; i++)
        {
            textComponent.text += fullText[i];
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
    
    private IEnumerator FadeAudioIn(AudioSource audioSource, float duration)
    {
        float startVolume = 0f;
        float targetVolume = 1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }
    
    private IEnumerator FadeAudioOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float targetVolume = 0f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        audioSource.volume = targetVolume;
        audioSource.Stop();
    }
    
    private IEnumerator FadeOutBlackScreen()
    {
        if (fadeImage == null) yield break;
        
        float elapsed = 0f;
        Color c = fadeImage.color;
        float startAlpha = c.a;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(false);
    }
    
    // Các hàm này sẽ được gọi từ Timeline signals hoặc markers
    public void PlayAlarmSound()
    {
        if (alarmAudioSource != null && alarmSound != null)
        {
            alarmAudioSource.clip = alarmSound;
            alarmAudioSource.volume = 0f;
            alarmAudioSource.Play();
            StartCoroutine(FadeAudioIn(alarmAudioSource, 1f));
        }
    }
    
    public void ShowIntroText()
    {
        if (introTextPanel != null && introText != null)
        {
            introTextPanel.SetActive(true);
            introText.text = "";
            StartCoroutine(TypewriterText(introText, introTextContent));
        }
    }
    
    public void PlayRustleSound()
    {
        if (alarmAudioSource != null && rustleSound != null)
        {
            alarmAudioSource.PlayOneShot(rustleSound);
        }
        // Ẩn text sau khi rustle sound phát
        HideIntroText();
    }
    
    public void HideIntroText()
    {
        if (introTextPanel != null)
        {
            introTextPanel.SetActive(false);
        }
    }
    
    public void PlayClickSoundAndStopAlarm()
    {
        if (alarmAudioSource != null && clickSound != null)
        {
            alarmAudioSource.PlayOneShot(clickSound);
            StartCoroutine(FadeAudioOut(alarmAudioSource, 0.5f));
        }
    }
    
    public void FadeToRoomScene()
    {
        StartCoroutine(FadeOutAndLoadScene());
    }
    
    private IEnumerator FadeOutAndLoadScene()
    {
        // KHÔNG fade out ở IntroScene
        // Giữ màn hình đen hoàn toàn khi load scene
        // RoomScene sẽ tự fade in khi load xong
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f; // Đảm bảo màn hình đen hoàn toàn
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
        
        // Đợi một chút để đảm bảo màn hình đen được hiển thị
        yield return new WaitForSeconds(0.1f);
        
        // Load scene RoomScene (màn hình vẫn đen)
        // RoomScene sẽ tự fade in khi load xong nhờ RoomSceneFadeIn script
        SceneManager.LoadScene("RoomScene");
    }
}
