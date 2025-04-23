using System.Collections;
using System.Collections.Generic;

public class Grid : IEnumerable<GridCell>
{
    private readonly GridCell[,] _grid;

    public Grid(int gridWidth, int gridHeight)
    {
        Width = gridWidth;
        Height = gridHeight;

        _grid = new GridCell[Width, Height];
    }

    public int Width { get; }

    public int Height { get; }

    public GridCell this[int x, int y]
    {
        get => _grid[x, y];
        set => _grid[x, y] = value;
    }

    public IEnumerator<GridCell> GetEnumerator()
    {
        return _grid.GetEnumerator().Cast<GridCell>();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _grid.GetEnumerator();
    }

    public bool IsWithinGridBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public static List<GridCell> CellsInLine(int x, int y, Grid grid, EMoveDirection eMoveDirection)
    {
        var dir = eMoveDirection.GetDirection();
        var targetX = x + dir.x;
        var targetY = y + dir.y;

        var cellsInLine = new List<GridCell>();

        while (grid.IsWithinGridBounds(targetX, targetY))
        {
            cellsInLine.Add(grid[targetX, targetY]);

            targetX += dir.x;
            targetY += dir.y;
        }

        ;

        return cellsInLine;
    }
}
