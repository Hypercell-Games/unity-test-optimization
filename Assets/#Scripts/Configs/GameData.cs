using System;
using System.Collections.Generic;
using UnityEngine;
using Unpuzzle;

[CreateAssetMenu(fileName = "GameData", menuName = "Configs/GameData")]
public class GameData : ScriptableObject
{
    [field: SerializeField] public SkinsConfig SkinsConfig { get; private set; }

    [field: SerializeField] public AnimationSetting LobbyTabBarAnimationSetting { get; private set; }

    [field: Header("Prefs")]
    [field: SerializeField]
    public IntVariable SoftCurrency { get; private set; }

    [field: SerializeField] public StringVariable SelectedSkin { get; private set; }
    [field: SerializeField] public IntVariable KeysAmount { get; private set; }

    public GameMode CurrentGameMode { get; private set; }
    public GameMode PreviousGameMode { get; private set; }

    public int SelectedSkinIndex
    {
        get => PlayerPrefs.GetInt("selectedSkin.index", 0);
        set => PlayerPrefs.SetInt("selectedSkin.index", value);
    }

    public int SoftCcyAmount => SoftCurrency.Value;

    private void OnEnable()
    {
        SwitchCurrentGameMode();
    }

    private void OnDisable()
    {
        PreviousGameMode = null;
        CurrentGameMode = null;
    }

    public BgSkinConfig GetSelectedSkin()
    {
        return SkinsConfig.GetBgSkin(SelectedSkinIndex);
    }

    public BgSkinConfig GetChallengeSkin()
    {
        return SkinsConfig.GetBgChallengeSkin();
    }

    public void SetSkin(BgSkinConfig skin)
    {
        SelectedSkinIndex = SkinsConfig.GetSkinIndex(skin);

        if (LevelsController.Instance != null)
        {
            LevelsController.Instance.UpdateSkin();
        }
    }

    public List<BgSkinConfig> GetSkins()
    {
        return SkinsConfig.BgSkins;
    }

    public void GoToGameMode(GameMode mode)
    {
        PreviousGameMode = CurrentGameMode;
        CurrentGameMode = mode;

        if (PreviousGameMode != null)
        {
            if ((PreviousGameMode is LobbyMode && CurrentGameMode is NormalMode) ||
                (PreviousGameMode is NormalMode && CurrentGameMode is LobbyMode))
            {
                Action callBack = null;
                Action<float> onProgressChanged = null;
                if (LoaderScreen.Instance)
                {
                    LoaderScreen.Instance.Show();
                    callBack = LoaderScreen.Instance.Hide;
                    onProgressChanged = LoaderScreen.Instance.SetProgress;
                }

                SceneLoader.Instance.LoadSceneAsyncWithCallback(CurrentGameMode.GetSceneName(), onProgressChanged,
                    callBack);
                return;
            }
        }

        SceneLoader.Instance.LoadSceneAsyncWithCallback(CurrentGameMode.GetSceneName(), null, null);
    }

    public void SwitchCurrentGameMode(GameMode newMode = null)
    {
    }

    public bool IsNormalMode()
    {
        return IsCurrentMode<NormalMode>(out _);
    }

    public bool IsGameModeNotNull()
    {
        return CurrentGameMode != null;
    }

    public bool IsCurrentMode<T>(out T mode) where T : GameMode
    {
        if (CurrentGameMode is T gameMode)
        {
            mode = gameMode;
            return true;
        }

        mode = null;
        return false;
    }

    public void AddSoftCcy(int amount, string place = "")
    {
        SoftCurrency.ApplyChange(amount);
    }

    public void RemoveSoftCcy(int amount, string place = "")
    {
        SoftCurrency.ApplyChange(-amount);
    }
}
