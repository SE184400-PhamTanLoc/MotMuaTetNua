using UnityEngine;
using System;

namespace Day29.Core
{
    public enum DayPhase { Morning, Afternoon, Night }

    public class PhaseManager : MonoBehaviour
    {
        public static PhaseManager Instance { get; private set; }

        [Header("Status")]
        [SerializeField] private DayPhase currentPhase = DayPhase.Morning;

        public event Action<DayPhase> OnPhaseChanged;

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

        public DayPhase GetCurrentPhase() => currentPhase;

        public void TransitionToPhase(DayPhase nextPhase)
        {
            if (nextPhase > currentPhase)
            {
                currentPhase = nextPhase;
                OnPhaseChanged?.Invoke(currentPhase);
                Debug.Log($"[PhaseManager] Transitioned to {currentPhase}");
            }
        }
        
        public void AdvancePhase()
        {
            if (currentPhase < DayPhase.Night)
            {
                TransitionToPhase(currentPhase + 1);
            }
        }
    }
}
