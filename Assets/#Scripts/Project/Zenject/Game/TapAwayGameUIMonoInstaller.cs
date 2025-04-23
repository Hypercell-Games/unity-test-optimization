using UnityEngine;
using Zenject;

public class TapAwayGameUIMonoInstaller : MonoInstaller
{
    [Header("Controlls")] [SerializeField] protected MobileInputController _mobileInputController;
    [Header("Screens")] [SerializeField] protected TapAway.UILevelCompleteScreen _levelCompleteScreen;

    [SerializeField] protected UIGameScreen _gameScreen;
    [SerializeField] protected UISettingsScreen _settingScreen;
    [SerializeField] protected UIShopScreen _shopScreen;

    public override void InstallBindings()
    {
        BindControls();
        BindScreens();
    }

    protected virtual void BindControls()
    {
        Container.BindInterfacesAndSelfTo<MobileInputController>().FromInstance(_mobileInputController).AsSingle()
            .NonLazy();
    }

    protected virtual void BindScreens()
    {
        Container.BindInterfacesAndSelfTo<TapAway.UILevelCompleteScreen>().FromInstance(_levelCompleteScreen).AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<UIGameScreen>().FromInstance(_gameScreen).AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<UISettingsScreen>().FromInstance(_settingScreen).AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<UIShopScreen>().FromInstance(_shopScreen).AsSingle()
            .NonLazy();
    }
}
