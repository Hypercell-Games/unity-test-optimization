using System;
using UnityEngine;
using static LevelJsonConverter;

public abstract class BaseLevelElementsController : MonoBehaviour
{
    [SerializeField] protected BaseBoardElementController _boardElementController;

    public abstract void InitializeController(JsonFile jsonFile,
        Action<BaseBoardElementController> onInstantiateElement);

    protected virtual BaseBoardElementController InstantiateBoardElement(Position position)
    {
        var boardElement = Instantiate(_boardElementController, transform);
        boardElement.GridPosition = position.ToVectorInt();
        boardElement.InitializeBoardElementInfo();
        boardElement.name = $"TileBoardElement ({boardElement.GridPosition.x},{boardElement.GridPosition.y})";
        return boardElement;
    }
}
