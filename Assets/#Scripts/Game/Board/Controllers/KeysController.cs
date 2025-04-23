using System.Linq;

public class KeysController
{
    private readonly int _levelNum;
    private readonly int _targetStage;

    public KeysController(int levelNum, int targetStage)
    {
        _levelNum = levelNum;
        _targetStage = targetStage;
    }

    public void SetupRandomKeyIfShould(int stageNumber, GridController gridController)
    {
        if (stageNumber == _targetStage)
        {
            SetupRandomKey(gridController);
        }
    }

    public void SetupRandomKey(GridController gridController)
    {
        var tileElements = gridController.GetGridCellsOfType<TileElementController>();
        tileElements.Shuffle();
        var randomTile = tileElements.FirstOrDefault();

        if (!(randomTile?.BaseBoardElementController is TileElementController tile))
        {
            return;
        }

        tile.SetHasKey(true);
    }
}
