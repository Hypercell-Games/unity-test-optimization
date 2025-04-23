using UnityEngine;

namespace Unpuzzle.Game.Data
{
    public static class LevelData
    {
        public static int SessionStartLevel { get; private set; }

        public static int SessionNumber { get; private set; }

        public static int GetLevelNumber()
        {
            return PlayerPrefsController.GetInt(EPlayerPrefsKey.game_levels_started);
        }

        public static void SetLevelNumber(int levelNumber)
        {
            PlayerPrefsController.SetInt(EPlayerPrefsKey.game_levels_started, levelNumber);
        }

        public static void SessionStarted()
        {
            SessionStartLevel = GetLevelNumber();

            SessionNumber = PlayerPrefs.GetInt(EPlayerPrefsKey.game_user_session_number.ToString(), 0);
            SessionNumber++;
            PlayerPrefs.SetInt(EPlayerPrefsKey.game_user_session_number.ToString(), SessionNumber);
        }
    }
}
