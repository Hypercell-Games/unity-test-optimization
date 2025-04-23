using System;

public class SettingsController : ZenjectSingleton<SettingsController>
{
    public bool IsVibrationEnable { get; private set; } = true;
    public event Action<bool> onVibrationEnable;

    public override void Initialize()
    {
        base.Initialize();
        LoadSettings();
    }

    protected virtual void LoadSettings()
    {
        var isVibrationEnable = PlayerPrefsController.GetBool(EPlayerPrefsKey.settings_vibration_enable,
            GameConfig.RemoteConfig.defaultSettingVibration);
        SetVibrationEnable(isVibrationEnable, false);
    }

    public virtual void SetVibrationEnable(bool value, bool save = true)
    {
        IsVibrationEnable = value;
        onVibrationEnable?.Invoke(value);

        if (save)
        {
            PlayerPrefsController.SetBool(EPlayerPrefsKey.settings_vibration_enable, value);
        }
    }
}
