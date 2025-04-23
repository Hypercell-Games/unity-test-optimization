using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unpuzzle.Game;

namespace Utils
{
    public class DailyModeData
    {
        private const string PREFS_KEY_MASK = "DailyChallengeData";
        private readonly Dictionary<int, DailyEventDaySaveData> saveDayDataLookup = new();

        public DateTime FirstDay = DateTime.Now;

        private DailyEventSaveData saveData = new();

        public DailyModeData()
        {
            Load();
        }

        public DateTime FirstMonth => new(FirstDay.Year, FirstDay.Month, 01);

        private void Load()
        {
            var json = PlayerPrefs.GetString(PREFS_KEY_MASK, null);
            if (!string.IsNullOrEmpty(json))
            {
                saveData = JsonUtility.FromJson<DailyEventSaveData>(json);
            }

            RecreateDataLookup();
        }

        public int CountCompletedDaysInMonth(int month, int year)
        {
            return saveData.daysProgress.Count(r => r.month == month && r.year == year && r.completed);
        }

        public bool AllDaysInMonthCompleted(int month, int year)
        {
            var currentMonthDays = saveData.daysProgress.FindAll(
                r => r.month == month && r.year == year);

            return currentMonthDays.All(r => r.completed);
        }

        private void RecreateDataLookup()
        {
            saveDayDataLookup.Clear();
            foreach (var data in saveData.daysProgress)
            {
                AddDayToLookupDict(data);

                var dateTime = new DateTime(data.year, data.month, data.day);
                if (dateTime < FirstDay)
                {
                    FirstDay = dateTime;
                }
            }
        }

        private void AddDayToLookupDict(DailyEventDaySaveData data)
        {
            saveDayDataLookup.Add(HashDay(data.day, data.month, data.year), data);
        }

        private void Save()
        {
            var json = JsonUtility.ToJson(saveData);

            PlayerPrefs.SetString(PREFS_KEY_MASK, json);
            PlayerPrefs.Save();
        }

        private DailyEventDaySaveData GetDayData(int day, int month, int year)
        {
            if (saveDayDataLookup.TryGetValue(HashDay(day, month, year), out var data))
            {
                return data;
            }

            data = new DailyEventDaySaveData(day, month, year);

            saveData.daysProgress.Add(data);
            AddDayToLookupDict(data);

            return data;
        }

        public bool IsModeUnlocked()
        {
            return saveData.unlocked;
        }

        public void SetModeUnlocked()
        {
            saveData.unlocked = true;

            Save();
        }

        public bool IsStarted(int day, int month, int year)
        {
            return GetDayData(day, month, year).started;
        }

        public bool IsCompleted(int day, int month, int year)
        {
            return GetDayData(day, month, year).completed;
        }

        public bool HasFirstLevelDate()
        {
            return !string.IsNullOrWhiteSpace(saveData.firstLevelDate);
        }

        public DateTime GetFirstLevelDate()
        {
            return GameLogicUtil.ParseStringToDate(saveData.firstLevelDate);
        }

        public void SetFirstLevelDate(DateTime date)
        {
            saveData.firstLevelDate = GameLogicUtil.DateToString(date);
            Save();
        }

        public void SetStarted(int day, int month, int year)
        {
            var dayData = GetDayData(day, month, year);
            if (dayData.started)
            {
                return;
            }

            dayData.started = true;
            Save();
        }

        public void SetCompleted(int day, int month, int year)
        {
            var dayData = GetDayData(day, month, year);
            if (dayData.completed)
            {
                return;
            }

            dayData.completed = true;
            Save();
        }

        public int GetLastWidgetShownDay()
        {
            return saveData.lastWidgetShownDay;
        }

        public void SetLastWidgetShownDay(int day)
        {
            saveData.lastWidgetShownDay = day;
            Save();
        }

        private static int HashDay(int day, int month, int year)
        {
            var hashCode = day;
            hashCode = (hashCode * 397) ^ month;
            hashCode = (hashCode * 397) ^ year;
            return hashCode;
        }

        [Serializable]
        public class DailyEventSaveData
        {
            public bool unlocked;
            public int lastWidgetShownDay = -1;

            public string firstLevelDate;

            public List<DailyEventDaySaveData> daysProgress = new();
        }

        [Serializable]
        public class DailyEventDaySaveData
        {
            public int day;
            public int month;
            public int year;

            public bool started;
            public bool completed;

            public DailyEventDaySaveData(int day, int month, int year)
            {
                this.day = day;
                this.month = month;
                this.year = year;
            }
        }
    }
}
