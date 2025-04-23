using UnityEngine;

namespace Unpuzzle
{
    public static class GameStateSaveUtils
    {
        private const string SAVE_LEVEL_STATE_KEY = "user.data.level.state";

        private const string IsLastStateWasFailScreenKey = "IsLastStateWasFailScreen";

        public static bool IsLastStateWasFailScreen
        {
            get => PlayerPrefs.GetInt(IsLastStateWasFailScreenKey, 0) != 0;
            set => PlayerPrefs.SetInt(IsLastStateWasFailScreenKey, value ? 1 : 0);
        }

        public static void CheckGameStart()
        {
            if (IsLastStateWasFailScreen)
            {
                ClearSavedData();
                IsLastStateWasFailScreen = false;
            }
        }

        public static void SaveData(PinOutLevelData saveData, int level, int movesLeft)
        {
            var savedData = new GameStateSaveData(saveData, level, movesLeft);

            PlayerPrefs.SetString(SAVE_LEVEL_STATE_KEY, JsonUtility.ToJson(savedData));
        }

        public static void ClearSavedData()
        {
            PlayerPrefs.SetString(SAVE_LEVEL_STATE_KEY, string.Empty);
            PlayerPrefs.DeleteKey(SAVE_LEVEL_STATE_KEY);
        }

        public static bool TryGetState(int level, string levelId, out GameStateSaveData state)
        {
            state = null;

            if (!PlayerPrefs.HasKey(SAVE_LEVEL_STATE_KEY))
            {
                return false;
            }

            var saveString = PlayerPrefs.GetString(SAVE_LEVEL_STATE_KEY);

            if (string.IsNullOrEmpty(saveString))
            {
                return false;
            }

            var savedState = JsonUtility.FromJson<GameStateSaveData>(saveString);

            if (savedState.appVer != Application.version)
            {
                return false;
            }

            if (savedState.levelName != levelId)
            {
                return false;
            }

            if (savedState.levelNum != level)
            {
                return false;
            }

            state = savedState;

            return true;
        }
    }
}
