using UnityEngine;
using Unpuzzle;
using Zenject;

public class GameUIMonoInstaller : MonoInstaller
{
    [Header("Controlls")] [SerializeField] protected MobileInputController _mobileInputController;
    [Header("Screens")] [SerializeField] protected UILevelCompleteScreen _levelCompleteScreen;

    [SerializeField] protected UILevelFailedScreen _levelFailedScreen;
    [SerializeField] protected UIGameScreen _gameScreen;
    [SerializeField] protected UISettingsScreen _settingScreen;
    [SerializeField] protected UIShopScreen _shopScreen;
    [SerializeField] protected YouWillLoseHeartScreen _youWillLoseHeartScreen;

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
        Container.BindInterfacesAndSelfTo<UILevelCompleteScreen>().FromInstance(_levelCompleteScreen).AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<UILevelFailedScreen>().FromInstance(_levelFailedScreen).AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<UIGameScreen>().FromInstance(_gameScreen).AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<UISettingsScreen>().FromInstance(_settingScreen).AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<UIShopScreen>().FromInstance(_shopScreen).AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<YouWillLoseHeartScreen>().FromInstance(_youWillLoseHeartScreen).AsSingle()
            .NonLazy();
    }
}
