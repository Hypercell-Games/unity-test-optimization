using System;
using UnityEngine;

public class GridCell
{
    public EBoardElementType BoardElementType;
    public Vector2Int GridPoint;
    public int SortOrder;

    public Vector2 WorldPoint;

    public GridCell()
    {
    }

    public GridCell(GridCell sourceCell)
    {
        BoardElementType = sourceCell.BoardElementType;
        BaseBoardElementController = sourceCell.BaseBoardElementController;
        WorldPoint = sourceCell.WorldPoint;
        GridPoint = sourceCell.GridPoint;
        SortOrder = sourceCell.SortOrder;
    }

    public BaseBoardElementController BaseBoardElementController { get; set; }

    public event Action<GridCell> OnStateChanged;

    public void FireChanged()
    {
        OnStateChanged?.Invoke(this);
    }

    public override string ToString()
    {
        return
            $"{nameof(GridPoint)}: {GridPoint}, {nameof(BoardElementType)}: {BoardElementType}, {nameof(BaseBoardElementController)}: {BaseBoardElementController} ({GetHashCode()})";
    }
}
