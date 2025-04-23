public abstract class MonoSingleton<T> : BaseMonoSingleton<T> where T : MonoSingleton<T>
{
    #region Fields

    private static T instance;

    #endregion Fields

    #region Properties

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindInstance();
            }

            return instance;
        }
    }

    protected bool IsInstance => Instance == this;

    #endregion Properties

    #region Methods

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            OnInstanced();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    protected virtual void OnInstanced()
    {
    }

    #endregion Methods
}
