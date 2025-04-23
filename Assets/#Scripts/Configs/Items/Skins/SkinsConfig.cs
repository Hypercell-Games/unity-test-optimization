using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unpuzzle;

[CreateAssetMenu(fileName = "SkinsConfig", menuName = MenuPath, order = MenuOrder)]
public class SkinsConfig : ScriptableObject
{
    public enum SkinBank
    {
        AllInGame,
        Debug,
        All
    }

    private const string MenuPath = "Configs/SkinsConfig";
    private const int MenuOrder = int.MinValue + 106;

    [SerializeField] private BaseSkinConfigEntry _debugSkin;

    [SerializeField] private List<BgSkinConfig> _bgSkins;
    [SerializeField] private BgSkinConfig _challengeBgSkin;

    public List<BgSkinConfig> BgSkins => _bgSkins;

    public int SkinsCount => _bgSkins.Count;

    public BgSkinConfig GetDefaultSkin()
    {
        return _bgSkins.FirstOrDefault();
    }

    public BgSkinConfig GetBgSkin(int index)
    {
        return _bgSkins[index];
    }

    public BgSkinConfig GetRandomSkinToUnlock()
    {
        var availableSkins = _bgSkins.FindAll(a => !a.IsUnlocked());

        if (availableSkins.Count == 0)
        {
            return null;
        }

        return availableSkins[Random.Range(0, availableSkins.Count)];
    }

    public BgSkinConfig GetAvailableSkinToIndroduce()
    {
        var availableSkin = _bgSkins.Find(a => !a.IsIntroduced);

        return availableSkin;
    }

    public int GetSkinIndex(BgSkinConfig skin)
    {
        return _bgSkins.IndexOf(skin);
    }

    public BgSkinConfig GetBgChallengeSkin()
    {
        return _challengeBgSkin;
    }
}
