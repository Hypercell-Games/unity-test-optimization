using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelStage
{
    [SerializeField] private List<LevelFloorEntry> _jsonPaths;

    public string JsonPath => _jsonPaths[0].JsonPath;

    public void SetFloors(List<LevelFloorEntry> levelFloorEntries)
    {
        _jsonPaths = levelFloorEntries;
    }
}
