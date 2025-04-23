using System;
using UnityEngine;

namespace Unpuzzle
{
    public static class BoosterHintUtil
    {
        private const string BOOSTER_COUNT_KEY = "USER.BOOSTERS.HINT";

        public static event Action OnBoosterCountChange = () => { };

        public static int GetBoosterCount()
        {
            return PlayerPrefs.GetInt(BOOSTER_COUNT_KEY, 5);
        }

        public static void AddBoster(int amount, string place)
        {
            if (amount < 0)
            {
                return;
            }

            PlayerPrefs.SetInt(BOOSTER_COUNT_KEY, GetBoosterCount() + amount);

            OnBoosterCountChange();
        }

        public static void BoosterUsed()
        {
            var boosterCount = GetBoosterCount();
            boosterCount -= 1;
            boosterCount = Mathf.Max(boosterCount, 0);

            PlayerPrefs.SetInt(BOOSTER_COUNT_KEY, boosterCount);

            OnBoosterCountChange();
        }
    }
}
