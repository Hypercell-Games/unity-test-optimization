using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unpuzzle;
using Unpuzzle.Game;
using Unpuzzle.Game.Data;
using Zenject;

public class UIGameScreen : BaseScreen
{
    [Space] [SerializeField] private GameData _gameData;

    [SerializeField] private GameObject _defaultTopGradient;
    [SerializeField] private GameObject _challengeTopGradient;

    [Header("Screen")] [SerializeField] private Image _raycastImage;

    [SerializeField] private MobileInputController _mobileInputController;
    [SerializeField] private Canvas _canvas;

    [Header("Buttons")] [SerializeField] private BaseButtonController _settingsButton;

    [SerializeField] private BaseButtonController _lobbyButton;
    [SerializeField] private BaseButtonController _restartButton;

    [Header("Text")] [SerializeField] private TextMeshProUGUI _levelText;

    [SerializeField] private TextMeshProUGUI _levelNumberText;
    [SerializeField] private GameObject _levelInfo;

    [Header("Widgets")] [SerializeField] private MovesCountWidget _movesCountWidget;

    [SerializeField] private KeysLayoutController _keysWidget;
    [SerializeField] private StageStatusBar _stageStatusBar;
    [Inject] UISettingsScreen _settingScreen;
    [Inject] YouWillLoseHeartScreen _youWillLoseHeartScreen;

    public StageStatusBar StageStatusBar => _stageStatusBar;

    public static UIGameScreen Instance { private set; get; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        AddListeners();
        UpdateVisual();

        _youWillLoseHeartScreen.Init();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    public event Action<ELevelStartType> onRestartLevel;

    private void AddListeners()
    {
        _settingsButton.onButtonClicked += OpenSettings;
        _lobbyButton.onButtonClicked += OnLobbyButtonClicked;
        _restartButton.onButtonClicked += OnRestartButtonClicked;

        LevelsController.Instance.onNewLevelStart += NewLevelLoaded;
    }

    private void RemoveListeners()
    {
        _settingsButton.onButtonClicked -= OpenSettings;
        _lobbyButton.onButtonClicked -= OnLobbyButtonClicked;
        _restartButton.onButtonClicked -= OnRestartButtonClicked;

        if (LevelsController.Instance)
        {
            LevelsController.Instance.onNewLevelStart -= NewLevelLoaded;
        }
    }

    public YieldInstruction TweenCollectKeyFrom(Transform key)
    {
        return _keysWidget.TweenCollectKeyFrom(key);
    }

    public void UpdateVisual()
    {
        var enableGameplayUI = DebugPanel.EnableGameplayUI;
        var isLobbyEnabled = GameLogicUtil.IsLobbyEnabled();

        _settingsButton.gameObject.SetActive(enableGameplayUI && !isLobbyEnabled);
        _lobbyButton.gameObject.SetActive(enableGameplayUI && isLobbyEnabled);
        _restartButton.gameObject.SetActive(enableGameplayUI);
        _levelInfo.gameObject.SetActive(enableGameplayUI);
    }

    public void UpdateMovesLeft(int count, bool tweenValue = false)
    {
        _movesCountWidget.UpdateMoves(count, tweenValue);
    }

    public void UpdateTargetLeft(int count)
    {
        _movesCountWidget.UpdateTarget(count, true);
    }

    public void ShowTargetIntroduce(PinOutLevelConfig levelInfo)
    {
        _movesCountWidget.ShowIntroduce(levelInfo);
    }

    public void SetMovesWidgetEnabled(bool isEnabled, bool isTarget)
    {
        _movesCountWidget.SetActive(isEnabled, isTarget);
    }

    public void FlyStarToCounter(Transform star, float delay)
    {
        _movesCountWidget.FlyStar(star, delay);
    }

    private void OpenSettings()
    {
        _settingScreen.Show();
    }

    private void OnLobbyButtonClicked()
    {
        var gameData = GlobalData.Instance.GetGameData();
        if (GameLogicUtil.IsLobbyEnabled())
        {
            GoToLobby();

            void GoToLobby()
            {
                gameData.GoToGameMode(new LobbyMode(GlobalData.Instance.GetGameData()));
            }
        }
        else
        {
            LevelsController.Instance.StartCurrentLevel(true);
        }
    }

    private void OnRestartButtonClicked()
    {
        if (_gameData.IsNormalMode() || !_gameData.IsGameModeNotNull())
        {
            onRestartLevel?.Invoke(ELevelStartType.BUTTON_RESTART);
            return;
        }

        _gameData.CurrentGameMode?.OnLevelRestart();
    }

    public override void Hide(bool force = false, Action callback = null)
    {
        _raycastImage.enabled = false;

        base.Hide(force, callback);
    }

    public override void Show(bool force = false, Action callback = null)
    {
        _raycastImage.enabled = true;
        _levelText.gameObject.SetActive(true);
        _levelNumberText.text = (LevelData.GetLevelNumber() + 1).ToString();

        base.Show(force, callback);

        EnableGradient(GameConfig.RemoteConfig.uiTopGradientEnabled);
    }

    private void EnableGradient(bool isEnabled)
    {
        if (!isEnabled || LevelsController.Instance == null)
        {
            _defaultTopGradient.SetActive(false);
            _challengeTopGradient.SetActive(false);
            return;
        }

        var isNormalMode = LevelsController.Instance.LevelType == LevelType.Normal;
        _defaultTopGradient.SetActive(isNormalMode);
        _challengeTopGradient.SetActive(!isNormalMode);
    }

    private bool ShouldShowSkinsButton()
    {
        return false;
    }

    public void NewLevelLoaded()
    {
        UpdateVisual();
    }

    public void InitNormalLevel()
    {
        _levelText.gameObject.SetActive(true);
        _restartButton.gameObject.SetActive(true);
    }
}
