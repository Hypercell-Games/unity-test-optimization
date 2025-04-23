using System;
using System.Linq;
using UnityEngine;
using Unpuzzle;

[Serializable]
public class GameConfig : HyperKitConfig
{
    public PinOutLevelConfig[] levelList = {};

    public float speedToHide = 80f;
    public float timeToPinOut = 0.5f;

    public bool uiTopGradientEnabled = true;

    public string[] levels = { "level1", "level2" };
    public string[] levelsSecondQueue;
    public string backgroundColor = "#FFFFFF";
    public string hintedLevels = "1,2,7";
    public string androidAppURL = "";
    public int minRateUsLevel = 3;
    public int openAppUrlAtRate = 3;
    public int tilePathEffectVersion;
    public int wrongClickAnimationVersion = 2;
    public float blockedShakeStrenght = 0.3f;
    public int wrongClickHapticVersion = 1;
    public int tapOnTileHapticVersion = 1;
    public int moveTileVersion = 1;
    public int showAdsAtLevel = 5;
    public int stagesSystemVersion;
    public float tilePressedScale = 0.95f;
    public float adsFadeInDuration = 0.9f;
    public float adsFadeOutDuration = 0.9f;
    public bool adsFadeIcon = true;
    public float adsFadeScreenCloseModeDelay = 1f;
    public int adsBreakeCoins = 5;
    public float adsBreakDelayShowing = 2;
    public float adsBreakDelayHide = 1;
    public bool adsBreakeEnabled = true;
    public bool unlockStagesAnimation = true;
    public bool unlockStagesHaptic = true;
    public float tileBouncePower = 0.1f;
    public float tileBounceStep = 0.99f;
    public float tileBounceDelay = 0.06f;
    public float completeScreenDelay = 0.3f;
    public int showInterstitialBetweenStages = 1;
    public bool hintWhenPlayerStuck = true;
    public float hintWhenPlayerStuckDelay = 5f;
    public float hintWhenPlayerStuckScale = 1.05f;
    public bool defaultSettingVibration = true;
    public float fireTriggerSize = 10f;
    public float defaultDpi = 460f;
    public float tapMaxZone = 5f;
    public float maxZoom = 800f;
    public float zoomSensivity = 1f;
    public float swipeSensivity = 3f;
    public bool cameraRotationRalativeLevel;
    public bool gameTimerStartAfterMove;
    public float fogDistanceStart = 10f;
    public float fogDistanceEnd = 20f;
    public bool juiceEffectEnable = true;
    public float CameraMinNearClipPlane = 0.1f;
    public float CameraHalfFrustumDepth = 250f;
    public bool tapAwayTutor = true;
    public float tapAwayDragLimit = 0.1f;
    public int tapAwayNotShowAdsIfThereAreBlocksLeft = 20;
    public MaxMovesConfig maxMovesConfig = new();

    public string randomSkinsUnlockOrder =
        "StandardMultiSkin,ComplexMultiSkinWooden,ComplexMultiSkinSimpleFlat,OldKeysSkinBlue,ComplexMultiSkinColorKeyboard,ComplexSkinSwitchRed,ComplexMultiSkinCars,StandardMultiSkinPopIt,ComplexMultiSkinRockets";

    public string shopRandomSkinCost = "200, 300, 400";
    public int shopFreeCoinsAmount = 100;
    public float rewardForDaily = 1f;
    public float scalePinRadius = 1f;
    public bool additionalPinColliderEnabled = true;
    public bool outlineVariant = true;
    public float batchingMPBPeriod = 4;
    public int timeRedEffect = 10;
    public bool newPinMoveOutScaleAnimation = true;
    public bool zoomNewEnabled = true;
    public bool showInterAtLevelStart;
    public bool showInterAtLevelComplete = true;
    public bool showInterAtLevelCompleteChallenge = true;
    public bool showInterAtLevelCompleteBoss = true;

    public static GameConfig RemoteConfig => HyperKit.Data.GetCustomConfig<GameConfig>();
    public float secondStepHookCastRadius => 3f;

    public string[] LevelsFirstQueue => levels != null && levels.Length > 0
        ? levels
        : Enumerable.Range(1, 120).Select(i => $"LevelItem {i}").ToArray();

    public string[] LevelsSecondQueue => levelsSecondQueue != null && levelsSecondQueue.Length > 0
        ? levelsSecondQueue
        : Enumerable.Range(5, 120 - 4).Select(i => $"LevelItem {i}").ToArray();
}

[Serializable]
public class PinOutLevelConfig
{
    public enum LevelModeType
    {
        Moves,
        Time
    }

    public string name;
    public int moves;
    public PinMaterialId pinMaterialId;
    public bool isTarget;
    public int time;
    [SerializeField] private string levelDifficulty = LevelDifficultyType.Normal.ToString();

    public LevelModeType LevelMode => time > 0 ? LevelModeType.Time : LevelModeType.Moves;

    public LevelDifficultyType LevelDifficultyType =>
        Enum.TryParse<LevelDifficultyType>(levelDifficulty, out var result) ? result : LevelDifficultyType.Normal;

    public static PinOutLevelConfig CreateBossLevel(string name, int time)
    {
        return new PinOutLevelConfig { name = name, time = time, isTarget = true };
    }
}
