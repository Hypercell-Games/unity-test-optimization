using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class FailStateController
{
    private readonly GameBoardController _gameBoard;
    private Coroutine _runningFailCheckCo;

    public Action<bool> OnFailed;

    public FailStateController(GameBoardController gameBoard)
    {
        _gameBoard = gameBoard;
    }

    public void TryScheduleFailCheck(GridController gridController)
    {
        if (_runningFailCheckCo != null)
        {
            return;
        }

        _runningFailCheckCo = _gameBoard.StartCoroutine(Co_FailCheck(gridController));
    }

    private IEnumerator Co_FailCheck(GridController gridController)
    {
        var rotators = gridController.GetElementControllersOfType<RotatorElementController>();

        yield return new WaitWhile(() => rotators.Any(r => r.IsRotating));

        if (!gridController.CanDoAnyMove())
        {
            OnFailed?.Invoke(false);
        }
        else
        {
            FailCheckMovesOnly();
        }

        _runningFailCheckCo = null;
    }

    public void FailCheckMovesOnly()
    {
        if (_gameBoard.MovesLeft <= 0)
        {
            OnFailed?.Invoke(true);
        }
    }
}
