using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioChannelVolume : MonoBehaviour
{
    public AudioChannelType channel = AudioChannelType.Sfx;
    [Tooltip("Âm lượng gốc của AudioSource trước khi nhân với slider.")]
    public float baseVolume = 1f;
    public bool useAudioSourceVolumeAsBaseOnAwake = true;

    private AudioSource source;
    private AudioSettingsManager manager;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        if (useAudioSourceVolumeAsBaseOnAwake)
        {
            baseVolume = source.volume;
        }
    }

    private void OnEnable()
    {
        manager = AudioSettingsManager.InstanceOrCreate();
        manager.VolumesChanged += ApplyCurrentVolume;
        ApplyCurrentVolume();
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.VolumesChanged -= ApplyCurrentVolume;
        }
    }

    public void ApplyCurrentVolume()
    {
        if (source == null || manager == null) return;
        float channelVolume = manager.GetChannelVolume(channel);
        source.volume = Mathf.Clamp01(baseVolume * channelVolume);
    }
}
