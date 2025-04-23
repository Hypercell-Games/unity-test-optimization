using UnityEngine;

public class TileComplexSkinController : TileBaseSkinController<ComplexSkinConfigEntry>
{
    [Header("Internal")] [SerializeField] private SpriteRenderer _tile;

    [SerializeField] private SpriteRenderer _wrongTile;

    [Header("Config")] [SerializeField] private Material _defaultMaterial;

    private bool _isPressed;

    private EMoveDirection _moveDirection;

    private ComplexSkinConfigEntry _skinConfig;

    protected override void SetSkin(ComplexSkinConfigEntry skin)
    {
        _skinConfig = skin;

        RefreshSkin();
    }

    private void RefreshSkin()
    {
        var targetCardinalSprite = _isPressed ? _skinConfig.PressedTile : _skinConfig.UnpressedTile;

        _tile.sprite = targetCardinalSprite.GetSprite(_moveDirection);
        _wrongTile.sprite = _skinConfig.WrongTile.GetSprite(_moveDirection);

        var targetMaterial = _skinConfig.OverrideMaterial != null ? _skinConfig.OverrideMaterial : _defaultMaterial;
        _tile.sharedMaterial = targetMaterial;
    }

    public override TileSkinType GetSkinType()
    {
        return TileSkinType.Complex;
    }

    public override void ChangeRotation(EMoveDirection moveDirection, Vector3 eulerAngles)
    {
        _moveDirection = moveDirection;

        RefreshSkin();
    }

    public override void OnTilePressed()
    {
        _isPressed = true;

        RefreshSkin();
    }

    public override void OnTileUnPressed()
    {
        _isPressed = false;

        RefreshSkin();
    }
}
