using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Unpuzzle;
using Unpuzzle.Game.Data;
using static PinOutLevelConfig;

namespace Unpuzzle
{
    public abstract class AbstractGameController : MonoBehaviour
    {
        [SerializeField] protected UIGameScreen _gameScreen;
        [SerializeField] protected CameraController _cameraController;
        [SerializeField] protected AdsBreakScreen _adBreakScreen;
        [SerializeField] protected PinProgressScreen _adProgressScreen;
        [SerializeField] protected PinProgressBar _pinProgressBar;
        private readonly int pinsToCollect = 100;

        private int _currentStage;
        private bool _firstMoveDone;
        private bool _isComplete;
        private bool _isFailed;

        private bool _isPinProgressEnabled;
        private bool _isTargetEnabled;

        private LevelModeType _levelMode;

        protected LevelProgress _levelProgress;

        private LevelsController _levelsController;

        protected int _loseCount;

        private MovesController _movesController;
        private int _movesCount;

        protected LevelOptions _options;

        private List<LevelStage> _stages;

        private int _startPinCount;
        private TargetController _targetController;
        private TimeController _timeController;
        private int _timeCount;

        private int pinRemoved;

        public int CurrentMovesCount => _movesController.MovesLeft;
        public bool StartAnimationShowing { get; private set; }

        private void Update()
        {
            if (_isFailed || _isComplete)
            {
                return;
            }

            var deltaTime = Time.deltaTime;
            _levelProgress.durationSec += deltaTime;

            if (_levelMode == LevelModeType.Time &&
                (!GameConfig.RemoteConfig.gameTimerStartAfterMove || _firstMoveDone))
            {
                _timeController.DecreaseTime(deltaTime);
                if (!_timeController.HasTime())
                {
                    CheckTime();
                }
            }
        }

        public event Action<ELevelCompleteReason, LevelProgress> OnComplete = (reason, progress) => { };

        public event Action<LevelProgress> OnFailed = progress => { };

        public event Action<HookController> OnHookRemoved = hook => { };

        public event Action<Transform> OnKeyFound = key => { };

        public event Action OnMoveDone = () => { };

        public bool CanMove()
        {
            return !StartAnimationShowing && (
                (_levelMode == LevelModeType.Moves && _movesController.MovesLeft > 0) ||
                (_levelMode == LevelModeType.Time && _timeController.TimeLeft > 0));
        }

        public LevelStage GetCurrentStage()
        {
            if (_stages == null || _stages.Count == 0)
            {
                return null;
            }

            if (_currentStage >= _stages.Count)
            {
                return null;
            }

            return _stages[_currentStage];
        }

        public bool IsHintBoosterNotSupport()
        {
            return _stages.Any(a => a == null || a.Level == null || a.Level.HasFrozenPins || a.Level.HasGhostPins);
        }

        public bool IsRocketBoosterNotSupport()
        {
            return _stages.Any(a => a == null || a.Level == null || a.Level.HasGhostPins);
        }

        public void Init(List<PinOutLevel> stages, LevelsController levelsController, LevelOptions levelOptions,
            PinOutLevelConfig config)
        {
            _loseCount = 0;
            pinRemoved = 0;
            _levelProgress = new LevelProgress();
            _isComplete = false;
            _firstMoveDone = false;
            _levelsController = levelsController;

            _movesController = new MovesController();
            _timeController = new TimeController();
            _targetController = new TargetController();
            _movesController.OnMovesLeftChanged += (moves, increse) => OnMoveCountUpdate();

            _levelMode = levelOptions.mode;
            _movesCount = levelOptions.movesCount;
            _timeCount = levelOptions.timeCount;
            Debug.Log("target enabled: " + levelOptions.targetEnabled);
            _isTargetEnabled = levelOptions.targetEnabled;

            _stages = new List<LevelStage>();

            var levelOffset = 0f;

            for (var i = 0; i < stages.Count; i++)
            {
                var level = stages[i];
                var zoom = _cameraController.GetZoom(new Vector2(
                    level.GetMaxScreenBoundOffset(_cameraController.MainCamera),
                    level.GetMaxScreenBoundOffsetHeight(_cameraController.MainCamera)));
                var offset = level.CenterPivot != null
                    ? level.transform.InverseTransformPoint(level.CenterPivot.position)
                    : level.CalculateVisualCenter();

                var height = level.GetHeightOfLevel();

                if (i > 0)
                {
                    levelOffset += height;
                }

                level.transform.localPosition = Vector3.up * levelOffset;

                levelOffset += height + 50;

                var stage = new LevelStage(level);
                stage.offset = offset;
                stage.stageScale = zoom;
                level.OnHookRemoved += OnHookLevelRemoved;
                level.OnMoveDone += MoveDone;
                level.OnHookRemoved += RemovePin;
                _stages.Add(stage);
            }

            Physics.simulationMode = SimulationMode.Script;
            Physics.SyncTransforms();
            Physics.Simulate(0.1f);
            Physics.simulationMode = SimulationMode.FixedUpdate;

            foreach (var level in stages)
            {
                level.Init(0, _timeCount, _isTargetEnabled, levelOptions.moveOverrided, _levelMode, this);
                level.gameObject.SetActive(false);
            }


            _currentStage = 0;

            _stages[_currentStage].Level.gameObject.SetActive(true);

            _isFailed = false;
            GameStateSaveUtils.IsLastStateWasFailScreen = false;

            _cameraController.ResetRotation();

            if (_stages.Count == 1)
            {
                Physics.SyncTransforms();
                _stages[_currentStage].Level.StartLevel();
                _cameraController.SetZoom(_stages[_currentStage].stageScale);
                _cameraController.SetCameraOffset(_stages[_currentStage].offset);
                _stages[_currentStage].Level.ShowAnimation();
            }
            else
            {
                var time = 1f;
                _stages.ForEach(a => a.Level.gameObject.SetActive(true));
                StartAnimationShowing = true;
                var sequence = DOTween.Sequence()
                    .AppendCallback(() =>
                    {
                        var lastStage = _stages[_stages.Count - 1];
                        _cameraController.SetZoom(lastStage.stageScale);
                        _cameraController.SetCameraOffset(_stages[_currentStage].offset);
                        _cameraController.Handle.position = lastStage.Level.transform.position;
                        lastStage.Level.ShowAnimation();
                    });
                sequence.AppendInterval(time);

                for (var i = _stages.Count - 2; i >= 0; i--)
                {
                    var stage = _stages[i];
                    sequence.AppendCallback(() =>
                    {
                        _cameraController.LerpZoom(stage.stageScale, time, Ease.Linear);
                        stage.Level.ShowAnimation();
                    });
                    sequence.Append(_cameraController.Handle.DOMove(stage.Level.transform.position, time)
                        .SetEase(Ease.Linear));
                }

                sequence.AppendCallback(() =>
                {
                    StartAnimationShowing = false;

                    _stages.ForEach(a => a.Level.gameObject.SetActive(false));
                    _stages[_currentStage].Level.gameObject.SetActive(true);

                    Physics.SyncTransforms();
                    _stages[_currentStage].Level.StartLevel();
                });
            }

            var moves = 0;
            _stages.ForEach(a => moves += a.Level.MoveCount);
            if (_movesCount < 1)
            {
                _movesCount = moves + Mathf.CeilToInt(moves * 0.2f);
            }

            _gameScreen.SetMovesWidgetEnabled(
                GameConfig.RemoteConfig.maxMovesConfig.enabled && config.LevelMode == LevelModeType.Moves,
                levelOptions.targetEnabled);

            _timeController.SetTime(levelOptions.timeCount, levelOptions.timeCount);
            _movesController.SetMoves(_movesCount);

            _gameScreen.StageStatusBar.Init(_stages.Count);

            _startPinCount = _stages[_currentStage].Level.MoveCount;
            _isPinProgressEnabled = CheckPinProgressEnabled();

            if (_isPinProgressEnabled)
            {
                _pinProgressBar.gameObject.SetActive(true);
                _pinProgressBar.SetProgress(0, pinsToCollect);
            }

            if (!_isTargetEnabled)
            {
                OnInit(levelOptions);
                return;
            }

            var targetCount = 0;

            foreach (var stage in _stages)
            {
                stage.Level.Hooks.ForEach(a => targetCount += a.GetTargetCount());
            }

            _targetController.OnTargetChange += target => { OnTargetUpdate(); };
            _targetController.SetMoves(targetCount);

            _gameScreen.UpdateTargetLeft(targetCount);

            _gameScreen.ShowTargetIntroduce(config);

            OnInit(levelOptions);
        }

        protected virtual void OnInit(LevelOptions levelOptions)
        {
            _options = levelOptions;
        }

        private void RemovePin(HookController hook, bool completed, bool removedByHook)
        {
            if (completed)
            {
                return;
            }

            var currentStage = GetCurrentStage();

            if (currentStage == null)
            {
                return;
            }

            if (!_isPinProgressEnabled || _adProgressScreen.Showed)
            {
                return;
            }

            pinRemoved++;

            _pinProgressBar.AddCollected(1);
        }

        public void PinProgressNextStage()
        {
            if (pinRemoved >= pinsToCollect)
            {
                pinRemoved = 0;
                _pinProgressBar.SetProgress(0, pinsToCollect);
            }
        }

        private void OnStageComplete(ELevelCompleteReason reason, LevelProgress levelProgress)
        {
            pinRemoved = 0;
            _gameScreen.StageStatusBar.CompleteStage();

            if (_currentStage >= _stages.Count - 1)
            {
                _isComplete = true;
                OnCompleteLevel(_levelProgress);

                return;
            }

            StartAnimationShowing = true;

            var lastStage = _stages[_currentStage];

            _currentStage++;

            _stages[_currentStage].Level.gameObject.SetActive(true);

            _stages[_currentStage].Level.transform.DOLocalMove(Vector3.zero, 0.5f)
                .OnComplete(() =>
                {
                    Physics.SyncTransforms();
                    _stages[_currentStage].Level.StartLevel();

                    StartAnimationShowing = false;
                });

            _cameraController.ResetRotationLerp(0.5f);
            _cameraController.LerpZoom(_stages[_currentStage].stageScale, 0.5f, Ease.OutSine);
            _cameraController.CameraOffsetLerp(_stages[_currentStage].offset, 0.5f);

            _startPinCount = _stages[_currentStage].Level.MoveCount;

            _adBreakScreen.ShowAd();

            _startPinCount = _stages[_currentStage].Level.MoveCount;

            _isPinProgressEnabled = CheckPinProgressEnabled();

            if (_isPinProgressEnabled)
            {
                _pinProgressBar.gameObject.SetActive(true);
                _pinProgressBar.SetProgress(0, pinsToCollect);
            }
            else
            {
                _pinProgressBar.gameObject.SetActive(false);
            }
        }

        public void MoveDone()
        {
            _levelProgress.moveCount++;
            _movesController.DecreaseMoves();
            OnMoveDone();
            LevelsController.Instance.MoveDone();
        }

        public void CheckMoves()
        {
            if (_levelMode == LevelModeType.Moves && _movesController.MovesLeft <= 0 && !_isFailed && !_isComplete)
            {
                _isFailed = true;
                GameStateSaveUtils.IsLastStateWasFailScreen = true;
                OnFailLevel(_levelProgress);
            }
        }

        public void CheckTime()
        {
            if (_levelMode == LevelModeType.Time && _timeController.TimeLeft <= 0 && !_isFailed && !_isComplete)
            {
                _isFailed = true;
                GameStateSaveUtils.IsLastStateWasFailScreen = true;
                OnFailLevel(_levelProgress);
            }
        }

        public void AddTime(int timeCount)
        {
            _timeCount = timeCount;
            _timeController.SetTime(timeCount, timeCount);
            _isFailed = false;
            GameStateSaveUtils.IsLastStateWasFailScreen = false;
            LevelsController.Instance.CurrentGameController.GetCurrentStage().Level.OnPreMove();
        }

        public void AddMoves(int movesCount)
        {
            _movesController.AddMoves(movesCount);
            _isFailed = false;
            GameStateSaveUtils.IsLastStateWasFailScreen = false;
            LevelsController.Instance.CurrentGameController.GetCurrentStage().Level.OnPreMove();
        }

        private void OnMoveCountUpdate()
        {
            _gameScreen.UpdateMovesLeft(_movesController.MovesLeft);
        }

        private void OnTargetUpdate()
        {
        }

        private bool CheckPinProgressEnabled()
        {
            if (_currentStage >= _stages.Count || LevelData.GetLevelNumber() < GameConfig.RemoteConfig.showAdsAtLevel)
            {
                return false;
            }

            return true;
        }

        private bool IsEnoughPinsForProgressAds()
        {
            if (_currentStage >= _stages.Count)
            {
                return false;
            }

            return true;
        }

        private void OnHookLevelRemoved(HookController hookRemoved, bool isComplete, bool removedByBooster)
        {
            _firstMoveDone = true;
            OnHookRemoved(hookRemoved);

            if (_isTargetEnabled)
            {
                _targetController.SetMoves(_targetController.TargetLeft - hookRemoved.GetTargetCount());

                if (hookRemoved.IsTarget)
                {
                    var switchers = hookRemoved.TargetSwitchers;
                    var camera = CameraManager.Instance.GetCameraItem(ECameraType.GAME).Camera;
                    var pos = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0));

                    pos += camera.transform.forward * 300f;
                    var delay = 0f;
                    foreach (var switcher in switchers)
                    {
                        var star = switcher.TargetPin;

                        UIGameScreen.Instance.FlyStarToCounter(star.transform, delay);
                        delay += 0.2f;
                    }
                }

                if (_targetController.TargetLeft <= 0)
                {
                    var currentStageHooks = _stages[_currentStage].Level.Hooks;
                    for (var i = 0; i < currentStageHooks.Count; i++)
                    {
                        currentStageHooks[i].ForceMoveOut((1 + i) * 0.2f);
                    }

                    if (_isComplete)
                    {
                        return;
                    }

                    _isComplete = true;
                    OnCompleteLevel(_levelProgress);
                    return;
                }
            }

            if (isComplete)
            {
                OnStageComplete(ELevelCompleteReason.WIN, new LevelProgress());
            }

            if (!_isComplete)
            {
                CheckMoves();
            }
        }

        protected virtual void OnCompleteLevel(LevelProgress levelProgress)
        {
            OnComplete(ELevelCompleteReason.WIN, levelProgress);
        }

        protected virtual void OnFailLevel(LevelProgress level)
        {
            _loseCount++;
            OnFailed(level);
        }

        public void DestroyLevels()
        {
            foreach (var stage in _stages)
            {
                stage.Level.gameObject.SetActive(false);
                Destroy(stage.Level.gameObject);
            }
        }
    }
}

public class LevelOptions
{
    public LevelDifficultyType difficultyType;
    public LevelModeType mode;
    public bool moveOverrided;
    public int movesCount;
    public bool targetEnabled;
    public int timeCount;
}
