using System;

namespace Day29.Persistence
{
    [Serializable]
    public class Day29Data
    {
        public float memoryLevel;
        public bool preservedFamilyPhoto;
        public bool altarCleanPerfect;
        public bool fireMaintainedStable;
        public bool dinnerPreparedCorrectly;
        public bool choseToReturnHome;

        // NPC availability based on memory and choices
        public bool youngerSisterPresent;
        public bool elderPresent;
    }
}
