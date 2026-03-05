using UnityEngine;
using Day29.Core;

namespace Day29.Tasks
{
    public class SisterDialogueTask : MonoBehaviour
    {
        public bool ChoseToReturnHome { get; private set; }
        private bool isCompleted = false;

        public void MakeChoice(bool willReturn)
        {
            if (isCompleted) return;

            ChoseToReturnHome = willReturn;
            ApplyMemoryEffects();
            isCompleted = true;
            TaskManager.Instance.CompleteTask("SisterDialogue");
        }

        private void ApplyMemoryEffects()
        {
            float memoryBonus = ChoseToReturnHome ? 10f : -5f;
            MemoryManager.Instance.AddMemory(memoryBonus);
            
            // Brightness multiplier would be applied here or in MemoryVisualFeedback
        }
    }
}
