using UnityEngine;
using System.Collections.Generic;
using Day29.Core;

namespace Day29.Tasks
{
    public class DinnerPrepTask : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int totalDishes = 5;
        [SerializeField] private List<int> correctSlotIndices; // Map dish index to slot index

        private Dictionary<int, int> currentPlacements = new Dictionary<int, int>();
        private bool isCompleted = false;

        public void PlaceDish(int dishId, int slotId)
        {
            if (isCompleted) return;

            currentPlacements[dishId] = slotId;

            if (currentPlacements.Count == totalDishes)
            {
                ApplyMemoryEffects();
                isCompleted = true;
                TaskManager.Instance.CompleteTask("DinnerPrep");
            }
        }

        private void ApplyMemoryEffects()
        {
            int correctCount = 0;
            foreach (var placement in currentPlacements)
            {
                if (correctSlotIndices.Contains(placement.Value)) // Simplified check
                {
                    correctCount++;
                }
            }

            float memoryBonus = 0;
            if (correctCount == totalDishes) memoryBonus = 10;
            else if (correctCount >= totalDishes / 2) memoryBonus = 5;
            else memoryBonus = -10;

            MemoryManager.Instance.AddMemory(memoryBonus);
        }

        public bool IsCompleted() => isCompleted;
    }
}
