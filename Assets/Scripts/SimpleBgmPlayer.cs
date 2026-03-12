using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SimpleBgmPlayer : MonoBehaviour
{
    private AudioSource source;
    private float baseVolume = 0.8f;

    public void Setup(AudioClip clip, float volume = 0.8f)
    {
        source = GetComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D Sound
        baseVolume = volume;

        EnsureMusicChannelVolume();

        if (!source.isPlaying)
        {
            source.Play();
        }
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
        channelVolume.baseVolume = baseVolume;
        channelVolume.ApplyCurrentVolume();
    }
}
