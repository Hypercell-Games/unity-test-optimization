using System;
using static LevelJsonConverter;

public class LevelSawsController : BaseLevelElementsController
{
    public override void InitializeController(JsonFile jsonFile,
        Action<BaseBoardElementController> onInstantiateElement)
    {
        var levelSaws = jsonFile.saws;
        foreach (var levelSaw in levelSaws)
        {
            var boardElement = InstantiateBoardElement(levelSaw.position);
            onInstantiateElement?.Invoke(boardElement);
        }
    }
}
