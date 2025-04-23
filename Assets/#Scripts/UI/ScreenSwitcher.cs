using SortCore;
using UnityEngine;
using Unpuzzle.Game.Data;

public class ScreenSwitcher : MonoBehaviour
{
    private static ScreenSwitcher _instance;

    [SerializeField] private UIGameScreen _gameScreen;
    [SerializeField] private UILevelCompleteScreen _levelCompleteScreen;
    [SerializeField] private RateAppLayout _rateAppLayout;

    public static ScreenSwitcher Instance
    {
        get => _instance ??= FindObjectOfType<ScreenSwitcher>();
        private set => _instance = value;
    }

    public static bool ShouldShowChallengesScreen { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void NextFinishScreen(ScreenType screenType = ScreenType.None)
    {
        _levelCompleteScreen.Show();
    }

    public void NextStartScreen(ScreenType screenType = ScreenType.None)
    {
        if (TryShowPrivacyPolicyScreen(screenType))
        {
            return;
        }

        if (TryShowRateApp(screenType))
        {
            return;
        }

        _gameScreen.Show();
    }

    private bool TryShowPrivacyPolicyScreen(ScreenType screenType)
    {
        return false;
    }

    private bool TryShowRateApp(ScreenType screenType)
    {
        var rateAppShown = PlayerPrefsController.GetBool(EPlayerPrefsKey.rate_app_shown);
        if (rateAppShown)
        {
            return false;
        }

        if (GameConfig.RemoteConfig.minRateUsLevel <= 0 ||
            LevelData.GetLevelNumber() + 1 < GameConfig.RemoteConfig.minRateUsLevel)
        {
            return false;
        }

        PlayerPrefsController.SetBool(EPlayerPrefsKey.rate_app_shown, true);

#if UNITY_ANDROID
        _rateAppLayout.Show();
#elif UNITY_IOS
        UnityEngine.iOS.Device.RequestStoreReview();
        ScreenSwitcher.Instance.NextStartScreen(ScreenType.RateApp);
#endif
        return true;
    }
}

public enum ScreenType
{
    None,
    PrivacyPolicyScreen,
    RateApp,
    SurveyScreen,
    LifeOfferPanel,
    ChallengesScreen
}
