using System;
using static LevelJsonConverter;

public class LevelWallsController : BaseLevelElementsController
{
    public override void InitializeController(JsonFile jsonFile,
        Action<BaseBoardElementController> onInstantiateElement)
    {
        var levelWalls = jsonFile.walls;
        foreach (var levelSaw in levelWalls)
        {
            var boardElement = InstantiateBoardElement(levelSaw.position);
            onInstantiateElement?.Invoke(boardElement);
        }
    }
}
