using System.Collections.Generic;
using System.Text;
using ModestTree;
using UnityEngine;

namespace Unpuzzle
{
    public static class PinOutLevelConverter
    {
        public static PinOutLevelData GetLevelData(PinOutLevel level)
        {
            var pins = level.GetComponentsInChildren<HookController>();
            var IceBlocks = level.GetComponentsInChildren<IceBlockController>();
            var PinBolts = level.GetComponentsInChildren<PinBoltPlank>();

            var levelData = new PinOutLevelData();

            levelData.name = level.name;

            levelData.elements = new List<PinOutLevelDataElement>();

            levelData.customPivot = level.CenterPivot != null;

            var hintList = level.GetHintPinList();
            var hintPinOrder = new StringBuilder();
            var hintPinFrozenPairOrder = new StringBuilder();

            for (var i = 0; i < hintList.hintList.Count; i++)
            {
                var step = hintList.hintList[i];

                if (step.targetPin == null)
                {
                    continue;
                }

                if (i > 0)
                {
                    hintPinOrder.Append(';');
                    hintPinFrozenPairOrder.Append(';');
                }

                hintPinOrder.Append(pins.IndexOf(step.targetPin));

                if (step.frozenPin == null)
                {
                    hintPinFrozenPairOrder.Append(-1);
                    continue;
                }

                hintPinFrozenPairOrder.Append(pins.IndexOf(step.frozenPin));
            }

            levelData.hintsOrder = hintPinOrder.ToString();
            levelData.hintFrozenBlockPairOrder = hintPinFrozenPairOrder.ToString();

            if (levelData.customPivot)
            {
                levelData.offset = level.CenterPivot.localPosition;
            }

            foreach (var pin in pins)
            {
                var pinData = GetElementData(pin, level.transform);


                if (pin.BlockedPins.Count > 0)
                {
                    pinData.blckPins = new List<int>();
                    foreach (var blockedPin in pin.BlockedPins)
                    {
                        var blockedPinIndex = pins.IndexOf(blockedPin);

                        if (blockedPinIndex < 0 || blockedPin == pin)
                        {
                            continue;
                        }

                        pinData.blckPins.Add(blockedPinIndex);
                    }
                }

                if (pinData.isFrzn)
                {
                    pinData.isFrzn = false;
                }

                levelData.elements.Add(pinData);
            }

            foreach (var iceBlock in IceBlocks)
            {
                var iceBlockData = GetIceBlockData(iceBlock, level.transform);
                levelData.iceBlocks.Add(iceBlockData);
            }

            foreach (var pinBolt in PinBolts)
            {
                var pinBolts = GetPinBoltPlankData(pinBolt, level.transform);
                levelData.pinBoltsPlanks.Add(pinBolts);
            }

            return levelData;
        }

        private static IceBlockElementLevelData GetIceBlockData(IceBlockController iceBlock, Transform levelTransform)
        {
            var iceBlockData = new IceBlockElementLevelData();

            iceBlockData.blockType = iceBlock.IceBlockType;
            iceBlockData.position = levelTransform.InverseTransformPoint(iceBlock.transform.position);
            iceBlockData.rotation = iceBlock.transform.rotation.eulerAngles;
            iceBlockData.size = iceBlock.GetSize();
            iceBlockData.punchCount = iceBlock.IceBreakeCount;

            return iceBlockData;
        }

        private static PinBoltPlankData GetPinBoltPlankData(PinBoltPlank pinBolt, Transform levelTransform)
        {
            var pinBoltData = new PinBoltPlankData();

            pinBoltData.pos = levelTransform.InverseTransformPoint(pinBolt.transform.position);
            pinBoltData.rot = pinBolt.transform.rotation.eulerAngles;
            pinBoltData.scl = pinBolt.GetScale();

            return pinBoltData;
        }

        private static PinOutLevelDataElement GetElementData(HookController pin, Transform levelTransform)
        {
            var pinData = new PinOutLevelDataElement();
            pinData.type = pin.pinType;
            pinData.pos = levelTransform.InverseTransformPoint(pin.transform.position);
            pinData.rot = pin.transform.rotation.eulerAngles;
            pinData.colOvrd = pin.IsColorOverrided;
            pinData.colId = pin.OverrideColor;
            pinData.matOvrd = pin.IsMaterialOverrided;
            pinData.matId = pin.OverridedMaterial;
            pinData.hsKey = pin.IsContainKey;
            pinData.hsStr = pin.IsTarget;
            pinData.isFrzn = pin.pinType == PinType.loop ? pin.IsFrozen : false;
            pinData.isFire = pin.IsFire;
            pinData.isGhost = pin.IsGhost;


            var scaler = pin.GetComponent<HookScaler>();

            if (scaler)
            {
                pinData.scaled = scaler.ScaleEnabled;
                pinData.pinScl = new Vector2(1f, scaler.Scale);
            }
            else
            {
                var quadScaler = pin.GetComponent<HookScalerQuad>();
                var cornerScaler = pin.GetComponent<HookScalerCorner>();
                var universalScaler = pin.GetComponent<HookScalerUniversal>();
                var partLineScaler = pin.GetComponent<HookScalerPartLine>();

                if (quadScaler && quadScaler.ScaleEnabled)
                {
                    pinData.scaled = quadScaler.ScaleEnabled;
                    pinData.pinScl = new Vector2(quadScaler.HorizontalScale, quadScaler.VerticalScale);
                }

                if (cornerScaler && cornerScaler.ScaleEnabled)
                {
                    pinData.scaled = cornerScaler.ScaleEnabled;
                    pinData.pinScl = new Vector2(cornerScaler.ScaleX, cornerScaler.ScaleY);
                }

                if (partLineScaler && partLineScaler.ScaleEnabled)
                {
                    pinData.scaled = partLineScaler.ScaleEnabled;
                    pinData.pinScl = new Vector2(partLineScaler.LeftScale, partLineScaler.RightScale);
                }

                if (universalScaler && universalScaler.ScaleEnabled)
                {
                    pinData.scaled = universalScaler.ScaleEnabled;
                    pinData.pinScl = new Vector2(universalScaler.HorizontalScale, universalScaler.VerticalScale);
                }
            }

            var chains = pin.GetComponent<HookChain>();

            if (chains != null)
            {
                var chainTypes = new StringBuilder();
                var chainLenght = new StringBuilder();
                var chainRotation = new StringBuilder();

                var elements = chains.Elements;

                for (var i = 0; i < elements.Count; i++)
                {
                    var element = elements[i];

                    if (i > 0)
                    {
                        chainTypes.Append(';');
                        chainLenght.Append(';');
                        chainRotation.Append(';');
                    }

                    chainTypes.Append((int)element.Type);
                    chainLenght.Append(element.GetSerializeScale());
                    chainRotation.Append(element.transform.localRotation.eulerAngles.y);
                }

                pinData.chains = chainTypes.ToString();
                pinData.chainsScl = chainLenght.ToString();
                pinData.chainRot = chainRotation.ToString();
            }

            return pinData;
        }
    }
}
