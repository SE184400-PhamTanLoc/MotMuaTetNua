using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class NarrativeTextController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogBox; // Khung hội thoại (Panel hoặc GameObject chứa text)
    public Text narrativeText; // Text bên trong (Unity UI)
    [Tooltip("Nếu dùng TextMeshPro thì gán vào đây; ưu tiên hơn narrativeText nếu cả hai có.")]
    public TMP_Text narrativeTMP;
    
    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.05f; // Thời gian giữa mỗi ký tự (giây)
    public KeyCode continueKey = KeyCode.E; // Phím để tiếp tục/đóng text
    
    [Header("Audio")]
    public AudioSource dialogAudioSource;
    public AudioClip dialogTypeClip;
    [Range(0f, 1f)] public float dialogTypeVolume = 0.35f;
    
    private Coroutine currentTextCoroutine;
    private bool isTyping = false;
    private bool isWaitingForInput = false;
    private string fullText = "";
    private System.Action onCompleteCallback;
    private bool ignoreNextInput = false; // Bỏ qua input E trong frame đầu tiên khi dialog mở

    public static NarrativeTextController Instance { get; private set; }

void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
}

void Start()
{
    EnsureReferences();
    EnsureAudioSource();
    if (dialogBox == null && (narrativeTMP != null || narrativeText != null))
    {
        var t = (narrativeTMP != null ? narrativeTMP.transform : narrativeText.transform);
        dialogBox = t.parent != null ? t.parent.gameObject : t.gameObject;
    }
}

private void EnsureReferences()
{
    if (dialogBox == null)
    {
        dialogBox = GameObject.Find("DialogBox");
        if (dialogBox == null) dialogBox = GameObject.Find("Dialog");
        if (dialogBox == null) dialogBox = GameObject.Find("DialoguePanel");
    }
    if (narrativeTMP == null && narrativeText == null)
    {
        if (dialogBox != null)
        {
            narrativeTMP = dialogBox.GetComponentInChildren<TMP_Text>(true);
            if (narrativeText == null) narrativeText = dialogBox.GetComponentInChildren<Text>(true);
        }
        if (narrativeTMP == null && narrativeText == null)
        {
            var go = GameObject.Find("DialogText");
            if (go != null)
            {
                narrativeTMP = go.GetComponent<TMP_Text>();
                if (narrativeText == null) narrativeText = go.GetComponent<Text>();
            }
        }
    }
}
    
    public bool IsDialogActive
    {
        get { return dialogBox != null && dialogBox.activeSelf; }
    }

    private bool HasTextComponent => narrativeTMP != null || narrativeText != null;

    private void SetDialogText(string value)
    {
        if (narrativeTMP != null) narrativeTMP.text = value;
        else if (narrativeText != null) narrativeText.text = value;
    }
    
    void Update()
    {
        // CHỈ xử lý input khi dialog box đang active
        // Đây là owner duy nhất của phím E khi dialog đang hiện
        if (!IsDialogActive)
        {
            StopTypingLoopSfx();
            ignoreNextInput = false; // Reset khi dialog không active
            return;
        }

        // Bỏ qua input E trong frame đầu tiên khi dialog vừa mở (tránh skip ngay lập tức)
        if (ignoreNextInput)
        {
            ignoreNextInput = false;
            return;
        }

        // Nếu đang type và nhấn E: skip typewriter (hiện toàn bộ text ngay)
        if (isTyping && Input.GetKeyDown(continueKey))
        {
            SkipTypewriter();
            return;
        }
        
        // Nếu đang chờ input và nhấn E: tiếp tục/đóng text
        if (isWaitingForInput && Input.GetKeyDown(continueKey))
        {
            // Dừng coroutine hiện tại và gọi callback
            if (currentTextCoroutine != null)
            {
                StopCoroutine(currentTextCoroutine);
            }
            
            // Ẩn text và gọi callback
            HideText();
            if (onCompleteCallback != null)
            {
                onCompleteCallback();
                onCompleteCallback = null;
            }
        }
    }
    
    public void ShowText(string text, System.Action onComplete = null)
    {
        EnsureAudioSource();
        EnsureReferences();
        if (dialogBox == null && (narrativeTMP != null || narrativeText != null))
        {
            var t = narrativeTMP != null ? narrativeTMP.transform : narrativeText.transform;
            dialogBox = t.parent != null ? t.parent.gameObject : t.gameObject;
        }
        if (!HasTextComponent)
        {
            Debug.LogWarning("NarrativeTextController: Chưa có Text hoặc TMP_Text! Gán narrativeText/narrativeTMP trong Inspector, hoặc đặt tên object: DialogBox (panel) và DialogText (chữ), hoặc thêm TMP_Text vào panel hội thoại.");
            if (onComplete != null) onComplete();
            return;
        }
        if (dialogBox == null)
        {
            Debug.LogWarning("NarrativeTextController: dialogBox chưa được gán!");
            if (onComplete != null) onComplete();
            return;
        }
        
        // Dừng text hiện tại nếu có và reset state
        if (currentTextCoroutine != null)
        {
            StopCoroutine(currentTextCoroutine);
            currentTextCoroutine = null;
        }
        StopTypingLoopSfx();
        
        // Reset state để đảm bảo text mới có thể hiển thị
        isTyping = false;
        isWaitingForInput = false;
        
        fullText = text;
        onCompleteCallback = onComplete;
        
        // Bỏ qua input E trong frame đầu tiên khi dialog mở
        ignoreNextInput = true;
        
        // Start coroutine - đảm bảo coroutine được start
        currentTextCoroutine = StartCoroutine(ShowTextCoroutine(text));
    }
    
    private IEnumerator ShowTextCoroutine(string text)
    {
        isTyping = true;
        isWaitingForInput = false;
        
        dialogBox.SetActive(true);
        StartTypingLoopSfx();
        SetDialogText("");
        yield return null;
        for (int i = 0; i < text.Length; i++)
        {
            SetDialogText(text.Substring(0, i + 1));
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        // Text đã xong, chờ nhấn E
        StopTypingLoopSfx();
        isTyping = false;
        isWaitingForInput = true;
        
        // Có thể thêm indicator (ví dụ: "Nhấn E để tiếp tục")
        // narrativeText.text += "\n[E]";
    }
    
    public void HideText()
    {
        // Ẩn dialog box
        if (dialogBox != null)
        {
            dialogBox.SetActive(false);
        }
        
        SetDialogText("");
        
        if (currentTextCoroutine != null)
        {
            StopCoroutine(currentTextCoroutine);
            currentTextCoroutine = null;
        }
        StopTypingLoopSfx();
        
        isTyping = false;
        isWaitingForInput = false;
        fullText = "";
    }
    
    // Skip typewriter effect (nhấn E khi đang type sẽ hiện toàn bộ text ngay)
    public void SkipTypewriter()
    {
    if (!isTyping) return;

    if (currentTextCoroutine != null)
        {
        StopCoroutine(currentTextCoroutine);
        currentTextCoroutine = null;
    }

            StopTypingLoopSfx();
            SetDialogText(fullText);
            isTyping = false;
            isWaitingForInput = true;
    }
    
    private void EnsureAudioSource()
    {
    if (dialogAudioSource == null)
        {
        dialogAudioSource = GetComponent<AudioSource>();
        if (dialogAudioSource == null)
        {
            dialogAudioSource = gameObject.AddComponent<AudioSource>();
        }
        }

    dialogAudioSource.playOnAwake = false;
    dialogAudioSource.spatialBlend = 0f;
    EnsureSfxChannelVolume(dialogAudioSource);
}

    private void PlayDialogClip(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        if (dialogAudioSource != null)
        {
            dialogAudioSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
            return;
        }

        if (Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, Mathf.Clamp01(volumeScale));
        }
    }
    
    private void StartTypingLoopSfx()
    {
        if (dialogAudioSource == null || dialogTypeClip == null) return;

        if (dialogAudioSource.clip != dialogTypeClip)
        {
            dialogAudioSource.clip = dialogTypeClip;
        }
        
        dialogAudioSource.loop = true;
        dialogAudioSource.volume = Mathf.Clamp01(dialogTypeVolume);
        if (!dialogAudioSource.isPlaying)
        {
            dialogAudioSource.Play();
        }
    }

    private void StopTypingLoopSfx()
    {
        if (dialogAudioSource == null) return;
        if (!dialogAudioSource.isPlaying) return;
        
        dialogAudioSource.Stop();
    }

    private void EnsureSfxChannelVolume(AudioSource source)
    {
        if (source == null) return;

        AudioChannelVolume channelVolume = source.GetComponent<AudioChannelVolume>();
        if (channelVolume == null)
        {
            channelVolume = source.gameObject.AddComponent<AudioChannelVolume>();
        }

        channelVolume.channel = AudioChannelType.Sfx;
        channelVolume.useAudioSourceVolumeAsBaseOnAwake = false;
        channelVolume.baseVolume = source.volume > 0f ? source.volume : 1f;
        channelVolume.ApplyCurrentVolume();
    }
}
