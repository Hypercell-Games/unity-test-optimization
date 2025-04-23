using UnityEngine;

namespace Unpuzzle.Game.Data
{
    public class SurveyData
    {
        private const string KEY_SURVEY_ID = "survey.id";
        private const string KEY_SURVEY_LASTL_LEVEL = "survey.last_level";

        public static int GetID()
        {
            return PlayerPrefs.GetInt(KEY_SURVEY_ID, 0);
        }

        public static int IncrementID()
        {
            var newId = GetID() + 1;

            PlayerPrefs.SetInt(KEY_SURVEY_ID, newId);

            return newId;
        }

        public static int GetLastShowLevel()
        {
            return PlayerPrefs.GetInt(KEY_SURVEY_LASTL_LEVEL, 0);
        }

        public static void SetLastShowLevel(int level)
        {
            PlayerPrefs.SetInt(KEY_SURVEY_LASTL_LEVEL, level);
        }
    }
}
