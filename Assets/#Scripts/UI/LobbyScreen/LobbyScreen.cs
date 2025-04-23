using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Unpuzzle;
using Unpuzzle.Game.Data;
using Unpuzzle.UI.NewTabBar;
using Zenject;

public class LobbyScreen : MonoBehaviour
{
    [SerializeField] private TextButtonController _playButton;
    [SerializeField] private NewTabBarScreen _tabBarScreen;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private LobbyMap _map;
    [SerializeField] private GameObject _overlap;
    [SerializeField] private Animation _shineAnimation;

    [Header("Play")] [SerializeField] private Image _outlineHardImage;

    [SerializeField] private Image _outlineEpicImage;
    [SerializeField] private Image _skullHardImage;
    [SerializeField] private Image _skullEpicImage;
    [SerializeField] private GameObject _playNormalText;
    [SerializeField] private GameObject _playHardEpicText;
    [SerializeField] private GameObject _hardLevelText;
    [SerializeField] private GameObject _epicLevelText;
    [Inject] private LobbyScreensSwitcher _lobbyScreensSwitcher;

    [Inject] private UISettingsScreen _settingScreen;
    private Sequence _shineSeq;

    private void Awake()
    {
        _playButton.SetOnButtonClick(LoadNormalMode);

        _settingsButton.onClick.AddListener(() =>
        {
            _settingScreen.Show();
        });

        _tabBarScreen.Init(_lobbyScreensSwitcher.SwitchScreen);

        _shineSeq?.Kill();
        _shineAnimation.Stop();

        ShinePlayButton();

        _map.Init(this, _tabBarScreen);

        var levelConfig = LevelsConfigs.GetRemoteLevelNameSimple(LevelData.GetLevelNumber());
        _outlineHardImage.gameObject.SetActive(levelConfig.LevelDifficultyType == LevelDifficultyType.Hard);
        _skullHardImage.gameObject.SetActive(levelConfig.LevelDifficultyType == LevelDifficultyType.Hard);
        _outlineEpicImage.gameObject.SetActive(levelConfig.LevelDifficultyType == LevelDifficultyType.Epic);
        _skullEpicImage.gameObject.SetActive(levelConfig.LevelDifficultyType == LevelDifficultyType.Epic);
        _playNormalText.SetActive(levelConfig.LevelDifficultyType == LevelDifficultyType.Normal);
        _playHardEpicText.SetActive(levelConfig.LevelDifficultyType != LevelDifficultyType.Normal);
        _hardLevelText.SetActive(levelConfig.LevelDifficultyType == LevelDifficultyType.Hard);
        _epicLevelText.SetActive(levelConfig.LevelDifficultyType == LevelDifficultyType.Epic);
    }

    public void LockControl()
    {
        _overlap.SetActive(true);


        _shineAnimation.Stop();
        _shineSeq?.Kill();
    }

    public void UnlockControl()
    {
        _overlap.SetActive(false);
    }

    private void LoadNormalMode()
    {
        GlobalData.Instance.GetGameData().GoToGameMode(new NormalMode(GlobalData.Instance.GetGameData()));
    }

    public void ShinePlayButton()
    {
        _shineAnimation.Play();
    }
}
