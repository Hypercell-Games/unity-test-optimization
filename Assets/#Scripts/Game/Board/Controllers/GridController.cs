using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class GridController
{
    public readonly List<GridCell> _boardElements = new();
    private readonly GridMoveTester _gridMoveTester;

    private float cellSizeC = 1f;
    private float length = 1f;

    private float topClamp, leftClamp;
    private float widthLength, heightLength;

    public GridController(int gridWidth, int gridHeight)
    {
        Grid = new Grid(gridWidth, gridHeight);
        _gridMoveTester = new GridMoveTester(this);
    }

    public Grid Grid { get; }

    public List<GridCell> BoardElements => _boardElements;
    public float CellSize => length / cellSizeC;
    public int GridWidth => Grid.Width;
    public int GridHeight => Grid.Height;

    private float cellsDistanceX => CellSize * 0.03f;
    private float cellsDistanceY => -CellSize * 0.11f;

    public void CreateGridElements(Camera gameCamera, int sortOrder, float offsetX, float offsetY)
    {
        DefineClamps(gameCamera);

        var startBoardPoint = UpdateCellSizeAndLength();
        var xPos = startBoardPoint.x;
        var yPos = startBoardPoint.y;

        var increaseY = CellSize + cellsDistanceY;
        var increaseX = CellSize + cellsDistanceX;

        for (var yNumber = 1; yNumber <= GridHeight; yPos -= increaseY, yNumber++, xPos = startBoardPoint.x)
        {
            for (var xNumber = 1; xNumber <= GridWidth; xPos += increaseX, xNumber++)
            {
                var gridPosition = new Vector2(xPos, yPos);
                var elementPlace = new Vector2Int(xNumber - 1, yNumber - 1);
                var gridsCell = new GridCell
                {
                    WorldPoint = gridPosition + new Vector2(offsetX * increaseX, offsetY * increaseY),
                    GridPoint = elementPlace,
                    BoardElementType = EBoardElementType.EMPTY,
                    SortOrder = sortOrder * 1000 + yNumber * 10
                };

                gridsCell.OnStateChanged += OnCellStateChanged;

                AddCell(gridsCell);
            }
        }
    }

    public void FillGridFromJson(LevelJsonConverter.JsonFile json)
    {
        foreach (var baseElement in json.GetBaseElementsFlat())
        {
            var gridElement = Grid[baseElement.position.x, baseElement.position.y];

            var elementType = LevelJsonConverter.GetElementTypeForBaseElement(baseElement);

            if (!elementType.HasValue)
            {
                continue;
            }

            if (gridElement.BoardElementType == EBoardElementType.TILE_LOCKED && elementType == EBoardElementType.TILE)
            {
                continue;
            }

            gridElement.BoardElementType = elementType.Value;
        }
    }

    private void OnCellStateChanged(GridCell cell)
    {
        if (cell.BoardElementType == EBoardElementType.TILE_LOCKED &&
            !((TileElementController)cell.BaseBoardElementController).HasAnyRemainingActions())
        {
            UnlockActionsLockedElement(cell);
        }
    }

    private Vector2 UpdateCellSizeAndLength()
    {
        var startBoardPoint = GetStartBoardPoint();
        var xPos = startBoardPoint.x;
        var yPos = startBoardPoint.y;

        if (xPos >= leftClamp && yPos <= topClamp)
        {
            return startBoardPoint;
        }

        var percentageHorizontal = Mathf.Abs(xPos / leftClamp);
        var percentageVertical = Mathf.Abs(yPos / topClamp);
        if (percentageHorizontal > percentageVertical)
        {
            if (xPos < leftClamp)
            {
                cellSizeC = GridWidth - cellsDistanceX * (GridWidth - 1);
                length = widthLength;
            }
        }
        else if (yPos > topClamp)
        {
            cellSizeC = GridHeight + cellsDistanceY * (GridHeight - 1);
            length = heightLength;
        }

        return GetStartBoardPoint();
    }

    public void AddCell(GridCell cell)
    {
        Grid[cell.GridPoint.x, cell.GridPoint.y] = cell;
        _boardElements.Add(cell);
    }

    public void MoveCellContents(GridCell sourceCell, GridCell targetCell)
    {
        var targetPos = targetCell.GridPoint;
        var sourcePos = sourceCell.GridPoint;

        SetCellController(targetPos.x, targetPos.y, sourceCell.BaseBoardElementController, sourceCell.BoardElementType);
        SetCellController(sourcePos.x, sourcePos.y, null);
    }

    public void ClearTile(GridCell tile)
    {
        tile.BoardElementType = EBoardElementType.EMPTY;
        tile.BaseBoardElementController = null;
    }

    public void UnlockActionsLockedElement(GridCell tile)
    {
        tile.BoardElementType = EBoardElementType.TILE;
        UpdateTileInfo(tile, tile);
    }

    public void UpdateTileInfo(GridCell targetCell, GridCell cellData)
    {
        targetCell.BaseBoardElementController = cellData.BaseBoardElementController;
        targetCell.BoardElementType = cellData.BoardElementType;

        if (targetCell.BaseBoardElementController != null)
        {
            targetCell.BaseBoardElementController.ChangeSortOrder(targetCell.SortOrder);
        }
    }

    public void UpdateMultipleTilesInfo(Dictionary<GridCell, GridCell> targetSourceDict)
    {
        var modifiedCellsCpy = targetSourceDict.Keys
            .ToDictionary(gridCell => gridCell.GridPoint, gridCell => new GridCell(gridCell));

        foreach (var targetSourcePair in targetSourceDict)
        {
            var source = modifiedCellsCpy.ContainsKey(targetSourcePair.Value.GridPoint)
                ? modifiedCellsCpy[targetSourcePair.Value.GridPoint]
                : targetSourcePair.Value;

            UpdateTileInfo(targetSourcePair.Key, source);
        }

        var toClear = targetSourceDict.Values.Where(r => !targetSourceDict.ContainsKey(r));
        Array.ForEach(toClear.ToArray(), ClearTile);
    }

    public void SetCellController(int x, int y, BaseBoardElementController controller, EBoardElementType? type = null)
    {
        Grid[x, y].BaseBoardElementController = controller;
        Grid[x, y].BoardElementType = type ?? (controller?.BoardElementType ?? EBoardElementType.EMPTY);

        if (controller != null)
        {
            controller.GridPosition = Grid[x, y].GridPoint;
        }

        UpdateTileInfo(Grid[x, y], Grid[x, y]);
    }

    public bool CanDoAnyMove()
    {
        return _gridMoveTester.CanDoAnyMove(Grid);
    }

    public List<T> GetElementControllersOfType<T>() where T : BaseBoardElementController
    {
        return GetGridCellsOfTypeInternal<T>()
            .Select(r => r.BaseBoardElementController)
            .Cast<T>()
            .ToList();
    }

    public List<GridCell> GetGridCellsOfType<T>() where T : BaseBoardElementController
    {
        return GetGridCellsOfTypeInternal<T>().ToList();
    }

    private IEnumerable<GridCell> GetGridCellsOfTypeInternal<T>() where T : BaseBoardElementController
    {
        return GetGridCellsOfTypeInternal<T>(_boardElements);
    }

    public static List<GridCell> GetGridCellsOfType<T>(List<GridCell> boardState) where T : BaseBoardElementController
    {
        return GetGridCellsOfTypeInternal<T>(boardState).ToList();
    }

    private static IEnumerable<GridCell> GetGridCellsOfTypeInternal<T>(List<GridCell> boardState)
        where T : BaseBoardElementController
    {
        return boardState.Where(r => r.BaseBoardElementController is T);
    }

    public List<GridCell> GetLockedCells()
    {
        return _boardElements.Where(boardElement => boardElement.BoardElementType == EBoardElementType.TILE_LOCKED)
            .ToList();
    }

    public void SetBoardElementsStage(int stage, bool fast)
    {
        if (GameConfig.RemoteConfig.unlockStagesAnimation)
        {
            var countElements = _boardElements.Count(e => e.BoardElementType != EBoardElementType.EMPTY);
            var duration = Mathf.Clamp(countElements * 0.025f, 0.1f, 0.4f);
            for (int i = 0, counter = 0; i < _boardElements.Count; i++)
            {
                var e = _boardElements[i];
                if (e.BaseBoardElementController == null)
                {
                    continue;
                }

                var delay = DOVirtual.EasedValue(0f, 1f, Mathf.InverseLerp(0f, countElements - 1, counter),
                    Ease.OutSine);
                e.BaseBoardElementController.SetStageLayer(stage, fast, delay, duration);
                counter++;
            }
        }
        else
        {
            foreach (var boardElement in _boardElements)
            {
                if (boardElement.BaseBoardElementController == null)
                {
                    continue;
                }

                boardElement.BaseBoardElementController.SetStageLayer(stage, fast);
            }
        }
    }

    public GridCell GetCell(Vector2Int gridPoint)
    {
        return Grid[gridPoint.x, gridPoint.y];
    }

    public List<GridCell> GetNeighbourCells(int x, int y, bool includeOrdinal)
    {
        var neighbourCells = new List<GridCell>();

        for (var xi = -1; xi <= 1; xi++)
        {
            for (var yi = -1; yi <= 1; yi++)
            {
                var targetX = x + xi;
                var targetY = y + yi;

                if (xi == 0 && yi == 0)
                {
                    continue;
                }

                if (!includeOrdinal && Mathf.Abs(xi) == 1 && Mathf.Abs(yi) == 1)
                {
                    continue;
                }

                if (!Grid.IsWithinGridBounds(targetX, targetY))
                {
                    continue;
                }

                neighbourCells.Add(Grid[x + xi, y + yi]);
            }
        }

        return neighbourCells;
    }

    public GridCell GetCellByWorldPosition(Vector2 worldPosition)
    {
        GridCell element = default;
        foreach (var cell in _boardElements)
        {
            var offset = cell.WorldPoint - worldPosition;
            if (Mathf.Abs(offset.x) < CellSize * 0.5f && Mathf.Abs(offset.y) < CellSize * 0.5f)
            {
                element = cell;
                break;
            }
        }

        return element;
    }

    private Vector2 GetStartBoardPoint()
    {
        return new Vector2(
            -((GridWidth - 1f) * 0.5f) * CellSize - cellsDistanceX * ((GridWidth - 2f) * 0.5f),
            (GridHeight - 1f) * 0.5f * CellSize + cellsDistanceY * ((GridHeight - 1f) * 0.5f));
    }

    private void DefineClamps(Camera gameCamera)
    {
        if (gameCamera == null)
        {
            return;
        }

        leftClamp = gameCamera.ViewportToWorldPoint(new Vector2(0.1f, 0f)).x;
        topClamp = gameCamera.ViewportToWorldPoint(new Vector2(0f, 0.85f)).y;

        heightLength = Mathf.Abs(topClamp * 2f);
        widthLength = Mathf.Abs(leftClamp * 2f);
    }

    public bool RotateGridWithRotator(GridCell rotator)
    {
        return LevelRotatorsController.TryToRotate(this, rotator);
    }

    public void UpdateSortOrders()
    {
        BoardElements.ForEach(UpdateSortOrder);
    }

    private void UpdateSortOrder(GridCell boardElement)
    {
        if (!(boardElement.BaseBoardElementController is TileElementController tile))
        {
            return;
        }

        tile.ChangeSortOrder(boardElement.SortOrder);
    }

    public static Grid ConvertToGrid(List<GridCell> cells)
    {
        var maxX = cells.Max(r => r.GridPoint.x);
        var maxY = cells.Max(r => r.GridPoint.y);
        var grid = new Grid(maxX + 1, maxY + 1);

        cells.ForEach(r => grid[r.GridPoint.x, r.GridPoint.y] = r);

        return grid;
    }

    public RotatorElementController GetRotatorControlling(GridCell cell)
    {
        return GetElementControllersOfType<RotatorElementController>().FirstOrDefault(r => r.IsControlledCell(cell));
    }

    public List<GridCell> GetElementsInDirectionLine(GridCell startCell)
    {
        return GetElementsInDirectionLine(startCell, Grid);
    }

    public static List<GridCell> GetElementsInDirectionLine(GridCell startCell, Grid grid)
    {
        var tile = (TileElementController)startCell.BaseBoardElementController;

        return Grid.CellsInLine(startCell.GridPoint.x, startCell.GridPoint.y, grid, tile.EMoveDirection);
    }
}
