using System;
using UnityEngine;

public abstract class BaseSkinConfigEntry : JsonVariable<BaseSkinConfigEntry.SaveData>, IGalleryItem
{
    [Header("Gallery config")] [Tooltip("Thumbnail used in gallery")]
    public Sprite thumb;

    [Tooltip("Image used in unlock animation")]
    public Sprite bigThumb;

    public string GetName()
    {
        return name;
    }

    public Sprite GetThumb()
    {
        return thumb;
    }

    public Sprite GetBigThumb()
    {
        return bigThumb;
    }

    public bool IsUnlocked()
    {
        return Value.unlocked;
    }

    public bool SetUnlocked()
    {
        if (IsUnlocked())
        {
            return false;
        }

        SetValue(new SaveData { unlocked = true, introduced = Value.introduced });

        return true;
    }

    public bool IsIntroduced()
    {
        return Value.introduced;
    }

    public bool SetIntroduced()
    {
        if (IsIntroduced())
        {
            return false;
        }

        SetValue(new SaveData { unlocked = Value.unlocked, introduced = true });

        return true;
    }

    public abstract SingleSkinConfigEntry ProduceSingleSkinConfig(int level, int stage, ETileColor color);

    [Serializable]
    public struct SaveData
    {
        public bool introduced;
        public bool unlocked;
    }
}
