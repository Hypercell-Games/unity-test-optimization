using System;

public interface ILevelItem
{
    event Action<ELevelCompleteReason, LevelProgress> onLevelCompleted;
}
