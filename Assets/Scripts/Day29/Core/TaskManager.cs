using System.Collections.Generic;
using UnityEngine;
using System;

namespace Day29.Core
{
    public class TaskManager : MonoBehaviour
    {
        public static TaskManager Instance { get; private set; }

        private Dictionary<DayPhase, List<string>> phaseTasks = new Dictionary<DayPhase, List<string>>();
        private HashSet<string> completedTasks = new HashSet<string>();

        public event Action<string> OnTaskCompleted;

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
            
            InitializeTasks();
        }

        private void InitializeTasks()
        {
            phaseTasks[DayPhase.Morning] = new List<string> { "PhotoArrangement", "AltarCleaning" };
            phaseTasks[DayPhase.Afternoon] = new List<string> { "BanhTetFire", "DinnerPrep" };
            phaseTasks[DayPhase.Night] = new List<string> { "SisterDialogue" };
        }

        public void CompleteTask(string taskId)
        {
            if (!completedTasks.Contains(taskId))
            {
                completedTasks.Add(taskId);
                OnTaskCompleted?.Invoke(taskId);
                Debug.Log($"[TaskManager] Task Completed: {taskId}");
                CheckPhaseTransition();
            }
        }

        private void CheckPhaseTransition()
        {
            DayPhase currentPhase = PhaseManager.Instance.GetCurrentPhase();
            if (phaseTasks.ContainsKey(currentPhase))
            {
                bool allDone = true;
                foreach (var task in phaseTasks[currentPhase])
                {
                    if (!completedTasks.Contains(task))
                    {
                        allDone = false;
                        break;
                    }
                }

                if (allDone)
                {
                    Debug.Log($"[TaskManager] All tasks for {currentPhase} completed.");
                    PhaseManager.Instance.AdvancePhase();
                }
            }
        }

        public bool IsTaskCompleted(string taskId) => completedTasks.Contains(taskId);
    }
}
