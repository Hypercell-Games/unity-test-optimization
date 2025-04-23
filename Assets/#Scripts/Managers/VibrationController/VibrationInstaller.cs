using Zenject;

public class VibrationInstaller : MonoInstaller<VibrationInstaller>
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<VibrationController>().FromNew().AsSingle().NonLazy();
    }
}
