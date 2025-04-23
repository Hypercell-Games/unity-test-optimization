using UnityEngine;

public class WallController : BaseBoardElementController, IDestructibleElement
{
    [SerializeField] private ParticleSystem _dieEffect;

    public void PlayDestroyFX()
    {
        _dieEffect.transform.SetParent(null);
        _dieEffect.Play();
        gameObject.SetActive(false);
    }

    public override void InitializeBoardElementInfo()
    {
    }

    public override int ChangeSortOrder(int sortOrder)
    {
        var highestSortOrder = base.ChangeSortOrder(sortOrder);
        highestSortOrder++;

        BoardElementUtils.ChangeComponentsSortOrder(_dieEffect, sortOrder + 1000);

        return highestSortOrder;
    }
}
