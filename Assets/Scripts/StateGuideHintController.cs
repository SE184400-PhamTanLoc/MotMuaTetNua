using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class StateGuideHintEntry
{
    public GameState state;
    [TextArea(1, 3)] public string message;
    public float visibleDuration = 4f;
    public bool showOnlyOnce = true;
}

/// <summary>
/// Hiển thị box guide theo GameState (fade in/out).
/// Có thể replay hint gần nhất khi mở Settings panel.
/// </summary>
public class StateGuideHintController : MonoBehaviour
{
    [Header("Settings State Source")]
    [Tooltip("Nếu có, dùng thêm controller này để fallback đọc trạng thái mở settings.")]
    public InGameSettingsPanelController settingsPanelController;

    [Header("Guide Targets - Gameplay")]
    public GameObject gameplayGuideRoot;
    public CanvasGroup gameplayCanvasGroup;
    public TMP_Text gameplayGuideTextTMP;
    public Text gameplayGuideTextUGUI;

    [Header("Guide Targets - Settings")]
    public GameObject settingsGuideRoot;
    public CanvasGroup settingsCanvasGroup;
    public TMP_Text settingsGuideTextTMP;
    public Text settingsGuideTextUGUI;

    [Header("Entries")]
    public List<StateGuideHintEntry> entries = new List<StateGuideHintEntry>()
    {
        new StateGuideHintEntry
        {
            state = GameState.State1_FreeOnlyChair,
            message = "Đi đến bàn làm việc",
            visibleDuration = 4f,
            showOnlyOnce = true
        }
    };

    [Header("Animation")]
    public float fadeInDuration = 0.35f;
    public float fadeOutDuration = 0.35f;

    [Header("Gameplay Typewriter")]
    public bool enableGameplayTypewriter = true;
    public float gameplayTypewriterCharInterval = 0.04f;
    public AudioSource gameplayTypeAudioSource;
    public AudioClip gameplayTypeClip;
    [Range(0f, 1f)] public float gameplayTypeSfxVolume = 0.15f;
    [Tooltip("Thời gian fade in/out cho type loop.")]
    public float gameplayTypeSfxFadeDuration = 0.06f;

    [Header("Settings Replay")]
    public bool replayLastHintWhenSettingsOpen = true;
    public float replayDurationInSettings = 5f;
    [Tooltip("Khi mở settings, hint luôn hiện cố định (không tự biến mất).")]
    public bool keepHintVisibleInSettings = true;

    private readonly HashSet<GameState> shownStates = new HashSet<GameState>();
    private GameState? lastState;
    private bool lastSettingsOpen;
    private string lastShownMessage = "";
    private float currentAlpha;
    private bool isHintRunning;
    private Coroutine hintCoroutine;
    private Coroutine settingsReplayCoroutine;

    private void Start()
    {
        EnsureTypeAudioSource();
        if (settingsPanelController == null)
        {
            settingsPanelController = FindFirstObjectByType<InGameSettingsPanelController>();
        }
        SetRootsActive(false, false);
        SetAlphaDirect(0f);
        lastSettingsOpen = IsSettingsOpenNow();
    }

    private void Update()
    {
        if (GameFlow.Instance != null)
        {
            GameState current = GameFlow.Instance.currentState;
            if (!lastState.HasValue || current != lastState.Value)
            {
                lastState = current;
                OnStateChanged(current);
            }
        }

        bool settingsOpen = IsSettingsOpenNow();
        if (settingsOpen != lastSettingsOpen)
        {
            lastSettingsOpen = settingsOpen;
            OnSettingsVisibilityChanged(settingsOpen);
        }
    }

    private void OnStateChanged(GameState state)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            StateGuideHintEntry entry = entries[i];
            if (entry == null || entry.state != state) continue;
            if (entry.showOnlyOnce && shownStates.Contains(state)) return;
            if (string.IsNullOrWhiteSpace(entry.message)) return;

            shownStates.Add(state);
            ShowHint(entry.message, Mathf.Max(0.1f, entry.visibleDuration));
            return;
        }
    }

    private void OnSettingsVisibilityChanged(bool settingsOpen)
    {
        // Khi đang có hint active, chuyển nơi hiển thị ngay (gameplay <-> settings).
        if (isHintRunning)
        {
            ApplyActiveTarget(currentAlpha);
        }

        if (!settingsOpen)
        {
            if (settingsReplayCoroutine != null)
            {
                StopCoroutine(settingsReplayCoroutine);
                settingsReplayCoroutine = null;
            }
            if (settingsGuideRoot != null) settingsGuideRoot.SetActive(false);
            if (settingsCanvasGroup != null) settingsCanvasGroup.alpha = 0f;
            return;
        }

        if (!replayLastHintWhenSettingsOpen) return;
        string settingsMessage = GetCurrentStateMessageOrLast();
        if (string.IsNullOrWhiteSpace(settingsMessage)) return;

        if (keepHintVisibleInSettings)
        {
            if (hintCoroutine != null)
            {
                StopCoroutine(hintCoroutine);
                hintCoroutine = null;
                isHintRunning = false;
            }
            ShowPersistentHintInSettings(settingsMessage);
            return;
        }

        if (isHintRunning) return;
        if (settingsReplayCoroutine != null)
        {
            StopCoroutine(settingsReplayCoroutine);
        }
        settingsReplayCoroutine = StartCoroutine(ReplayLastHintInSettingsCoroutine());
    }

    public void ShowHint(string message, float visibleDuration = 4f)
    {
        lastShownMessage = message;

        if (keepHintVisibleInSettings && IsSettingsOpenNow())
        {
            ShowPersistentHintInSettings(message);
            return;
        }

        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
        }
        hintCoroutine = StartCoroutine(HintRoutine(message, visibleDuration));
    }

    private IEnumerator HintRoutine(string message, float visibleDuration)
    {
        isHintRunning = true;
        SetGameplayGuideText(enableGameplayTypewriter ? string.Empty : message);
        SetSettingsGuideText(message);

        int typeIndex = 0;
        float typeTimer = 0f;
        float typeInterval = Mathf.Max(0.005f, gameplayTypewriterCharInterval);

        // Fade in
        float t = 0f;
        while (t < fadeInDuration || (enableGameplayTypewriter && typeIndex < message.Length))
        {
            float dt = Time.unscaledDeltaTime;
            if (t < fadeInDuration)
            {
                t += dt;
                float alpha = fadeInDuration <= 0.001f ? 1f : Mathf.Clamp01(t / fadeInDuration);
                currentAlpha = alpha;
                ApplyActiveTarget(alpha);
            }

            if (enableGameplayTypewriter && typeIndex < message.Length)
            {
                if (!isTypeLoopSfxPlaying)
                {
                    StartGameplayTypeLoopSfx();
                }

                typeTimer += dt;
                while (typeTimer >= typeInterval && typeIndex < message.Length)
                {
                    typeTimer -= typeInterval;
                    typeIndex++;
                    string typed = message.Substring(0, typeIndex);
                    SetGameplayGuideText(typed);
                }
            }

            yield return null;
        }
        currentAlpha = 1f;
        ApplyActiveTarget(1f);
        if (enableGameplayTypewriter && typeIndex < message.Length)
        {
            SetGameplayGuideText(message);
        }
        StopGameplayTypeLoopSfx();

        // Hold
        float hold = 0f;
        while (hold < visibleDuration)
        {
            hold += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade out
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = fadeOutDuration <= 0.001f ? 0f : 1f - Mathf.Clamp01(t / fadeOutDuration);
            currentAlpha = alpha;
            ApplyActiveTarget(alpha);
            yield return null;
        }

        currentAlpha = 0f;
        SetRootsActive(false, false);
        SetAlphaDirect(0f);
        isHintRunning = false;
        hintCoroutine = null;
    }

    private IEnumerator ReplayLastHintInSettingsCoroutine()
    {
        if (settingsGuideRoot == null) yield break;

        string settingsMessage = GetCurrentStateMessageOrLast();
        SetGuideText(settingsMessage);
        SetSettingsGuideText(settingsMessage);
        SetRootsActive(false, true);

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = fadeInDuration <= 0.001f ? 1f : Mathf.Clamp01(t / fadeInDuration);
            if (settingsCanvasGroup != null) settingsCanvasGroup.alpha = alpha;
            yield return null;
        }

        if (settingsCanvasGroup != null) settingsCanvasGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, replayDurationInSettings));

        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = fadeOutDuration <= 0.001f ? 0f : 1f - Mathf.Clamp01(t / fadeOutDuration);
            if (settingsCanvasGroup != null) settingsCanvasGroup.alpha = alpha;
            yield return null;
        }

        if (settingsCanvasGroup != null) settingsCanvasGroup.alpha = 0f;
        if (settingsGuideRoot != null) settingsGuideRoot.SetActive(false);
        settingsReplayCoroutine = null;
    }

    private void ApplyActiveTarget(float alpha)
    {
        bool showInSettings = IsSettingsOpenNow();
        SetRootsActive(!showInSettings, showInSettings);
        SetAlphaDirect(alpha);
    }

    private void SetGuideText(string text)
    {
        SetGameplayGuideText(text);
        SetSettingsGuideText(text);
    }

    private void SetGameplayGuideText(string text)
    {
        if (gameplayGuideTextTMP != null) gameplayGuideTextTMP.text = text;
        if (gameplayGuideTextUGUI != null) gameplayGuideTextUGUI.text = text;
    }

    private void SetSettingsGuideText(string text)
    {
        if (settingsGuideTextTMP != null) settingsGuideTextTMP.text = text;
        if (settingsGuideTextUGUI != null) settingsGuideTextUGUI.text = text;
    }

    private void SetRootsActive(bool gameplayActive, bool settingsActive)
    {
        if (gameplayGuideRoot != null) gameplayGuideRoot.SetActive(gameplayActive);
        if (settingsGuideRoot != null) settingsGuideRoot.SetActive(settingsActive);
        if (!gameplayActive)
        {
            StopGameplayTypeLoopSfx();
        }
    }

    private void SetAlphaDirect(float alpha)
    {
        if (gameplayCanvasGroup != null) gameplayCanvasGroup.alpha = alpha;
        if (settingsCanvasGroup != null) settingsCanvasGroup.alpha = alpha;
    }

    private void ShowPersistentHintInSettings(string message)
    {
        SetGuideText(message);
        SetRootsActive(false, true);
        if (settingsCanvasGroup != null) settingsCanvasGroup.alpha = 1f;
    }

    private bool IsSettingsOpenNow()
    {
        if (InGameSettingsPanelController.IsAnySettingsOpen)
        {
            return true;
        }

        if (settingsPanelController != null && settingsPanelController.settingsPanelRoot != null)
        {
            return settingsPanelController.settingsPanelRoot.activeInHierarchy;
        }

        return false;
    }

    private string GetCurrentStateMessageOrLast()
    {
        if (GameFlow.Instance == null)
        {
            return lastShownMessage;
        }

        GameState current = GameFlow.Instance.currentState;
        for (int i = 0; i < entries.Count; i++)
        {
            StateGuideHintEntry entry = entries[i];
            if (entry == null || entry.state != current) continue;
            if (string.IsNullOrWhiteSpace(entry.message)) continue;
            return entry.message;
        }

        return lastShownMessage;
    }

    private void EnsureTypeAudioSource()
    {
        if (gameplayTypeAudioSource != null) return;
        gameplayTypeAudioSource = GetComponent<AudioSource>();
        if (gameplayTypeAudioSource == null)
        {
            gameplayTypeAudioSource = gameObject.AddComponent<AudioSource>();
        }

        gameplayTypeAudioSource.playOnAwake = false;
        gameplayTypeAudioSource.spatialBlend = 0f;
    }

    private bool isTypeLoopSfxPlaying;
    private Coroutine typeLoopFadeCoroutine;

    private void StartGameplayTypeLoopSfx()
    {
        if (gameplayTypeAudioSource == null || gameplayTypeClip == null) return;

        if (typeLoopFadeCoroutine != null)
        {
            StopCoroutine(typeLoopFadeCoroutine);
            typeLoopFadeCoroutine = null;
        }

        gameplayTypeAudioSource.clip = gameplayTypeClip;
        gameplayTypeAudioSource.loop = true;
        gameplayTypeAudioSource.volume = 0f;
        if (!gameplayTypeAudioSource.isPlaying)
        {
            gameplayTypeAudioSource.Play();
        }
        isTypeLoopSfxPlaying = true;
        typeLoopFadeCoroutine = StartCoroutine(FadeTypeLoopVolumeCoroutine(Mathf.Clamp01(gameplayTypeSfxVolume)));
    }

    private void StopGameplayTypeLoopSfx()
    {
        if (gameplayTypeAudioSource == null || !gameplayTypeAudioSource.isPlaying)
        {
            isTypeLoopSfxPlaying = false;
            return;
        }

        if (typeLoopFadeCoroutine != null)
        {
            StopCoroutine(typeLoopFadeCoroutine);
        }
        typeLoopFadeCoroutine = StartCoroutine(FadeOutAndStopTypeLoopCoroutine());
    }

    private IEnumerator FadeTypeLoopVolumeCoroutine(float targetVolume)
    {
        float duration = Mathf.Max(0.001f, gameplayTypeSfxFadeDuration);
        float start = gameplayTypeAudioSource.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            gameplayTypeAudioSource.volume = Mathf.Lerp(start, targetVolume, p);
            yield return null;
        }

        gameplayTypeAudioSource.volume = targetVolume;
        typeLoopFadeCoroutine = null;
    }

    private IEnumerator FadeOutAndStopTypeLoopCoroutine()
    {
        float duration = Mathf.Max(0.001f, gameplayTypeSfxFadeDuration);
        float start = gameplayTypeAudioSource.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            gameplayTypeAudioSource.volume = Mathf.Lerp(start, 0f, p);
            yield return null;
        }

        gameplayTypeAudioSource.volume = 0f;
        gameplayTypeAudioSource.Stop();
        isTypeLoopSfxPlaying = false;
        typeLoopFadeCoroutine = null;
    }
}
