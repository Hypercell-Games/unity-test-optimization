using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class MaxMovesConfig
{
    public bool enabled = true;

    public List<MaxMovesBoardEntry> boards = new();

    public MaxMovesBoardEntry FindBoardEntry(string name)
    {
        return boards.FirstOrDefault(r => r.name.Equals(name));
    }
}

[Serializable]
public class MaxMovesBoardEntry
{
    public string name;
    public int moves;
}
