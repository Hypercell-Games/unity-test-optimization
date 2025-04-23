using UnityEngine;

public enum EMoveDirection
{
    UP = 0,
    LEFT = 1,
    DOWN = 2,
    RIGHT = 3
}

public static class EMoveDirectionExtensions
{
    public static Vector2Int GetDirection(this EMoveDirection moveDirection)
    {
        return moveDirection switch
        {
            EMoveDirection.UP => Vector2Int.down,
            EMoveDirection.LEFT => Vector2Int.left,
            EMoveDirection.DOWN => Vector2Int.up,
            EMoveDirection.RIGHT => Vector2Int.right,
            _ => default
        };
    }

    public static Vector2Int GetMoveVector(this EMoveDirection moveDirection)
    {
        return moveDirection switch
        {
            EMoveDirection.UP => Vector2Int.up,
            EMoveDirection.LEFT => Vector2Int.left,
            EMoveDirection.DOWN => Vector2Int.down,
            EMoveDirection.RIGHT => Vector2Int.right,
            _ => default
        };
    }

    public static EMoveDirection GetDirectionFromMoveVector(Vector2Int moveVector)
    {
        if (moveVector == Vector2Int.up)
        {
            return EMoveDirection.UP;
        }

        if (moveVector == Vector2Int.down)
        {
            return EMoveDirection.DOWN;
        }

        if (moveVector == Vector2Int.left)
        {
            return EMoveDirection.LEFT;
        }

        if (moveVector == Vector2Int.right)
        {
            return EMoveDirection.RIGHT;
        }

        return EMoveDirection.UP;
    }
}
