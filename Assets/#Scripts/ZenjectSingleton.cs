using Zenject;

public abstract class ZenjectSingleton<T> : IInitializable, ILateDisposable where T : ZenjectSingleton<T>, new()
{
    #region Properties

    public static T Instance { get; private set; }

    protected bool IsInstance => Instance == this;
    public static bool HasInstance => Instance != null;

    #endregion Properties

    #region Methods

    protected ZenjectSingleton()
    {
        if (Instance == null)
        {
            Instance = this as T;
        }
    }

    public virtual void Initialize()
    {
        if (Instance == null)
        {
            Instance = this as T;
        }
    }

    public void LateDispose()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion Methods
}
