using System;
using UnityEngine;

public enum AudioChannelType
{
    Music,
    Sfx
}

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    public const string MasterVolumePrefKey = "audio_master_volume";
    public const string MusicVolumePrefKey = "audio_music_volume";
    public const string SfxVolumePrefKey = "audio_sfx_volume";

    public event Action VolumesChanged;

    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;

    public static AudioSettingsManager InstanceOrCreate()
    {
        if (Instance != null) return Instance;

        AudioSettingsManager existing = FindFirstObjectByType<AudioSettingsManager>();
        if (existing != null) return existing;

        GameObject go = new GameObject("AudioSettingsManager");
        return go.AddComponent<AudioSettingsManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadFromPrefs();
        ApplyMasterVolume();
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyMasterVolume();
        SaveVolumes();
        VolumesChanged?.Invoke();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        SaveVolumes();
        VolumesChanged?.Invoke();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        SaveVolumes();
        VolumesChanged?.Invoke();
    }

    public float GetChannelVolume(AudioChannelType channel)
    {
        return channel == AudioChannelType.Music ? musicVolume : sfxVolume;
    }

    private void ApplyMasterVolume()
    {
        AudioListener.volume = masterVolume;
    }

    private void LoadFromPrefs()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterVolumePrefKey, 1f);
        musicVolume = PlayerPrefs.GetFloat(MusicVolumePrefKey, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumePrefKey, 1f);
    }

    private void SaveVolumes()
    {
        PlayerPrefs.SetFloat(MasterVolumePrefKey, masterVolume);
        PlayerPrefs.SetFloat(MusicVolumePrefKey, musicVolume);
        PlayerPrefs.SetFloat(SfxVolumePrefKey, sfxVolume);
        PlayerPrefs.Save();
    }
}
