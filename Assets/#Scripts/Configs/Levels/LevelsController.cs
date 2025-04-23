using System;
using System.Collections.Generic;
using SortCore;
using UnityEngine;
using Unpuzzle;
using Unpuzzle.Game;
using Unpuzzle.Game.Data;
using Zenject;

public class LevelsController : MonoBehaviour
{
    private static LevelsController _instance;

    [SerializeField] private GameData _gameData;
    [SerializeField] private RateAppLayout _rateAppLayout;
    [SerializeField] private List<PinOutLevel> _debugLevel;
    [SerializeField] private bool _isTargetDebug;
    [SerializeField] private ParticleSystem _bgParticelSystem;
    [SerializeField] private bool _isOverrideMoves;
    [SerializeField] private int _overridedMoves;
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private LevelBuilder _levelBuilder;
    [SerializeField] private NormalGameController _normalGameController;

    [Inject] protected readonly LevelsConfigs LevelConfigs;

    private BaseLevelItem _currentLevelItem;
    [Inject] private UIGameScreen _gameScreen;

    [Inject] private UILevelFailedScreen _levelFailedScreen;

    public static LevelsController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelsController>();
            }

            return _instance;
        }
        private set => _instance = value;
    }

    public AbstractGameController CurrentGameController { get; private set; }

    public int CurrentLevelNumber { get; private set; }

    public AnimationCurve Curve => _curve;

    public LevelType LevelType { get; private set; } = LevelType.Normal;

    private void Awake()
    {
        Instance = this;
        SetData();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartNextLevel(ELevelCompleteReason.SKIP);
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public event Action<BaseLevelItem> onLevelDestroy;
    public event Action onNewLevelStart;
    public event Action onLevelCreate;
    public event Action<Transform> onKeyFound;
    public event Action onLevelCompleted;
    public event Action onMoveDone;

    [Inject]
    protected virtual void Initialize(UILevelFailedScreen levelFailedScreen)
    {
        levelFailedScreen.onRestart += StartNextLevel;
    }

    private void AddMoves(int movesCount)
    {
        CurrentGameController.AddMoves(movesCount);
    }

    private void AddTime(int movesCount)
    {
        CurrentGameController.AddTime(movesCount);
    }

    [Inject]
    protected virtual void Initialize(UILevelCompleteScreen levelCompleteScreen)
    {
    }

    [Inject]
    protected virtual void Initialize(UIGameScreen gameScreen)
    {
        gameScreen.onRestartLevel += RestartLevel;
    }

    public void StartNextLevel(ELevelCompleteReason levelCompleteReason)
    {
        GameStateSaveUtils.ClearSavedData();

        switch (levelCompleteReason)
        {
            case ELevelCompleteReason.WIN:
                if (!GameLogicUtil.TryGoToLobby(GlobalData.Instance.GetGameData(), true))
                {
                    StartNextLevel(true);
                }

                break;

            case ELevelCompleteReason.SKIP:
                IncreaseLevelId();
                StartNextLevel(true);
                break;

            case ELevelCompleteReason.LOSE:
                RestartLevel(ELevelStartType.RESTART);
                break;
        }
    }

    public void CompleteLevel(ELevelCompleteReason levelCompleteReason, LevelProgress levelProgress)
    {
        GameStateSaveUtils.ClearSavedData();
        onLevelCompleted?.Invoke();

        switch (levelCompleteReason)
        {
            case ELevelCompleteReason.WIN:
                IncreaseLevelId();
                VibrationController.Instance.Play(EVibrationType.Success);

                ScreenSwitcher.Instance.NextFinishScreen();
                break;

            case ELevelCompleteReason.LOSE:
                Debug.Log("fail screen show");
                _levelFailedScreen.Show();
                break;
        }
    }

    public void UpdateSkin()
    {
        var skin = LevelType == LevelType.Normal ? _gameData.GetSelectedSkin() : _gameData.GetChallengeSkin();

        ColorThemeLogicUtil.SetColorTheme(LevelType == LevelType.Normal ? ColorThemeType.Light : ColorThemeType.Dark);

        if (_bgParticelSystem != null)
        {
            Destroy(_bgParticelSystem.gameObject);
        }

        if (skin.ParticleSystem != null)
        {
            _bgParticelSystem = Instantiate(skin.ParticleSystem, Vector3.zero, Quaternion.identity);
        }

        RenderSettings.skybox = skin.SkyBox;
    }

    public void CreateLevel(bool cleanStart, ELevelStartType levelStartType = ELevelStartType.DEFAULT)
    {
        LevelType = LevelType.Normal;

        UpdateSkin();

        void Create()
        {
            var currentLevelInfo = LevelsConfigs.GetRemoteLevelNameSimple(CurrentLevelNumber);

            ColorHolder.Instance.SetMaterial(currentLevelInfo.pinMaterialId);

            _debugLevel = Application.isEditor ? _debugLevel : null;
            _isTargetDebug = Application.isEditor && _isTargetDebug;

            GameStateSaveData saveData = null;
            var levelsData = GetLevelsData(currentLevelInfo.name);
            var movesCount = currentLevelInfo.moves;
            if (levelsData.Count == 1)
            {
                GameStateSaveUtils.TryGetState(CurrentLevelNumber, currentLevelInfo.name, out saveData);

                if (saveData != null)
                {
                    movesCount = saveData.moves;
                }
            }

            StartGameController(_normalGameController, levelsData,
                new LevelOptions
                {
                    mode = currentLevelInfo.LevelMode,
                    targetEnabled = _isTargetDebug ? true : currentLevelInfo.isTarget,
                    moveOverrided = _isOverrideMoves,
                    movesCount = _isOverrideMoves ? _overridedMoves : movesCount,
                    difficultyType = currentLevelInfo.LevelDifficultyType
                }, currentLevelInfo, saveData);
        }

        if (cleanStart)
        {
            if (GameConfig.RemoteConfig.showInterAtLevelStart)
            {
                HyperKit.Ads.ShowInterstitial("ad_inter_level_start", _ => Create());
            }
            else
            {
                Create();
            }

            return;
        }

        Create();
    }

    private List<PinOutLevelData> GetLevelsData(string levelNames)
    {
        var data = new List<PinOutLevelData>();
        if (Application.isEditor && _debugLevel != null && _debugLevel.Count > 0)
        {
            foreach (var level in _debugLevel)
            {
                data.Add(PinOutLevelConverter.GetLevelData(level));
            }

            return data;
        }

        var levels = levelNames.Split(';');
        var levelsPath = Application.dataPath + "/#Prefabs/BuildLevels/";

        foreach (var levelName in levels)
        {
            var loadedLevel = Resources.Load<TextAsset>("Levels/" + levelName);

            data.Add(JsonUtility.FromJson<PinOutLevelData>(loadedLevel.text));
        }

        return data;
    }

    private void StartGameController(AbstractGameController gameController, List<PinOutLevelData> levels,
        LevelOptions levelOptions, PinOutLevelConfig config, GameStateSaveData saveData = null)
    {
        onNewLevelStart?.Invoke();
        CurrentGameController = gameController;
        CurrentGameController.Init(BuildStages(levels, saveData), this, levelOptions, config);
        onLevelCreate?.Invoke();
    }

    private List<PinOutLevel> BuildStages(List<PinOutLevelData> levelsJson, GameStateSaveData saveData = null)
    {
        var levelList = new List<PinOutLevel>();

        foreach (var levelJson in levelsJson)
        {
            var level = _levelBuilder.GetLevel(levelJson, saveData);
            level.gameObject.name = levelJson.name;
            level.gameObject.SetActive(false);
            levelList.Add(level);
        }

        return levelList;
    }

    public void MoveDone()
    {
        onMoveDone?.Invoke();
    }

    public void ForceLoadLevel(int levelNumber)
    {
        CurrentLevelNumber = Mathf.Max(0, levelNumber);
        LevelData.SetLevelNumber(CurrentLevelNumber);
        RestartLevel(ELevelStartType.RESTART);
    }

    public void ForceSkin(int index, bool debug)
    {
        _currentLevelItem.SetSkin(index, debug, CurrentLevelNumber);
    }

    public void RestartLevel(ELevelStartType levelStartType)
    {
        if (LevelType == LevelType.Normal)
        {
            GameStateSaveUtils.ClearSavedData();
        }

        HyperKit.Ads.ShowInterstitial("ad_inter_level_restart", _ =>
        {
            DestroyLevel();
            CreateLevel(false, levelStartType);

            VibrationController.Instance.Play(EVibrationType.MediumImpact);
        });
    }

    private void DestroyLevel()
    {
        onLevelDestroy?.Invoke(_currentLevelItem);

        if (CurrentGameController)
        {
            CurrentGameController.DestroyLevels();
        }
    }

    private void IncreaseLevelId()
    {
        LevelData.SetLevelNumber(CurrentLevelNumber + 1);
    }

    private void StartNextLevel(bool cleanStart)
    {
        DestroyLevel();

        CurrentLevelNumber++;

        CreateLevel(cleanStart);
    }

    public void StartCurrentLevel(bool cleanStart)
    {
        DestroyLevel();
        CreateLevel(cleanStart);
    }

    private void SetData()
    {
        CurrentLevelNumber = LevelData.GetLevelNumber();
    }
}

public enum ELevelStartType
{
    DEFAULT = 0,

    RESTART = 1,

    BUTTON_RESTART = 2
}

public enum LevelType
{
    Normal,
    Boss,
    Challenge
}
