using UnityEngine;
using Zenject;

public class GameMonoInstaller : MonoInstaller
{
    [Header("GAME")] [SerializeField] protected LevelsController _levelsController;

    public override void InstallBindings()
    {
        BindLevels();
    }

    protected virtual void BindLevels()
    {
        Container.BindInterfacesAndSelfTo<LevelsController>().FromInstance(_levelsController).AsSingle().NonLazy();
    }
}
