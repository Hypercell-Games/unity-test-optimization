using Zenject;

public class SettingsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<SettingsController>().FromNew().AsSingle().NonLazy();
    }
}
