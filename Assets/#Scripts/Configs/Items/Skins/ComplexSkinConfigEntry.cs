using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ComplexSkin", menuName = MenuPath, order = MenuOrder)]
public class ComplexSkinConfigEntry : SingleSkinConfigEntry
{
    private const string MenuPath = "Configs/ComplexSkinConfig";
    private const int MenuOrder = int.MinValue + 108;

    [Header("Sprites")] [SerializeField] private CardinalSprite _unpressedTile;

    [SerializeField] private CardinalSprite _pressedTile;
    [SerializeField] private CardinalSprite _wrongTile;

    [Header("Other")] [SerializeField] private Material _overrideMaterial;

    public CardinalSprite UnpressedTile => _unpressedTile;
    public CardinalSprite PressedTile => _pressedTile;
    public CardinalSprite WrongTile => _wrongTile;
    public Material OverrideMaterial => _overrideMaterial;

    public override TileSkinType GetSkinType()
    {
        return TileSkinType.Complex;
    }
}

[Serializable]
public class CardinalSprite
{
    [field: SerializeField] public Sprite Up { get; private set; }
    [field: SerializeField] public Sprite Down { get; private set; }
    [field: SerializeField] public Sprite Left { get; private set; }
    [field: SerializeField] public Sprite Right { get; private set; }

    public Sprite GetSprite(EMoveDirection moveDirection)
    {
        return moveDirection switch
        {
            EMoveDirection.UP => Up,
            EMoveDirection.LEFT => Left,
            EMoveDirection.DOWN => Down,
            EMoveDirection.RIGHT => Right,
            _ => throw new ArgumentOutOfRangeException(nameof(moveDirection), moveDirection, null)
        };
    }
}
