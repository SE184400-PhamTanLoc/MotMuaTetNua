using UnityEngine;
using System;

namespace Day29.Core
{
    public class MemoryManager : MonoBehaviour
    {
        public static MemoryManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField, Range(0, 100)] private float memoryLevel = 100f;
        
        public event Action<float> OnMemoryChanged;

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

        public float GetMemoryLevel() => memoryLevel;

        public void AddMemory(float amount)
        {
            memoryLevel = Mathf.Clamp(memoryLevel + amount, 0, 100);
            OnMemoryChanged?.Invoke(memoryLevel);
            Debug.Log($"[MemoryManager] Memory Level: {memoryLevel}");
        }

        public void SubtractMemory(float amount)
        {
            memoryLevel = Mathf.Clamp(memoryLevel - amount, 0, 100);
            OnMemoryChanged?.Invoke(memoryLevel);
            Debug.Log($"[MemoryManager] Memory Level: {memoryLevel}");
        }
    }
}
