using System;
using System.Collections.Generic;
using System.Linq;
using static LevelJsonConverter;

public class LevelRotatorsController : BaseLevelElementsController
{
    private readonly Dictionary<int[][], Position> _handsDictionary = new();

    public override void InitializeController(JsonFile jsonFile,
        Action<BaseBoardElementController> onInstantiateElement)
    {
        var levelRotators = jsonFile.rotators;
        FillHandsDictionary(levelRotators);
        InitializeHolders(onInstantiateElement);
    }

    private void InitializeHolders(Action<BaseBoardElementController> onInstantiateElement)
    {
        foreach (var handElement in _handsDictionary)
        {
            var element = InstantiateBoardElement(handElement.Value);
            var rotatorController = (RotatorElementController)element;
            for (var i = 0; i < handElement.Key.Length / 2; i++)
            {
                var holderController = rotatorController.InstantiateTurningTop();
                for (var j = 0; j < handElement.Key[i].Length; j += 2)
                {
                    holderController.Direction = handElement.Key[i][j];
                    holderController.Length = handElement.Key[i][j + 1];
                }
            }

            rotatorController.FillHolders();
            onInstantiateElement?.Invoke(element);
        }
    }

    private void FillHandsDictionary(Rotators[] rotators)
    {
        if (rotators == null || rotators.Length == 0)
        {
            return;
        }

        foreach (var rotator in rotators)
        {
            var hands = rotator.hands;
            var b = new int[rotator.hands.Length][];

            for (int i = 0, c = 0; i < hands.Length; i += 2, c++)
            {
                var hand = new int[2];

                hand[0] = hands[i];
                hand[1] = hands[i + 1];

                b[c] = hand;
            }

            _handsDictionary.Add(b, rotator.position);
        }
    }

    public static bool TryToRotate(GridController grid, GridCell rotator)
    {
        if (!(rotator.BaseBoardElementController is RotatorElementController rotatorElement))
        {
            return false;
        }

        if (grid.GetElementControllersOfType<RotatorElementController>().Any(r => r.IsRotating))
        {
            return false;
        }

        return rotatorElement.TryToRotate(grid._boardElements);
    }
}
