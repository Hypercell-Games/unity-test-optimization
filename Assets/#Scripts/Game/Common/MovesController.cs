using System;
using UnityEngine;

public class MovesController
{
    public int MovesLeft { get; private set; }

    public event Action<int, bool> OnMovesLeftChanged;

    public bool HasMoves()
    {
        return MovesLeft > 0;
    }

    public void DecreaseMoves()
    {
        MovesLeft--;

        OnMovesLeftChanged?.Invoke(MovesLeft, false);
    }

    public void AddMoves(int count)
    {
        MovesLeft += count;

        OnMovesLeftChanged?.Invoke(MovesLeft, true);
    }

    public void SetMoves(int movesLeft)
    {
        MovesLeft = movesLeft;

        OnMovesLeftChanged?.Invoke(MovesLeft, false);
    }
}

public class TimeController
{
    private float _countTimeOriginal;

    public float TimeLeft { get; private set; }

    public event Action<int, bool> OnTimeLeftChanged;

    public bool HasTime()
    {
        return TimeLeft > 0f;
    }

    public void DecreaseTime(float count)
    {
        TimeLeft -= count;
        TimeLeft = Mathf.Max(0, TimeLeft);
        OnTimeLeftChanged?.Invoke((int)TimeLeft, false);
    }

    public void AddTime(float count)
    {
        TimeLeft += count;
        TimeLeft = Mathf.Max(0, TimeLeft);
        OnTimeLeftChanged?.Invoke((int)TimeLeft, true);
    }

    public void SetTime(float timeLeft, int original)
    {
        TimeLeft = timeLeft;
        TimeLeft = Mathf.Max(0, TimeLeft);
        _countTimeOriginal = original;
        _countTimeOriginal = Mathf.Max(0, _countTimeOriginal);
        OnTimeLeftChanged?.Invoke((int)TimeLeft, false);
    }
}

public class TargetController
{
    public int TargetLeft { get; private set; }

    public event Action<int> OnTargetChange;

    public bool HasMoves()
    {
        return TargetLeft > 0;
    }

    public void DecreaseMoves()
    {
        TargetLeft--;

        OnTargetChange?.Invoke(TargetLeft);
    }

    public void AddMoves(int count)
    {
        TargetLeft += count;

        OnTargetChange?.Invoke(TargetLeft);
    }

    public void SetMoves(int movesLeft)
    {
        TargetLeft = movesLeft;

        OnTargetChange?.Invoke(TargetLeft);
    }
}
