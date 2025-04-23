using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;
using Unpuzzle.Game.Data;
using static PinOutLevelConfig;
using Random = UnityEngine.Random;

namespace Unpuzzle
{
    public class PinOutLevel : MonoBehaviour
    {
        private const float timePinFly = 0.5f;
        private const float levelShowTime = 1.5f;
        private const float rotateAngle = 45f;
        [SerializeField] private List<HookController> _tutorialHooks;
        [SerializeField] private Transform _centerPivot;
        [SerializeField] private IceBlockTutorial _iceBlockTutorial;

        [SerializeField] private List<HookNode> _tutorialNodes;

        private readonly List<IIceBlock> _iceBlocks = new();
        private readonly List<PinBoltPlank> _pinBoltsPlanks = new();
        private AbstractGameController _controller;

        private bool _firstMoveDone;

        private bool _isComplete;

        private bool _isFailed;

        private LevelModeType _levelMode;

        private int _timeCount;
        private bool isReversed;

        private int levelRage;

        [NonSerialized] public PinOutLevelData SavedData;

        public int MoveCount { get; private set; }

        public Transform CenterPivot
        {
            get => _centerPivot;
            set => _centerPivot = value;
        }

        public List<HookController> Tutorial => _tutorialHooks;
        public IceBlockTutorial IceBlockTutorial => _iceBlockTutorial;

        public bool HasFrozenPins => _iceBlocks.Count > 0;

        public bool HasGhostPins => Hooks.Any(a => a.IsGhost);

        public List<HookController> Hooks { get; private set; }

        public Vector3 VisualCenter { get; private set; }

        public HintLevelList Hints { get; private set; }

        public bool TargetEnabled { get; private set; }

        public float PlayersTime { get; private set; } = 0f;
        public int PlayersMoves { get; private set; }

        public bool IsControllBlocked { get; private set; }

        public event Action<HookController, bool, bool> OnHookRemoved = (hook, complete, removeByBooster) => { };
        public event Action<Transform> OnKeyFound = key => { };
        public event Action OnMoveDone = () => { };

        public void AddIceBlock(IIceBlock iceBlock)
        {
            _iceBlocks.Add(iceBlock);
        }

        public void AddPinBoltPlank(PinBoltPlank pinBoltPlanks)
        {
            _pinBoltsPlanks.Add(pinBoltPlanks);
        }

        public void SetPinList(List<HookController> pins)
        {
            Hooks = pins;
        }

        public void Init(int moves, int time, bool targetEnabled, bool isMoveOverrided, LevelModeType levelMode,
            AbstractGameController gameController)
        {
            _controller = gameController;
            _levelMode = levelMode;
            MoveCount = moves;
            _timeCount = time;
            TargetEnabled = targetEnabled;

            Hooks.ForEach(a => a._pinOutLevel = this);
            Hooks.ForEach(a =>
            {
                a.OnMoveOut = OnHookMoveOut;
                a.IsTarget = a.IsTarget && targetEnabled;
                a.IsContainKey = a.IsContainKey;
                a.Initialize();
            });

            var frozenCountMoves = _iceBlocks.Sum(a => a.IceBreakeCount);

            var movesToComplete = Hooks.Count + frozenCountMoves;
            if (MoveCount < Hooks.Count)
            {
                if (MoveCount <= 0)
                {
                    MoveCount = Hooks.Count + frozenCountMoves;
                }
                else
                {
                    MoveCount += frozenCountMoves;
                }
            }

            if (MoveCount < movesToComplete)
            {
                MoveCount = movesToComplete;
            }

            if (isMoveOverrided)
            {
                MoveCount = moves;
            }

            if (DebugPanel.isMoveOverride)
            {
                MoveCount = DebugPanel.moveCount;
            }


            if (!TargetEnabled)
            {
                return;
            }

            var targetCount = 0;

            Hooks.ForEach(a => targetCount += a.GetTargetCount());
        }

        public HintLevelList GetHintPinList1()
        {
            var hintList = new HintLevelList { hintList = new List<HintLevelListItem>() };
            foreach (var node in _tutorialNodes)
            {
                if (node == null ||
                    node.hook == null)
                {
                    continue;
                }

                for (var i = 0; i < node.nexts.Count; i++)
                {
                    var dir = node.nexts[i];
                    var found = false;
                    for (var j = 0; j < dir.data.Count; j++)
                    {
                        var hooks = dir.data;
                        if (!hooks.Any(h => h && !h.Removed))
                        {
                            hintList.hintList.Add(new HintLevelListItem
                            {
                                brokeIceBlockStep = false, targetPin = node.hook, frozenPin = null
                            });
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        break;
                    }
                }
            }

            return hintList;
        }

        public HintLevelList GetHintPinList()
        {
            var hintList = new HintLevelList { hintList = new List<HintLevelListItem>() };

            if (IceBlockTutorial != null)
            {
                foreach (var sequence in IceBlockTutorial.tutorialSequence)
                {
                    if (sequence.pin == null)
                    {
                        continue;
                    }

                    hintList.hintList.Add(new HintLevelListItem
                    {
                        brokeIceBlockStep = sequence.iceBlock != null,
                        targetPin = sequence.pin,
                        frozenPin = sequence.iceBlock
                    });
                }
            }
            else
            {
                foreach (var pin in _tutorialHooks)
                {
                    if (pin == null)
                    {
                        continue;
                    }

                    hintList.hintList.Add(new HintLevelListItem
                    {
                        brokeIceBlockStep = false, targetPin = pin, frozenPin = null
                    });
                }
            }

            return hintList;
        }

        public Vector3 CalculateVisualCenter()
        {
            VisualCenter = Vector3.zero;

            Hooks.ForEach(a =>
            {
                VisualCenter += a.transform.localPosition;
            });

            VisualCenter = VisualCenter / Hooks.Count;

            return VisualCenter;
        }

        public void SetHintPinList(HintLevelList hintList)
        {
            Hints = hintList;
        }

        public void ShowAnimation()
        {
            Debug.Log("Show animation");
            var hooks = Hooks.OrderBy(a => a.transform.localPosition.sqrMagnitude);
            var lockedContolCount = 0;
            var delay = 0f;

            var time = timePinFly;

            var maxDelay = Mathf.Max(levelShowTime - time, 0f);

            var delayAddictive = maxDelay / hooks.Count();

            var hookIndex = 0;

            lockedContolCount++;
            IsControllBlocked = true;

            var rotation = transform.localRotation.eulerAngles;
            rotation.y += rotateAngle;
            transform.localScale = Vector3.one * 0.01f;

            transform.localRotation = Quaternion.Euler(rotation);

            transform.DOLocalRotate(Vector3.zero, levelShowTime)
                .SetEase(Ease.InOutSine);
            transform.DOScale(1f, levelShowTime)
                .SetEase(LevelsController.Instance.Curve)
                .OnComplete(() =>
                {
                    lockedContolCount--;
                    if (lockedContolCount == 0)
                    {
                        IsControllBlocked = false;
                    }
                });
        }

        public float GetHeightOfLevel()
        {
            var maxOffset = 0f;

            for (var i = 0; i < Hooks.Count; i++)
            {
                var boundPivots = Hooks[i].BoundPivots;
                foreach (var pivot in boundPivots)
                {
                    var pos = Mathf.Abs(pivot.transform.localPosition.y);

                    if (pos > maxOffset)
                    {
                        maxOffset = pos;
                    }
                }
            }

            return maxOffset;
        }

        public float GetMaxScreenBoundOffset(Camera camera)
        {
            var maxOffset = 0f;
            var middlePos = camera.pixelWidth / 2f;

            for (var i = 0; i < Hooks.Count; i++)
            {
                var boundPivots = Hooks[i].BoundPivots;
                foreach (var pivot in boundPivots)
                {
                    var pivotPos = camera.WorldToScreenPoint(pivot.position);
                    maxOffset = Mathf.Max(Mathf.Abs(pivotPos.x - middlePos), maxOffset);
                }
            }

            return maxOffset * 2f;
        }

        public float GetMaxScreenBoundOffsetHeight(Camera camera)
        {
            var maxOffset = 0f;
            var middlePos = camera.pixelHeight / 2f;

            for (var i = 0; i < Hooks.Count; i++)
            {
                var boundPivots = Hooks[i].BoundPivots;
                foreach (var pivot in boundPivots)
                {
                    var pivotPos = camera.WorldToScreenPoint(pivot.position);
                    maxOffset = Mathf.Max(Mathf.Abs(pivotPos.y - middlePos), maxOffset);
                }
            }

            return maxOffset * 2f;
        }

        public void MoveDone()
        {
            PlayersMoves++;
            OnMoveDone();
            LevelsController.Instance.MoveDone();
        }

        public void StartLevel()
        {
            Hooks.ForEach(a => a.CheckAndSetBlockedByFrozen());

            _iceBlocks.ForEach(a => a.Initialize());
            _pinBoltsPlanks.ForEach(a => a.Initialize());
        }

        public void MovingBlocked()
        {
            levelRage = 0;
            isReversed = false;
        }

        private void OnHookMoveOut(HookController hook, bool removedByBooster)
        {
            if (Hooks.Remove(hook))
            {
                _firstMoveDone = true;

                if (Hooks.Count == 0)
                {
                    if (_isComplete)
                    {
                        return;
                    }

                    _isComplete = true;
                    OnHookRemoved(hook, true, removedByBooster);
                    return;
                }

                OnHookRemoved(hook, false, removedByBooster);
            }
        }

        public List<HookController> BoosterRemoveGetPins(float count)
        {
            var hookCount = count;

            var removedPins = new List<HookController>();

            if (Hooks.Count == 0 || hookCount <= 0)
            {
                return removedPins;
            }

            var removePinsLeft = hookCount;

            if (removePinsLeft > 0)
            {
                var defaultPins = Hooks.FindAll(a => !a.Removed).ToList();

                if (defaultPins.Count > 0)
                {
                    for (var i = 0; i < removePinsLeft; i++)
                    {
                        if (defaultPins.Count < 1)
                        {
                            break;
                        }

                        var randomPin = Random.Range(0, defaultPins.Count);
                        removedPins.Add(defaultPins[randomPin]);
                        defaultPins.RemoveAt(randomPin);
                    }
                }
            }

            return removedPins;
        }

        public List<IIceBlock> BoosterRemoveGetIceBlocks(float count)
        {
            var hookCount = count;

            var removeIceBlock = new List<IIceBlock>();

            if (_iceBlocks.Count == 0 || hookCount <= 0)
            {
                return removeIceBlock;
            }

            var iceBlocksToRemove = _iceBlocks.FindAll(a => a.IsActive).ToList();

            var iceBLockToRemoveCount = hookCount;

            while (iceBlocksToRemove.Count > 0 && iceBLockToRemoveCount > 0)
            {
                var iceBlockToRemove = iceBlocksToRemove[iceBlocksToRemove.Count - 1];
                iceBlocksToRemove.RemoveAt(iceBlocksToRemove.Count - 1);

                removeIceBlock.Add(iceBlockToRemove);


                iceBLockToRemoveCount--;
            }

            return removeIceBlock;
        }

        public void OnPreMove()
        {
            if (LevelsController.Instance.LevelType == LevelType.Normal)
            {
                GameStateSaveUtils.SaveData(SavedData, LevelData.GetLevelNumber(), _controller.CurrentMovesCount + 1);
            }
        }

        [ContextMenu("Count pins")]
        private void CountText()
        {
            var pins = GetComponentsInChildren<HookController>().ToList();
            var iceBlocks = GetComponentsInChildren<IceBlockController>().ToList();
            var planks = GetComponentsInChildren<PinBoltPlank>().ToList();

            var stringBuilder = new StringBuilder(name + "\n");

            stringBuilder.AppendLine("PINS COUNT: " + pins.Count);

            var types = Enum.GetValues(typeof(PinType)).Cast<PinType>().ToList();

            foreach (var item in types)
            {
                stringBuilder.AppendLine(item.ToString().ToUpper() + " " + "PINS: " +
                                         pins.FindAll(a => a.pinType == item).Count);
            }

            stringBuilder.AppendLine("FIRED PINS: " + pins.FindAll(a => a.IsFire).Count);
            stringBuilder.AppendLine("GHOST PINS: " + pins.FindAll(a => a.IsGhost).Count);
            stringBuilder.AppendLine("ACTIVATOR PINS: " + pins.FindAll(a => a.BlockedPins.Count > 0).Count);


            stringBuilder.AppendLine("ICE BLOCKS: " + iceBlocks.Count);
            stringBuilder.AppendLine("PLANKS: " + planks.Count);

            Debug.Log(stringBuilder.ToString());
        }
    }
}

public class HintLevelList
{
    public List<HintLevelListItem> hintList;
}

public class HintLevelListItem
{
    public bool brokeIceBlockStep;
    public HookController frozenPin;
    public HookController targetPin;
}
