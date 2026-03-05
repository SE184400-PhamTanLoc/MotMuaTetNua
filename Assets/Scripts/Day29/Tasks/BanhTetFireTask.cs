using UnityEngine;
using Day29.Core;

namespace Day29.Tasks
{
    public class BanhTetFireTask : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float minIdealTemp = 45f;
        [SerializeField] private float maxIdealTemp = 65f;
        [SerializeField] private float totalDuration = 120f;
        [SerializeField] private float tempDecreaseSpeed = 2f;
        [SerializeField] private float fuelBoostAmount = 10f;

        private float currentTemp = 50f;
        private float timer = 0f;
        private bool isCompleted = false;
        private bool isUnstable = false;

        private void Update()
        {
            if (isCompleted) return;

            timer += Time.deltaTime;
            currentTemp -= tempDecreaseSpeed * Time.deltaTime * (1f + (timer / totalDuration)); // Gradually faster cooling

            if (currentTemp < minIdealTemp || currentTemp > maxIdealTemp)
            {
                isUnstable = true;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                AddFuel();
            }

            if (timer >= totalDuration)
            {
                isCompleted = true;
                ApplyMemoryEffects();
                TaskManager.Instance.CompleteTask("BanhTetFire");
            }
        }

        private void AddFuel()
        {
            currentTemp += fuelBoostAmount;
            Debug.Log($"[BanhTetFireTask] Temp: {currentTemp}");
        }

        private void ApplyMemoryEffects()
        {
            float memoryBonus = 0;
            if (currentTemp >= minIdealTemp && currentTemp <= maxIdealTemp) memoryBonus = 10;
            else if (!isUnstable) memoryBonus = 5;
            else memoryBonus = -15;

            MemoryManager.Instance.AddMemory(memoryBonus);
        }

        public float GetCurrentTemp() => currentTemp;
        public float GetProgress() => timer / totalDuration;
    }
}
