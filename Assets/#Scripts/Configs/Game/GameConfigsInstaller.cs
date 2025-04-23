using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameConfigsInstaller", menuName = "Installers/GameConfigsInstaller")]
public class GameConfigsInstaller : ScriptableObjectInstaller<GameConfigsInstaller>
{
    [SerializeField] private LevelsConfigs _levelsConfigs;

    public override void InstallBindings()
    {
        Container.Bind<LevelsConfigs>().FromInstance(_levelsConfigs).AsSingle().NonLazy();
    }
}
