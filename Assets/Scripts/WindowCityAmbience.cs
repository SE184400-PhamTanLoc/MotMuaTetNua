using UnityEngine;

/// <summary>
/// Ambient tiếng thành phố phát từ vị trí cửa sổ (3D audio).
/// - Lại gần cửa sổ: nghe to hơn theo khoảng cách.
/// - Khi ngồi ghế / vào flow máy tính: tự giảm xuống rất nhỏ.
/// </summary>
public class WindowCityAmbience : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource cityAudioSource;
    public AudioClip cityLoopClip;
    [Range(0f, 1f)] public float baseVolume = 0.65f;
    [Range(0f, 1f)] public float seatedVolumeMultiplier = 0.08f;
    public float volumeFadeSpeed = 1.6f;

    [Header("3D Distance")]
    public float minDistance = 1.2f;
    public float maxDistance = 16f;

    [Header("Optional")]
    [Tooltip("Nếu bật, audio này sẽ đi theo kênh SFX slider.")]
    public bool bindToSfxChannel = true;
    [Tooltip("Fade in ambience đồng bộ với fade in của RoomScene.")]
    public bool syncWithRoomFadeIn = true;
    [Tooltip("Nếu > 0 thì dùng giá trị này thay vì tự lấy từ RoomSceneFadeIn.")]
    public float roomFadeInDurationOverride = -1f;

    private float sceneFadeElapsed;
    private float sceneFadeDuration;

    private void Awake()
    {
        EnsureAudioSource();
        Apply3DSettings();
        EnsureChannelVolumeIfNeeded();
    }

    private void Start()
    {
        if (cityAudioSource == null || cityLoopClip == null) return;

        cityAudioSource.clip = cityLoopClip;
        cityAudioSource.loop = true;
        cityAudioSource.volume = 0f;
        SetupSceneFadeDuration();
        if (!cityAudioSource.isPlaying)
        {
            cityAudioSource.Play();
        }
    }

    private void Update()
    {
        if (cityAudioSource == null || cityLoopClip == null) return;

        float stateMultiplier = ShouldBeVeryQuiet() ? seatedVolumeMultiplier : 1f;
        float sceneFadeMultiplier = GetSceneFadeMultiplier();
        float target = Mathf.Clamp01(baseVolume * stateMultiplier * sceneFadeMultiplier);

        cityAudioSource.volume = Mathf.MoveTowards(
            cityAudioSource.volume,
            target,
            Mathf.Max(0.01f, volumeFadeSpeed) * Time.deltaTime
        );
    }

    private bool ShouldBeVeryQuiet()
    {
        if (GameFlow.Instance == null) return false;

        return GameFlow.Instance.IsState(GameState.SittingAtDesk) ||
               GameFlow.Instance.IsState(GameState.ComputerActive) ||
               GameFlow.Instance.IsState(GameState.ComputerFinished) ||
               GameFlow.Instance.IsState(GameState.AlbumFocus) ||
               GameFlow.Instance.IsState(GameState.AlbumReading) ||
               GameFlow.Instance.IsState(GameState.AlbumInteractable);
    }

    private void EnsureAudioSource()
    {
        if (cityAudioSource == null)
        {
            cityAudioSource = GetComponent<AudioSource>();
            if (cityAudioSource == null)
            {
                cityAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        cityAudioSource.playOnAwake = false;
        cityAudioSource.spatialBlend = 1f;
        cityAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
    }

    private void Apply3DSettings()
    {
        if (cityAudioSource == null) return;
        cityAudioSource.minDistance = Mathf.Max(0.1f, minDistance);
        cityAudioSource.maxDistance = Mathf.Max(cityAudioSource.minDistance + 0.1f, maxDistance);
    }

    private void EnsureChannelVolumeIfNeeded()
    {
        if (!bindToSfxChannel || cityAudioSource == null) return;

        AudioChannelVolume channelVolume = cityAudioSource.GetComponent<AudioChannelVolume>();
        if (channelVolume == null)
        {
            channelVolume = cityAudioSource.gameObject.AddComponent<AudioChannelVolume>();
        }

        channelVolume.channel = AudioChannelType.Sfx;
        channelVolume.useAudioSourceVolumeAsBaseOnAwake = false;
        channelVolume.baseVolume = Mathf.Max(channelVolume.baseVolume, baseVolume);
        channelVolume.ApplyCurrentVolume();
    }

    private void SetupSceneFadeDuration()
    {
        if (!syncWithRoomFadeIn)
        {
            sceneFadeDuration = 0f;
            sceneFadeElapsed = 0f;
            return;
        }

        if (roomFadeInDurationOverride > 0f)
        {
            sceneFadeDuration = roomFadeInDurationOverride;
            sceneFadeElapsed = 0f;
            return;
        }

        RoomSceneFadeIn roomFade = FindFirstObjectByType<RoomSceneFadeIn>();
        sceneFadeDuration = roomFade != null ? Mathf.Max(0f, roomFade.fadeInDuration) : 0f;
        sceneFadeElapsed = 0f;
    }

    private float GetSceneFadeMultiplier()
    {
        if (!syncWithRoomFadeIn || sceneFadeDuration <= 0.001f)
        {
            return 1f;
        }

        sceneFadeElapsed += Time.deltaTime;
        return Mathf.Clamp01(sceneFadeElapsed / sceneFadeDuration);
    }
}
