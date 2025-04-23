using UnityEngine;

public class TileSimpleSkinController : TileBaseSkinController<StandardSkinConfigEntry>
{
    [Header("Internal")] [SerializeField] private Transform _arrowTransform;

    [SerializeField] private SpriteRenderer _arrowSprite;
    [SerializeField] private SpriteRenderer _unpressedTile;
    [SerializeField] private SpriteRenderer _pressedTile;
    [SerializeField] private SpriteRenderer _wrongUnpressedTile;
    [SerializeField] private SpriteRenderer _wrongPressedTile;

    protected override void SetSkin(StandardSkinConfigEntry skin)
    {
        _arrowSprite.sprite = skin.ArrowSprite;
        _unpressedTile.sprite = skin.DefaultSprite;
        _pressedTile.sprite = skin.PressedSprite;
        _wrongUnpressedTile.sprite = skin.WrongSprite;
        _wrongPressedTile.sprite = skin.WrongPushSprite;
    }

    public override TileSkinType GetSkinType()
    {
        return TileSkinType.Simple;
    }

    public override void ChangeRotation(EMoveDirection moveDirection, Vector3 eulerAngles)
    {
        _arrowTransform.eulerAngles = eulerAngles + new Vector3(0, 0, 180);
    }

    public override void OnTilePressed()
    {
        _arrowTransform.localPosition += Vector3.down * 0.2f;
        _unpressedTile.gameObject.SetActive(false);
        _pressedTile.gameObject.SetActive(true);
    }

    public override void OnTileUnPressed()
    {
        _arrowTransform.localPosition = Vector3.zero;
        _unpressedTile.gameObject.SetActive(true);
        _pressedTile.gameObject.SetActive(false);
    }
}
