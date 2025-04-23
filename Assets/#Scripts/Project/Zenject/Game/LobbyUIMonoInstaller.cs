using UnityEngine;
using Unpuzzle;
using Zenject;

public class LobbyUIMonoInstaller : MonoInstaller
{
    [Header("Controlls")] [SerializeField] protected MobileInputController _mobileInputController;

    [Header("Screens")] [SerializeField] protected UISettingsScreen _settingScreen;

    [SerializeField] protected UIShopScreen _shopScreen;
    [SerializeField] protected LobbyScreensSwitcher _lobbyScreensSwitcher;

    public override void InstallBindings()
    {
        BindControls();
        BindScreens();
    }

    protected virtual void BindControls()
    {
    }

    protected virtual void BindScreens()
    {
        Container.BindInterfacesAndSelfTo<UISettingsScreen>().FromInstance(_settingScreen).AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<UIShopScreen>().FromInstance(_shopScreen).AsSingle()
            .NonLazy();
        Container.BindInterfacesAndSelfTo<LobbyScreensSwitcher>().FromInstance(_lobbyScreensSwitcher).AsSingle()
            .NonLazy();
    }
}
