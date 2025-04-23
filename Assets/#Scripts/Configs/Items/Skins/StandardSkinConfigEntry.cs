using UnityEngine;

[CreateAssetMenu(fileName = "StandardSkin", menuName = MenuPath, order = MenuOrder)]
public class StandardSkinConfigEntry : SingleSkinConfigEntry
{
    private const string MenuPath = "Configs/StandardSkinConfig";
    private const int MenuOrder = int.MinValue + 107;

    [Header("Sprites")] [SerializeField] private Sprite _defaultSprite;

    [SerializeField] private Sprite _pressedSprite;
    [SerializeField] private Sprite _wrongSprite;
    [SerializeField] private Sprite _wrongPushingSprite;
    [SerializeField] private Sprite _arrowSprite;

    public Sprite PressedSprite => _pressedSprite;
    public Sprite DefaultSprite => _defaultSprite;
    public Sprite WrongSprite => _wrongSprite;
    public Sprite WrongPushSprite => _wrongPushingSprite;
    public Sprite ArrowSprite => _arrowSprite;

    public override TileSkinType GetSkinType()
    {
        return TileSkinType.Simple;
    }
}
