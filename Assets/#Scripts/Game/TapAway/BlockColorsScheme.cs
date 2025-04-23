using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockColorsScheme", menuName = "Game/BlockColorScheme")]
public class BlockColorsScheme : ScriptableObject
{
    [SerializeField] private List<BlockMaterials> _blockMaterials = new();
    [SerializeField] private List<BlockMaterials> _blockTapMaterials = new();

    public BlockMaterials GetMaterials(BlockColor blockColor)
    {
        return _blockMaterials.FirstOrDefault(bm => bm.BlockColor == blockColor);
    }

    public List<BlockMaterials> GetTapMaterials()
    {
        return _blockTapMaterials;
    }
}

[Serializable]
public class BlockMaterials
{
    [SerializeField] private BlockColor _blockColor;
    [SerializeField] private Material _material;
    [SerializeField] private Material _outlineMaterial;

    public BlockMaterials(Material material)
    {
        _material = material;
    }

    public BlockColor BlockColor => _blockColor;
    public Material Material => _material;
}
