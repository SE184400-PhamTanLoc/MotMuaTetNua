using UnityEngine;
using System.IO;
using Day29.Core;
using Day29.Tasks;

namespace Day29.Persistence
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private string savePath;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            savePath = Path.Combine(Application.persistentDataPath, "Day29Data.json");
        }

        public void SaveDay29()
        {
            Day29Data data = new Day29Data
            {
                memoryLevel = MemoryManager.Instance.GetMemoryLevel(),
                // In a production scenario, these flags would be pulled from their respective task components
                preservedFamilyPhoto = true, // Simplified for now
                altarCleanPerfect = true,
                fireMaintainedStable = true,
                dinnerPreparedCorrectly = true,
                choseToReturnHome = true
            };

            string json = JsonUtility.ToJson(data);
            File.WriteAllText(savePath, json);
            Debug.Log($"[SaveSystem] Day 29 data saved to: {savePath}");
        }

        public Day29Data LoadDay29()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                return JsonUtility.FromJson<Day29Data>(json);
            }
            return null;
        }
    }
}
