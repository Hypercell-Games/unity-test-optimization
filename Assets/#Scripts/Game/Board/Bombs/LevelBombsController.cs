using System;
using static LevelJsonConverter;

public class LevelBombsController : BaseLevelElementsController
{
    public override void InitializeController(JsonFile jsonFile,
        Action<BaseBoardElementController> onInstantiateElement)
    {
        var levelBombs = jsonFile.bombs;
        foreach (var levelSaw in levelBombs)
        {
            var boardElement = InstantiateBoardElement(levelSaw.position);
            onInstantiateElement?.Invoke(boardElement);
        }
    }
}
