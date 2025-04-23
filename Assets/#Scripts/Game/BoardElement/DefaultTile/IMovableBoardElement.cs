using System;
using UnityEngine;

public interface IMovableBoardElement
{
    void MoveToPosition(Vector2 worldPosition, Action callback = null);
}
