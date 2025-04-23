using UnityEngine;

public class GlobalData : MonoBehaviour
{
    private static GlobalData _instance;

    [SerializeField] private GameData _gameData;

    public static GlobalData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GlobalData>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (_instance && _instance == this)
        {
            _instance = null;
        }
    }

    public GameData GetGameData()
    {
        return _gameData;
    }
}
