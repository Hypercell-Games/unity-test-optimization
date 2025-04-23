using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        SetConfigs();

        BindControllers();
        BindTools();
    }

    protected virtual void SetConfigs()
    {
        Application.targetFrameRate = 60;
        Input.multiTouchEnabled = true;
    }

    protected virtual void BindControllers()
    {
    }

    protected virtual void BindTools()
    {
    }
}
