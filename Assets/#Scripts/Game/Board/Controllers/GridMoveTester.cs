using System.Collections.Generic;
using System.Linq;

public class GridMoveTester
{
    private readonly GridController gridController;

    public GridMoveTester(GridController gridController)
    {
        this.gridController = gridController;
    }

    public bool CanDoAnyMove(Grid grid)
    {
        var tiles = gridController.GetGridCellsOfType<TileElementController>();
        if (tiles.Count == 0)
        {
            return true;
        }

        if (CanMoveAnyTile(tiles, grid))
        {
            return true;
        }

        var testedBoardState = gridController.BoardElements.ToList();
        var rotators = gridController.GetElementControllersOfType<RotatorElementController>();

        if (CanDoAnyMoveAfterRotations(testedBoardState, rotators, tiles.Count))
        {
            return true;
        }

        return false;
    }

    private bool CanMoveAnyTile(List<GridCell> cellsWithTiles, Grid grid)
    {
        foreach (var element in cellsWithTiles)
        {
            if (!(element.BaseBoardElementController is TileElementController) ||
                element.BoardElementType == EBoardElementType.TILE_LOCKED)
            {
                continue;
            }

            var lineElements = GridController.GetElementsInDirectionLine(element, grid);
            if (lineElements.Count == 0)
            {
                return true;
            }


            var closestBoardElementType = lineElements[0].BoardElementType;
            if (closestBoardElementType == EBoardElementType.EMPTY ||
                closestBoardElementType == EBoardElementType.SAW ||
                closestBoardElementType == EBoardElementType.BOMB)
            {
                return true;
            }
        }

        return false;
    }

    private bool CanDoAnyMoveAfterRotations(List<GridCell> testedBoardState, List<RotatorElementController> rotators,
        int initialTilesCount)
    {
        foreach (var rotator in rotators)
        {
            if (rotator.IsRotating)
            {
                return true;
            }

            var availableRotations = rotator.GetAvailableRotationDirections(testedBoardState);
            if (availableRotations.Count == 0)
            {
                continue;
            }

            if (CanMoveOnAnyRotation(testedBoardState, rotators, initialTilesCount, rotator, availableRotations))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanMoveOnAnyRotation(List<GridCell> testedBoardState, List<RotatorElementController> rotators,
        int initialTilesCount, RotatorElementController rotator, List<int> rotations)
    {
        foreach (var rotation in rotations)
        {
            var tempBoardState = rotator.RotateOnTargetMultiplayer(testedBoardState.ToList(), rotation);
            var tempBoardGrid = GridController.ConvertToGrid(tempBoardState);
            var countTiles = GridController.GetGridCellsOfType<TileElementController>(tempBoardState).Count;

            if (CanMoveAnyTile(tempBoardState, tempBoardGrid) || initialTilesCount != countTiles)
            {
                return true;
            }

            var otherRotators = rotators.Where(r => r != rotator).ToList();
            if (CanDoAnyMoveAfterRotations(tempBoardState, otherRotators, initialTilesCount))
            {
                return true;
            }
        }

        return false;
    }
}
