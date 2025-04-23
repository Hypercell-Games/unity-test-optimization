using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unpuzzle
{
    public class LevelBuilder : MonoBehaviour
    {
        [SerializeField] private PinOutLevel _levelPrefab;
        [SerializeField] private PinFactory _factory;

        public PinOutLevel GetLevel(PinOutLevelData levelData, GameStateSaveData saveData = null)
        {
            var level = Instantiate(_levelPrefab);

            BuildLevel(level, levelData, saveData);

            return level;
        }

        private void BuildLevel(PinOutLevel level, PinOutLevelData levelData, GameStateSaveData savedData = null)
        {
            var pinList = new List<HookController>();
            level.SavedData = levelData;

            for (var i = 0; i < levelData.elements.Count; i++)
            {
                var pinData = levelData.elements[i];

                if ((savedData != null && savedData.removedPins[i] > 0) || pinData.type == PinType.pinBolt)
                {
                    pinData.sIsDestroyed = 1;
                    pinList.Add(null);
                    continue;
                }

                var pin = _factory.GetPin(pinData.type);

                pin.transform.SetParent(level.transform);

                pin.IsTarget = pinData.hsStr;
                pin.IsContainKey = pinData.hsKey;
                pin.transform.localPosition = pinData.pos;
                pin.transform.rotation = Quaternion.Euler(pinData.rot);


                pin.IsGhost = false;
                pin.IsFire = false;

                pin.SaveData = pinData;

                if (savedData != null)
                {
                    pinData.sIsDestroyed = savedData.revealedPins[i];
                    pin.IsGhost = pin.IsGhost && savedData.revealedPins[i] < 1;
                }

                pin.UpdateValues(pinData);
                pinList.Add(pin);
            }


            for (var i = 0; i < levelData.iceBlocks.Count; i++)
            {
                var iceBlockData = levelData.iceBlocks[i];

                if (savedData != null && savedData.brokenIce[i] > 0)
                {
                    iceBlockData.sIsDestroyed = 1;
                    continue;
                }

                var iceBlock = _factory.GetIceBlockController(iceBlockData.blockType);
                iceBlock.transform.SetParent(level.transform);

                iceBlock.transform.localPosition = iceBlockData.position;
                iceBlock.transform.rotation = Quaternion.Euler(iceBlockData.rotation);
                iceBlock.SetSize(iceBlockData.size);
                iceBlock.IceBreakeCount = iceBlockData.punchCount;
                iceBlock.SaveData = iceBlockData;

                level.AddIceBlock(iceBlock);
            }

            if (levelData.customPivot)
            {
                var customPivot = new GameObject("customPivot");
                customPivot.transform.SetParent(level.transform);
                level.CenterPivot = customPivot.transform;
                customPivot.transform.localPosition = levelData.offset;
            }

            level.SetPinList(pinList.FindAll(a => a != null));


            var hintList = new HintLevelList();

            hintList.hintList = new List<HintLevelListItem>();

            if (!string.IsNullOrEmpty(levelData.hintsOrder))
            {
                var hintOrder = levelData.hintsOrder.Split(';').Select(int.Parse).ToList();
                var hintFrozenPairOrder = levelData.hintFrozenBlockPairOrder.Split(';').Select(int.Parse).ToList();

                for (var i = 0; i < hintOrder.Count; i++)
                {
                    var hintStep = new HintLevelListItem();
                    hintStep.targetPin = pinList[hintOrder[i]];
                    if (hintStep.targetPin == null)
                    {
                        continue;
                    }

                    if (hintFrozenPairOrder[i] < 0)
                    {
                        hintStep.brokeIceBlockStep = false;
                        hintStep.frozenPin = null;
                    }
                    else
                    {
                        hintStep.brokeIceBlockStep = true;
                        hintStep.frozenPin = pinList[hintFrozenPairOrder[i]];
                    }

                    hintList.hintList.Add(hintStep);
                }
            }

            level.SetHintPinList(hintList);
        }
    }
}
