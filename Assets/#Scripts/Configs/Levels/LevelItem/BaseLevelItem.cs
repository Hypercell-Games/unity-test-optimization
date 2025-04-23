using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class BaseLevelItem : MonoBehaviour, ILevelItem
{
    [SerializeField] private GameData _gameData;
    [SerializeField] private GameBoardController _gameBoardPrefab;
    [SerializeField] private Transform _gameBoardsHolder;

    private readonly List<GameBoardController> _gameBoardControllers = new();
    private int _countMoves;
    private int _currentStageNum;

    private bool _isLevelCompleted;
    private int _levelNum;
    private float _timeStart;

    private void Awake()
    {
        InitializeLevelData();
    }

    public event Action<ELevelCompleteReason, LevelProgress> onLevelCompleted;
    public event Action<Vector2> onLevelInitialized;
    public event Action<TileElementController> onKeyFound;
    public event Action<int, bool> onMovesLeftChanges;
    public event Action onOutOfMoves;

    public void StartLevel(int levelNum, LevelStage[] levelStages, ELevelStartType levelStartType)
    {
        _timeStart = Time.unscaledTime;
        _countMoves = 0;

        _levelNum = levelNum;

        _gameBoardsHolder.ClearChildren();
        _gameBoardControllers.Clear();

        var stages = LevelJsonConverter.JSONDeserializer(levelStages);
        var targetKeyStage = Random.Range(0, stages.Length);

        for (var i = 0; i < stages.Length; i++)
        {
            var stage = stages[i];
            var gameBoardControllerStage = Instantiate(_gameBoardPrefab, _gameBoardsHolder);
            gameBoardControllerStage.OnMovesLeftChanged += (r, t) => onMovesLeftChanges?.Invoke(r, t);
            gameBoardControllerStage.InitializeBoard(_levelNum, i, stages.Length - i - 1, stage, OnStageCompleted,
                OnGetStageSolution, targetKeyStage, onKeyFound);
            _gameBoardControllers.Add(gameBoardControllerStage);
            gameBoardControllerStage.SetStage(i, true);
        }

        StartStage(_gameBoardControllers[0]);
    }

    public void SetSkin(int index, bool debug, int levelNum)
    {
        for (var i = 0; i < _gameBoardControllers.Count; i++)
        {
            var gameBoardController = _gameBoardControllers[i];
            gameBoardController.SetSkin(index, debug, levelNum, i);
        }
    }

    private void InitializeLevelData()
    {
        _isLevelCompleted = false;
        _currentStageNum = 0;
    }

    private void StartStage(GameBoardController gameBoardController)
    {
        gameBoardController.StartStage();


        onMovesLeftChanges?.Invoke(gameBoardController.MovesLeft, true);
    }

    private void OnStageCompleted(ELevelCompleteReason levelCompleteReason)
    {
        if (levelCompleteReason == ELevelCompleteReason.WIN)
        {
        }
        else if (levelCompleteReason == ELevelCompleteReason.LOSE)
        {
        }

        if (levelCompleteReason == ELevelCompleteReason.LOSE_OUT_OF_MOVES)
        {
            onOutOfMoves?.Invoke();
            return;
        }

        if (_currentStageNum + 1 < _gameBoardControllers.Count && levelCompleteReason == ELevelCompleteReason.WIN)
        {
            void StageTransitions()
            {
                VibrationController.Instance.Play(EVibrationType.Success);

                _currentStageNum++;
                var gameBoardController = _gameBoardControllers[_currentStageNum];
                StartStage(gameBoardController);
                for (var i = _currentStageNum; i < _gameBoardControllers.Count; i++)
                {
                    _gameBoardControllers[i].SetStage(i - _currentStageNum, false);
                }

                for (var i = 0; i < _currentStageNum; i++)
                {
                    _gameBoardControllers[i].gameObject.SetActive(false);
                }
            }

            if (GameConfig.RemoteConfig.showInterstitialBetweenStages <= 0)
            {
                StageTransitions();
            }
            else if (GameConfig.RemoteConfig.showInterstitialBetweenStages == 1)
            {
                StageTransitions();
                HyperKit.Ads.ShowInterstitial("ad_inter_between_stages");
            }
            else
            {
                HyperKit.Ads.ShowInterstitial("ad_inter_between_stages", _ => StageTransitions());
            }
        }
        else
        {
            if (!_isLevelCompleted)
            {
                void Complete()
                {
                    if (levelCompleteReason == ELevelCompleteReason.WIN)
                    {
                    }
                    else if (levelCompleteReason == ELevelCompleteReason.LOSE)
                    {
                        VibrationController.Instance.Play(EVibrationType.Warning);
                    }

                    var levelProgress = new LevelProgress
                    {
                        durationSec = Time.unscaledTime - _timeStart,
                        moveCount = _gameBoardControllers.Sum(b => b.GetMoveCount())
                    };
                    onLevelCompleted?.Invoke(levelCompleteReason, levelProgress);
                }

                if (levelCompleteReason == ELevelCompleteReason.WIN)
                {
                    DOVirtual.DelayedCall(GameConfig.RemoteConfig.completeScreenDelay, Complete).SetLink(gameObject);
                }
                else
                {
                    Complete();
                }

                _isLevelCompleted = true;
            }
        }
    }

    private void OnGetStageSolution(Vector2 solutionPosition)
    {
        onLevelInitialized?.Invoke(solutionPosition);
    }

    public void AddMoves(int movesCount)
    {
        _gameBoardControllers[_currentStageNum].AddMoves(movesCount);
    }
}
