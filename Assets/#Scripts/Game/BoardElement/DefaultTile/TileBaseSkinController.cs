using System.Collections.Generic;
using UnityEngine;

public abstract class TileBaseSkinController<T> : TileBaseSkinController where T : SingleSkinConfigEntry
{
    protected abstract void SetSkin(T skin);

    public override void SetSkin(SingleSkinConfigEntry skin)
    {
        SetSkin((T)skin);
    }
}

public abstract class TileBaseSkinController : MonoBehaviour
{
    [field: SerializeField] public List<SpriteRenderer> SpritesForSorting { get; private set; }

    [field: SerializeField] public List<WrongDefaultGroup> WrongGroups { get; private set; }

    [SerializeField] private SpriteRenderer[] _lockedStageSprites;

    public abstract TileSkinType GetSkinType();

    public abstract void ChangeRotation(EMoveDirection moveDirection, Vector3 eulerAngles);

    public abstract void OnTilePressed();

    public abstract void OnTileUnPressed();

    public abstract void SetSkin(SingleSkinConfigEntry skin);

    public virtual void OnChangeSortOrder(int order) { }
}
