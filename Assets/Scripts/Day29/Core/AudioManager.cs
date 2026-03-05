using UnityEngine;
using UnityEngine.Audio;

namespace Day29.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private string ambientParam = "AmbientVol";
        [SerializeField] private string voiceParam = "VoiceVol";
        [SerializeField] private string masterLowPassParam = "MasterLPF";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            MemoryManager.Instance.OnMemoryChanged += HandleMemoryChanged;
            HandleMemoryChanged(MemoryManager.Instance.GetMemoryLevel());
        }

        private void OnDestroy()
        {
            if (MemoryManager.Instance != null)
            {
                MemoryManager.Instance.OnMemoryChanged -= HandleMemoryChanged;
            }
        }

        private void HandleMemoryChanged(float memoryLevel)
        {
            // Adjust volumes and filters based on memory level
            float volumeDb = -40f * (1f - (memoryLevel / 100f));
            mainMixer.SetFloat(ambientParam, volumeDb);
            
            float cutoffFreq = 22000f; // Default max
            if (memoryLevel < 40)
            {
                cutoffFreq = 2500f + (3500f - 2500f) * (memoryLevel / 40f);
            }
            else if (memoryLevel < 70)
            {
                cutoffFreq = 5000f + (22000f - 5000f) * ((memoryLevel - 40f) / 30f);
            }
            
            mainMixer.SetFloat(masterLowPassParam, cutoffFreq);
        }

        public void PlaySound(AudioSource source, AudioClip clip, float volume = 1f)
        {
            if (source != null && clip != null)
            {
                source.volume = volume;
                source.PlayOneShot(clip);
            }
        }
    }
}
