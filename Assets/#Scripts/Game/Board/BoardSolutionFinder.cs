using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;

public static class BoardSolutionFinder
{
    private static Cell _emptyCell;
    private static readonly Dictionary<Grid, Thread> _threads = new();

    public static Stopwatch SW { get; private set; }

    public static GridCell SolutionCell { get; private set; }

    public static bool SoultionFinderFinished { get; private set; }

    public static bool HasSolution { get; private set; }

    public static int MinMoves { get; private set; } = int.MaxValue;

    public static void Reset(Grid grid)
    {
        if (_threads.ContainsKey(grid))
        {
            var thread = _threads[grid];
            thread.Abort();
            _threads.Remove(grid);
        }
    }

    public static void FindSolution(Grid grid, LevelJsonConverter.JsonFile jsonFile = null)
    {
        var width = grid.Width;
        var height = grid.Height;

        if (SW == null)
        {
            SW = new Stopwatch();
            SW.Start();
        }
        else
        {
            SW.Restart();
        }

        Reset(grid);
        SoultionFinderFinished = false;
        SolutionCell = null;

        MinMoves = int.MaxValue;
        var cells = new Cell[width, height];
        _emptyCell.BoardElementType = EBoardElementType.EMPTY;
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                cells[i, j] = _emptyCell;
            }
        }

        var elementsData = jsonFile?.GetBaseElementsFlat();


        foreach (var boardElement in grid)
        {
            var pos = boardElement.GridPoint;

            switch (boardElement.BoardElementType)
            {
                case EBoardElementType.NONE:
                case EBoardElementType.EMPTY:
                    break;
                case EBoardElementType.TILE_LOCKED:
                case EBoardElementType.TILE:
                    cells[pos.x, pos.y] = CreateCellForTile(boardElement, elementsData);
                    break;
                case EBoardElementType.SAW:
                    cells[pos.x, pos.y] = new Cell { BoardElementType = EBoardElementType.SAW };
                    break;
                case EBoardElementType.ROTATOR:
                    var cell = new Cell { BoardElementType = EBoardElementType.ROTATOR };


                    cells[pos.x, pos.y] = cell;
                    break;
            }
        }


        var thread = new Thread(StartCalc);
        thread.Start();
        _threads[grid] = thread;

        void StartCalc()
        {
            HasSolution = HasSoulution(width, height, cells, out var solution);
            if (HasSolution)
            {
                SolutionCell = grid.FirstOrDefault(c => c.GridPoint == solution);
            }

            SW.Stop();
            SoultionFinderFinished = true;
        }
    }

    private static Cell CreateCellForTile(GridCell boardElement, List<BaseElement> elementsData)
    {
        var direction = EMoveDirection.UP;
        var actionsToUnlock = 0;

        if (boardElement.BaseBoardElementController is TileElementController tile)
        {
            direction = EMoveDirectionExtensions.GetDirectionFromMoveVector(tile.MoveDirection);
            actionsToUnlock = tile.RemainingActionsForUnlock;
        }
        else
        {
            var tileData =
                elementsData?.FirstOrDefault(r => r.position.ToVectorInt() == boardElement.GridPoint && r is Tiles) as
                    Tiles;
            var lockData =
                elementsData?.FirstOrDefault(r => r.position.ToVectorInt() == boardElement.GridPoint && r is Locks) as
                    Locks;

            if (lockData != null)
            {
                actionsToUnlock = lockData.charges;
            }

            direction = (EMoveDirection)tileData.direction;
        }

        return new Cell
        {
            BoardElementType = EBoardElementType.TILE, Direction = (int)direction, Locks = actionsToUnlock
        };
    }

    private static bool HasSoulution(int width, int height, Cell[,] cells, out Vector2Int solution)
    {
        var minSteps = int.MaxValue;
        var posX = 0;
        var posY = 0;
        var found = false;
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var steps = Tap(width, height, cells, i, j, 0);
                if (steps < minSteps)
                {
                    posX = i;
                    posY = j;
                    found = true;
                    solution = new Vector2Int(posX, posY);
                    return found;
                }
            }
        }

        solution = new Vector2Int(posX, posY);
        return found;
    }

    private static bool HasTile(int width, int height, Cell[,] cells)
    {
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var cell = cells[i, j];
                if (cell.BoardElementType == EBoardElementType.TILE && cell.Locks < 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static int Tap(int width, int height, Cell[,] cells, int posX, int posY, int steps)
    {
        var cell = cells[posX, posY];
        switch (cell.BoardElementType)
        {
            case EBoardElementType.NONE:
            case EBoardElementType.EMPTY:
            case EBoardElementType.SAW:
                break;

            case EBoardElementType.ROTATOR:
                break;

            case EBoardElementType.TILE:
                if (cell.Locks > 0)
                {
                    return int.MaxValue;
                }

                int DestroyTile()
                {
                    if (steps == int.MaxValue)
                    {
                        return int.MaxValue;
                    }

                    var cell0 = cells[posX, posY];
                    cells[posX, posY].BoardElementType = EBoardElementType.EMPTY;
                    var steps1 = TapOnEveryOne(width, height, cells, posX, posY, ++steps);
                    cells[posX, posY] = cell0;
                    return steps1;
                }

                int MoveToTile(int prevX, int prevY)
                {
                    if (steps == int.MaxValue)
                    {
                        return int.MaxValue;
                    }

                    var cell0 = cells[posX, posY];
                    var cell1 = cells[prevX, prevY];
                    cells[posX, posY].BoardElementType = EBoardElementType.EMPTY;
                    cells[prevX, prevY] = cell;
                    var steps1 = TapOnEveryOne(width, height, cells, posX, posY, ++steps);
                    cells[posX, posY] = cell0;
                    cells[prevX, prevY] = cell1;
                    return steps1;
                }

                switch (cell.Direction)
                {
                    case 0:
                        if (posY == 0)
                        {
                            return DestroyTile();
                        }

                        for (var i = posY - 1; i >= 0; i--)
                        {
                            var cell1 = cells[posX, i];
                            switch (cell1.BoardElementType)
                            {
                                case EBoardElementType.NONE:
                                case EBoardElementType.EMPTY:
                                    break;

                                case EBoardElementType.TILE:
                                case EBoardElementType.ROTATOR:
                                    if (i == posY - 1)
                                    {
                                        return int.MaxValue;
                                    }

                                    return MoveToTile(posX, i + 1);

                                case EBoardElementType.SAW:
                                    return DestroyTile();
                            }
                        }

                        return DestroyTile();

                    case 1:
                        if (posX == 0)
                        {
                            return DestroyTile();
                        }

                        for (var i = posX - 1; i >= 0; i--)
                        {
                            var cell1 = cells[i, posY];
                            switch (cell1.BoardElementType)
                            {
                                case EBoardElementType.NONE:
                                case EBoardElementType.EMPTY:
                                    break;

                                case EBoardElementType.TILE:
                                case EBoardElementType.ROTATOR:
                                    if (i == posX - 1)
                                    {
                                        return int.MaxValue;
                                    }

                                    return MoveToTile(i + 1, posY);

                                case EBoardElementType.SAW:
                                    return DestroyTile();
                            }
                        }

                        return DestroyTile();

                    case 2:
                        if (posY + 1 == height)
                        {
                            return DestroyTile();
                        }

                        for (var i = posY + 1; i < height; i++)
                        {
                            var cell1 = cells[posX, i];
                            switch (cell1.BoardElementType)
                            {
                                case EBoardElementType.NONE:
                                case EBoardElementType.EMPTY:
                                    break;

                                case EBoardElementType.TILE:
                                case EBoardElementType.ROTATOR:
                                    if (i == posY + 1)
                                    {
                                        return int.MaxValue;
                                    }

                                    return MoveToTile(posX, i - 1);

                                case EBoardElementType.SAW:
                                    return DestroyTile();
                            }
                        }

                        return DestroyTile();

                    case 3:
                        if (posX + 1 == width)
                        {
                            return DestroyTile();
                        }

                        for (var i = posX + 1; i < width; i++)
                        {
                            var cell1 = cells[i, posY];
                            switch (cell1.BoardElementType)
                            {
                                case EBoardElementType.NONE:
                                case EBoardElementType.EMPTY:
                                    break;

                                case EBoardElementType.TILE:
                                case EBoardElementType.ROTATOR:
                                    if (i == posX + 1)
                                    {
                                        return int.MaxValue;
                                    }

                                    return MoveToTile(i - 1, posY);

                                case EBoardElementType.SAW:
                                    return DestroyTile();
                            }
                        }

                        return DestroyTile();
                }

                break;
        }

        return int.MaxValue;

        int TapOnEveryOne(int width, int height, Cell[,] cells, int posX, int posY, int steps)
        {
            void AddLocks(int number)
            {
                for (var i = 0; i < width; i++)
                {
                    for (var j = 0; j < height; j++)
                    {
                        cells[i, j].Locks += number;
                    }
                }
            }

            void IncLocks()
            {
                AddLocks(1);
            }

            void DecLocks()
            {
                AddLocks(-1);
            }

            if (!IsStepsNotMuch(steps))
            {
                return int.MaxValue;
            }

            DecLocks();
            if (!HasTile(width, height, cells))
            {
                IncLocks();
                return steps;
            }

            var minSteps = int.MaxValue;
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    if (i == posX && j == posY)
                    {
                        continue;
                    }

                    var steps1 = Tap(width, height, cells, i, j, steps);
                    if (steps1 < minSteps)
                    {
                        minSteps = steps1;
                        MinMoves = minSteps;

                        IncLocks();
                        return MinMoves;
                    }
                }
            }

            IncLocks();
            return minSteps;
        }
    }

    private static bool IsStepsNotMuch(int steps)
    {
        return steps < MinMoves;
    }

    struct Cell
    {
        public EBoardElementType BoardElementType;
        public int Direction;

        public int Locks;
    }
}
