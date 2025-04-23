using System;
using Lofelt.NiceVibrations;
using UnityEngine;
using Zenject;

public class VibrationController : ZenjectSingleton<VibrationController>, IDisposable
{
    public bool IsEnable { get; private set; }
    public bool IsSupport => DeviceCapabilities.isVersionSupported || Application.isEditor;

    public virtual void Dispose()
    {
        if (SettingsController.HasInstance)
        {
            SettingsController.Instance.onVibrationEnable -= OnVibrationEnable;
        }
    }

    [Inject]
    protected virtual void Initialize(SettingsController settingsController)
    {
        settingsController.onVibrationEnable += OnVibrationEnable;

        OnVibrationEnable(settingsController.IsVibrationEnable);
    }

    public virtual void Play(EVibrationType vibration)
    {
        if (!IsSupport || !IsEnable)
        {
            return;
        }

        var preset = (HapticPatterns.PresetType)vibration;
        HapticPatterns.PlayPreset(preset);
    }

    public virtual void Vibrate()
    {
        Handheld.Vibrate();
    }

    #region SETTINGS

    protected virtual void OnVibrationEnable(bool value)
    {
        IsEnable = value;
    }

    #endregion SETTINGS
}
