using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class StartMenuBgmPlayer : MonoBehaviour
{
    [Header("BGM")]
    public AudioClip menuBgmClip;
    [Range(0f, 1f)] public float bgmBaseVolume = 0.8f;
    public bool playOnStart = true;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.mute = false;

        if (menuBgmClip != null)
        {
            source.clip = menuBgmClip;
        }

        EnsureMusicChannelVolume();
    }

    private void Start()
    {
        if (!playOnStart) return;
        if (source == null || source.clip == null) return;
        if (!source.isPlaying) source.Play();
    }

    private void EnsureMusicChannelVolume()
    {
        AudioChannelVolume channelVolume = GetComponent<AudioChannelVolume>();
        if (channelVolume == null)
        {
            channelVolume = gameObject.AddComponent<AudioChannelVolume>();
        }

        channelVolume.channel = AudioChannelType.Music;
        channelVolume.useAudioSourceVolumeAsBaseOnAwake = false;
        channelVolume.baseVolume = bgmBaseVolume;
        channelVolume.ApplyCurrentVolume();
    }
}
