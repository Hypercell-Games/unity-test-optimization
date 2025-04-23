using System;
using Lofelt.NiceVibrations;

[Serializable]
public enum EVibrationType
{
    None = HapticPatterns.PresetType.None,

    Selection = HapticPatterns.PresetType.Selection,

    Success = HapticPatterns.PresetType.Success,
    Warning = HapticPatterns.PresetType.Warning,
    Failure = HapticPatterns.PresetType.Failure,

    LightImpact = HapticPatterns.PresetType.LightImpact,
    MediumImpact = HapticPatterns.PresetType.MediumImpact,
    HeavyImpact = HapticPatterns.PresetType.HeavyImpact,
    RigidImpact = HapticPatterns.PresetType.RigidImpact,
    SoftImpact = HapticPatterns.PresetType.SoftImpact
}
