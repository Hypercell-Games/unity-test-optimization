using System;
using UnityEngine;

namespace Unpuzzle
{
    public class LevelStage
    {
        public Vector3 offset;

        public float stageScale;

        public LevelStage(PinOutLevel level)
        {
            Level = level;
        }

        public PinOutLevel Level { get; }

        public event Action<ELevelCompleteReason, LevelProgress> OnCompleteStage = (reason, progress) => { };

        public void StartStage()
        {
        }
    }
}
