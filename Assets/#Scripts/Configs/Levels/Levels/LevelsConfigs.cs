using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelsConfigs", menuName = MenuPath, order = MenuOrder)]
public class LevelsConfigs : ScriptableObject
{
    private const string MenuPath = "Configs/LevelsConfigs";
    private const string LoggerOrigin = nameof(LevelsConfigs);
    private const int MenuOrder = int.MinValue + 121;

    [SerializeField] private List<LevelItemConfigs> _levelItemConfigs = new();

    public LevelItemConfigs GetLevelInfoFallback(int number)
    {
        var levelName = GetRemoteLevelName(number);
        var levelItem = GetLevelItem(levelName);

        try
        {
            CheckLevelItem(levelItem, number, levelName);
        }
        catch (Exception e)
        {
            levelItem = GetLocalLevelItem(number);
        }

        return levelItem;
    }

    private void CheckLevelItem(LevelItemConfigs levelItem, int number, string levelName)
    {
        if (levelItem != null)
        {
            return;
        }

        throw new Exception($"Level {levelName} for number {number} not found. Config {Gamestoty.appVersion}");
    }

    public bool IsNowHintedLevel(int currentLevel)
    {
        var hintedLevelsConfig = GameConfig.RemoteConfig.hintedLevels;
        var hintedLevels = hintedLevelsConfig.Split(',');
        for (var i = 0; i < hintedLevels.Length; i++)
        {
            if (int.TryParse(hintedLevels[i], out var levelNum) && levelNum == currentLevel)
            {
                return true;
            }
        }

        return false;
    }

    public string Validate()
    {
        var gameConfig = GameConfig.RemoteConfig;


        if (gameConfig.LevelsSecondQueue.Length == 0)
        {
            return "LevelsSecondQueue is EMPTY";
        }

        var levelsCount = gameConfig.LevelsFirstQueue.Length + gameConfig.LevelsSecondQueue.Length;
        for (var i = 0; i < levelsCount; i++)
        {
            var levelName = GetRemoteLevelName(i);
            var levelItem = GetLevelItem(levelName);
            if (levelItem == null)
            {
                return $"level \"{levelName}\" not found!";
            }

            var text = levelItem.Validate();
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        for (var i = 0; i < _levelItemConfigs.Count; i++)
        {
            var levelItem = _levelItemConfigs[i];
            var text = levelItem.Validate();
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        var levels = Resources.LoadAll<LevelItemConfigs>("Levels/");
        for (var i = 0; i < levels.Length; i++)
        {
            var levelItem = levels[i];
            var text = levelItem.Validate();
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        return string.Empty;
    }

    private string GetRemoteLevelName(int number)
    {
        var gameConfig = GameConfig.RemoteConfig;
        var levelsFQ = gameConfig.LevelsFirstQueue;
        var levelsSQ = gameConfig.LevelsSecondQueue;
        var levelItemConfigName = number < levelsFQ.Length
            ? levelsFQ[number]
            : levelsSQ[(number - levelsFQ.Length) % levelsSQ.Length];
        return levelItemConfigName;
    }

    public static PinOutLevelConfig GetRemoteLevelNameSimple(int number)
    {
        var gameConfig = GameConfig.RemoteConfig;
        var levelList = gameConfig.levelList;

        if (number >= levelList.Length)
        {
            number = (number - levelList.Length) % (levelList.Length - 5) + 5;
        }

        return gameConfig.levelList[number % gameConfig.levelList.Length];
    }

    private LevelItemConfigs GetLevelItem(string levelName)
    {
        for (var i = 0; i < _levelItemConfigs.Count; i++)
        {
            var levelItemConfig = _levelItemConfigs[i];
            if (levelItemConfig.name == levelName)
            {
                return levelItemConfig;
            }
        }

        return Resources.Load<LevelItemConfigs>($"Levels/{levelName}");
    }

    private LevelItemConfigs GetLocalLevelItem(int number)
    {
        return _levelItemConfigs[number % _levelItemConfigs.Count];
    }
}
