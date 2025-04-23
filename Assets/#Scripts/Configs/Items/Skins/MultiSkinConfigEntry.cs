using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

[CreateAssetMenu(fileName = "MultiSkin", menuName = MenuPath, order = MenuOrder)]
public class MultiSkinConfigEntry : BaseSkinConfigEntry
{
    private const string MenuPath = "Configs/MultiSkinConfig";
    private const int MenuOrder = int.MinValue + 109;

    [Header("Skin config")] [SerializeField]
    private List<SingleSkinConfigEntry> skins = new();

    public override SingleSkinConfigEntry ProduceSingleSkinConfig(int level, int stage,
        ETileColor color = ETileColor.None)
    {
        if (color != ETileColor.None)
        {
            foreach (var skin in skins)
            {
                if (skin.TileColor == color)
                {
                    return skin;
                }
            }
        }

        {
            var random = new Random(10000 + level * 10 + stage);
            var skinNum = random.Next() % skins.Count;
            var skin = skins[skinNum];
            return skin;
        }
    }
}
