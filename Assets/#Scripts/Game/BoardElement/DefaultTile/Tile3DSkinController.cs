using DG.Tweening;
using UnityEngine;

public class Tile3DSkinController : TileBaseSkinController<ModelSkinConfigEntry>
{
    [Header("Internal")] [SerializeField] private MeshRenderer _meshRenderer;

    [SerializeField] private MeshFilter _meshFilter;
    private EMoveDirection _moveDirection;

    private ModelSkinConfigEntry _skinConfig;

    protected override void SetSkin(ModelSkinConfigEntry skin)
    {
        _skinConfig = skin;

        _meshFilter.mesh = skin.Mesh;
        _meshRenderer.materials = skin.Materials;
    }

    public override TileSkinType GetSkinType()
    {
        return TileSkinType.Model;
    }

    public override void ChangeRotation(EMoveDirection moveDirection, Vector3 eulerAngles)
    {
        _moveDirection = moveDirection;

        var meshTransform = _meshRenderer.transform;
        meshTransform.localEulerAngles = new Vector3(meshTransform.localEulerAngles.x, -eulerAngles.z,
            meshTransform.localEulerAngles.z);
    }

    public override void OnTilePressed()
    {
        _meshRenderer.transform.DOScaleZ(0.9f, 0.1f).SetEase(Ease.OutSine);
    }

    public override void OnTileUnPressed()
    {
        _meshRenderer.transform.DOScaleZ(1, 0.1f).SetEase(Ease.OutSine);
    }

    public override void OnChangeSortOrder(int order)
    {
        transform.localPosition =
            new Vector3(transform.localPosition.x, transform.localPosition.y, -order / 10f + 10);
    }
}
