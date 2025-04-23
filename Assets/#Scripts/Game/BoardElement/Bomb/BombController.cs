using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BombController : BaseBoardElementController
{
    [SerializeField] private GameObject _explosionGameObject;

    public override void InitializeBoardElementInfo()
    {
    }

    public void Explode()
    {
        _explosionGameObject.transform.SetParent(null);
        _explosionGameObject.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public override int ChangeSortOrder(int sortOrder)
    {
        var highestSortOrder = base.ChangeSortOrder(sortOrder);
        highestSortOrder++;


        return highestSortOrder;
    }

    public List<GridCell> FindTargets(GridController grid)
    {
        var targets = new List<GridCell>();

        foreach (var element in grid.GetNeighbourCells(GridPosition.x, GridPosition.y, true))
        {
            if (targets.Contains(element))
            {
                continue;
            }


            if (!(element.BoardElementType == EBoardElementType.TILE_LOCKED ||
                  element.BoardElementType == EBoardElementType.TILE ||
                  element.BoardElementType == EBoardElementType.WALL ||
                  element.BoardElementType == EBoardElementType.BOMB))
            {
                continue;
            }

            targets.Add(element);

            if (element.BaseBoardElementController is BombController bomb)
            {
                targets.AddRange(bomb.FindTargets(grid));
            }
        }

        return targets.Distinct().ToList();
    }
}
