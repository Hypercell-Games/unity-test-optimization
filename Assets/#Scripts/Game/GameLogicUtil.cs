using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using static LevelJsonConverter;

namespace Unpuzzle.Game
{
    public static class GameLogicUtil
    {
        public const string DATE_FORMAT = "dd.MM.yyyy";

        public static bool ShouldShowLobbyLevelTransition { get; set; }

        public static int GetMovesForLevel(JsonFile level)
        {
            var config = GameConfig.RemoteConfig.maxMovesConfig;
            if (!config.enabled)
            {
                return int.MaxValue;
            }

            if (!string.IsNullOrEmpty(level.name))
            {
                var entry = config.FindBoardEntry(level.name);
                if (entry != null)
                {
                    return entry.moves;
                }
            }

            return GetMovesForLevelWithFormula(level);
        }

        public static int GetMovesForLevelWithFormula(JsonFile level)
        {
            return level.moves > 0
                ? level.moves
                : level.tiles.Length * 2 +
                  level.bombs.Length * 2 +
                  level.rotators.Length * 4;
        }

        public static int CoinsAmountForLevel(int level)
        {
            return 5 + level;
        }

        public static List<int> SplitIntString(string value, char separator = ',')
        {
            if (string.IsNullOrEmpty(value))
            {
                return new List<int>();
            }

            return value.Split(separator).Select(int.Parse).ToList();
        }

        public static List<string> SplitString(string value, char separator = ',')
        {
            return string.IsNullOrEmpty(value) ? new List<string>() : value.Split(separator).ToList();
        }

        public static int GetShopRandomSkinPrice(int index)
        {
            var values = SplitIntString(GameConfig.RemoteConfig.shopRandomSkinCost);
            return values[Mathf.Min(index, values.Count - 1)];
        }

        public static DateTime ParseStringToDate(string dateTimeString)
        {
            return DateTime.ParseExact(dateTimeString, DATE_FORMAT, CultureInfo.InvariantCulture);
        }

        public static string DateToString(DateTime date)
        {
            return date.ToString(DATE_FORMAT, CultureInfo.InvariantCulture);
        }

        public static bool IsLobbyEnabled()
        {
            return true;
        }

        public static bool TryGoToLobby(GameData gameData, bool levelComplete = false,
            LevelType levelType = LevelType.Normal)
        {
            var isNormalMode = gameData.IsCurrentMode<NormalMode>(out var normalMode);
            if (IsLobbyEnabled() && isNormalMode)
            {
                var showLevelCompleteInter = levelComplete &&
                                             ((levelType == LevelType.Normal &&
                                               GameConfig.RemoteConfig.showInterAtLevelComplete) ||
                                              (levelType == LevelType.Challenge && GameConfig.RemoteConfig
                                                  .showInterAtLevelCompleteChallenge) ||
                                              (levelType == LevelType.Boss &&
                                               GameConfig.RemoteConfig.showInterAtLevelCompleteBoss));
                if (showLevelCompleteInter)
                {
                    var interstitialId = levelType switch
                    {
                        LevelType.Normal => "ad_inter_level_complete",
                        LevelType.Boss => "ad_inter_level_complete_boss",
                        LevelType.Challenge => "ad_inter_level_complete_challenge",
                        _ => "ad_inter_level_complete"
                    };
                    HyperKit.Ads.ShowInterstitial(interstitialId, _ => GoToLobby());
                }
                else
                {
                    GoToLobby();
                }

                void GoToLobby()
                {
                    if (normalMode.ChallengeLevel)
                    {
                        gameData.GoToGameMode(LobbyMode.CreateChallengesMode(gameData, normalMode.ChallengeLevelIndex));
                    }
                    else
                    {
                        gameData.GoToGameMode(new LobbyMode(gameData));
                    }
                }

                return true;
            }

            return false;
        }
    }
}
