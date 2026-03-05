using UnityEngine;
using Day29.Core;

namespace Day29.Tasks
{
    public class AltarCleaningTask : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float totalDirtAmount = 100f;
        [SerializeField] private float wipeSpeed = 10f;
        [SerializeField] private float perfectTimeLimit = 40f;
        [SerializeField] private float goodTimeLimit = 70f;

        private float currentDirt;
        private float timer;
        private bool isCleaning = false;
        private bool isCompleted = false;

        private void Start()
        {
            currentDirt = totalDirtAmount;
        }

        private void Update()
        {
            if (isCleaning && !isCompleted)
            {
                timer += Time.deltaTime;
                // In a real implementation, mouse drag would call a Clean(amount) method
            }
        }

        public void StartCleaning()
        {
            isCleaning = true;
        }

        public void StopCleaning()
        {
            isCleaning = false;
        }

        public void Clean(float amount)
        {
            if (isCompleted) return;

            currentDirt -= amount * wipeSpeed * Time.deltaTime;
            
            if (currentDirt <= 0)
            {
                currentDirt = 0;
                isCompleted = true;
                ApplyMemoryEffects();
                TaskManager.Instance.CompleteTask("AltarCleaning");
            }
        }

        private void ApplyMemoryEffects()
        {
            float memoryBonus = 0;
            if (timer < perfectTimeLimit) memoryBonus = 10;
            else if (timer < goodTimeLimit) memoryBonus = 5;
            else memoryBonus = -10;

            MemoryManager.Instance.AddMemory(memoryBonus);
        }

        public float GetDirtPercentage() => (currentDirt / totalDirtAmount) * 100f;
    }
}
