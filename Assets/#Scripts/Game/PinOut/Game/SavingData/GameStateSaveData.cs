using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    [Serializable]
    public class GameStateSaveData
    {
        [SerializeField] public string levelName = string.Empty;

        [SerializeField] public int moves = 1;

        [SerializeField] public int levelNum;

        [SerializeField] public string appVer = string.Empty;

        [SerializeField] public List<int> removedPins = new();

        [SerializeField] public List<int> revealedPins = new();

        [SerializeField] public List<int> brokenPlanks = new();

        [SerializeField] public List<int> brokenIce = new();

        public GameStateSaveData() { }

        public GameStateSaveData(PinOutLevelData savedData, int level, int movesLeft)
        {
            levelNum = level;
            levelName = savedData.name;
            appVer = Application.version;
            moves = movesLeft;

            foreach (var item in savedData.elements)
            {
                removedPins.Add(item.sIsDestroyed);
                revealedPins.Add(item.sRevealed);
            }

            foreach (var item in savedData.pinBoltsPlanks)
            {
                brokenPlanks.Add(item.sIsDestroyed);
            }

            foreach (var item in savedData.iceBlocks)
            {
                brokenIce.Add(item.sIsDestroyed);
            }
        }
    }
}
