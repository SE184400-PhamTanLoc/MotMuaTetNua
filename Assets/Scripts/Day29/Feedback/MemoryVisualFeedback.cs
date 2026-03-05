using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Day29.Core;

namespace Day29.Feedback
{
    public class MemoryVisualFeedback : MonoBehaviour
    {
        [Header("Post Processing")]
        [SerializeField] private Volume postProcessVolume;
        private ColorAdjustments colorAdjustments;

        [Header("Lighting")]
        [SerializeField] private Light globalLight;
        [SerializeField] private float maxIntensity = 1.0f;
        [SerializeField] private float minIntensity = 0.5f;

        [Header("NPCs")]
        [SerializeField] private GameObject[] npcs;

        private void Start()
        {
            if (postProcessVolume.profile.TryGet(out colorAdjustments))
            {
                MemoryManager.Instance.OnMemoryChanged += ApplyVisualEffects;
                ApplyVisualEffects(MemoryManager.Instance.GetMemoryLevel());
            }
        }

        private void OnDestroy()
        {
            if (MemoryManager.Instance != null)
            {
                MemoryManager.Instance.OnMemoryChanged -= ApplyVisualEffects;
            }
        }

        private void ApplyVisualEffects(float memoryLevel)
        {
            // 1. Saturation
            float saturationValue = 0f;
            if (memoryLevel < 40) saturationValue = -40f;
            else if (memoryLevel < 70) saturationValue = -15f;
            
            colorAdjustments.saturation.value = saturationValue;

            // 2. Light Intensity
            float intensityT = memoryLevel / 100f;
            globalLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, intensityT);

            // 3. NPC Activation
            if (memoryLevel < 40)
            {
                if (npcs.Length > 0) npcs[0].SetActive(false);
                if (npcs.Length > 1) npcs[1].SetActive(false);
            }
            else if (memoryLevel < 70)
            {
                if (npcs.Length > 0) npcs[0].SetActive(true);
                if (npcs.Length > 1) npcs[1].SetActive(false);
            }
            else
            {
                foreach (var npc in npcs) npc.SetActive(true);
            }
        }
    }
}
