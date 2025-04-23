using System;
using System.Linq;
using UnityEngine;
using static LevelJsonConverter;

public class LevelTilesController : BaseLevelElementsController
{
    public override void InitializeController(JsonFile jsonFile,
        Action<BaseBoardElementController> onInstantiateElement)
    {
        var levelTiles = jsonFile.tiles;
        var levelLocks = jsonFile.locks;
        foreach (var levelTile in levelTiles)
        {
            var boardElement = InstantiateBoardElement(levelTile.position);
            if (IsLockElement(levelLocks, levelTile))
            {
                boardElement.BoardElementType = EBoardElementType.TILE_LOCKED;
            }

            onInstantiateElement?.Invoke(boardElement);
        }
    }

    private bool IsLockElement(Locks[] locks, Tiles tile)
    {
        return locks.Any(lockElement => new Vector2(lockElement.position.x, lockElement.position.y) ==
                                        new Vector2(tile.position.x, tile.position.y));
    }
}
