using UnityEngine;
using System.Collections.Generic;
using Day29.Core;

namespace Day29.Tasks
{
    public class PhotoArrangementTask : MonoBehaviour
    {
        public static PhotoArrangementTask Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int requiredPhotos = 3;
        [SerializeField] private List<int> familyPhotoIndices; // Indices of "good" photos
        [SerializeField] private List<int> deceasedMemberIndices; // Indices of "important" photos that shouldn't be ignored

        private List<int> selectedPhotos = new List<int>();
        private bool isCompleted = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void SelectPhoto(int photoId)
        {
            if (isCompleted) return;

            if (selectedPhotos.Contains(photoId))
            {
                selectedPhotos.Remove(photoId);
            }
            else if (selectedPhotos.Count < requiredPhotos)
            {
                selectedPhotos.Add(photoId);
            }

            CheckCompletion();
        }

        private void CheckCompletion()
        {
            if (selectedPhotos.Count == requiredPhotos)
            {
                ApplyMemoryEffects();
                isCompleted = true;
                TaskManager.Instance.CompleteTask("PhotoArrangement");
            }
        }

        private void ApplyMemoryEffects()
        {
            float memoryBonus = 0;
            
            // Logic based on requirements:
            bool hasFamilyPhoto = false;
            foreach (int id in selectedPhotos)
            {
                if (familyPhotoIndices.Contains(id)) hasFamilyPhoto = true;
            }

            if (hasFamilyPhoto) memoryBonus += 10;
            
            // Check if any deceased member was ignored (if they were among the 6, but not selected)
            bool ignoredDeceased = false;
            foreach (int id in deceasedMemberIndices)
            {
                if (!selectedPhotos.Contains(id)) ignoredDeceased = true;
            }
            
            if (ignoredDeceased) memoryBonus -= 5;

            MemoryManager.Instance.AddMemory(memoryBonus);
        }

        public bool IsSelected(int photoId) => selectedPhotos.Contains(photoId);
    }
}
