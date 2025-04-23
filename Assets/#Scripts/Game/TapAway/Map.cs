using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Map : MonoBehaviour
{
    [SerializeField] private GameData _gameData;

    [Space] [SerializeField] private TapAway.UILevelCompleteScreen _levelCompleteScreen;

    [SerializeField] [TextArea(5, 50)] private string _levelConfigToDebug;
    [SerializeField] [TextArea(5, 50)] private string _levelsConfigToDebug;

    private LevelTapAway _level;
    public static int CurrentLevel { get; private set; }

    private void Awake()
    {
        _levelCompleteScreen.onNextLevel += LoadNext;

        var levelsConfig = GetLevelsListConfig();
        var levelConfig = levelsConfig[CurrentLevel % levelsConfig.Count];
        var levelPrefab = Resources.Load<LevelTapAway>($"Levels/{levelConfig.levelName}");

        _level = Instantiate(levelPrefab, transform);
        _level.Init(this, levelConfig.rotationSeed, levelConfig.moves);
    }

    private List<TapAwayLevelConfig> GetLevelsListConfig()
    {
        var levelsDataHolder = JsonUtility.FromJson<DataHolder<TapAwayLevelConfig>>(_levelsConfigToDebug);
        return levelsDataHolder.data;
    }

    private void AddMoves(int moves)
    {
        _level.AddMoves(moves);
    }

    private void LoseLevel()
    {
        _gameData.GoToGameMode(new NormalMode(_gameData));
    }

    public void LevelCompleted()
    {
        _levelCompleteScreen.Show();
    }

    public void LevelFailed()
    {
        Debug.Log("Level failed");
    }

    public void LoadNext()
    {
        CurrentLevel++;

        _gameData.GoToGameMode(new NormalMode(_gameData));
    }

    public void LoadPrev()
    {
        if (CurrentLevel > 0)
        {
            CurrentLevel--;
        }
        else
        {
            var levelsConfig = GetLevelsListConfig();
            if (levelsConfig != null && levelsConfig.Count > 0)
            {
                CurrentLevel = levelsConfig.Count - 1;
            }
        }

        SceneManager.LoadScene(Scenes.GAME_TAP_AWAY);
    }
}
