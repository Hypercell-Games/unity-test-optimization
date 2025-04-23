using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LevelJsonConverter
{
    public static JsonFile[] JSONDeserializer(LevelStage[] levelStages)
    {
        var maxSize = Vector2Int.zero;
        var stages = new JsonFile[levelStages.Length];
        for (var i = 0; i < levelStages.Length; i++)
        {
            var levelStage = levelStages[i];
            var jsonPath = levelStage.JsonPath;
            var jsonString = Resources.Load<TextAsset>(jsonPath).text;
            var jsonFile = JsonUtility.FromJson<JsonFile>(jsonString);
            jsonFile.name = levelStage.JsonPath;
            stages[i] = jsonFile;
            maxSize.x = Math.Max(maxSize.x, jsonFile.size.width);
            maxSize.y = Math.Max(maxSize.y, jsonFile.size.height);
        }

        for (var i = 0; i < stages.Length; i++)
        {
            var stage = stages[i];
            var stageOffset = new Vector2(maxSize.x - stage.size.width, -(maxSize.y - stage.size.height)) * 0.5f;
            stage.size.width = maxSize.x;
            stage.size.height = maxSize.y;
            if (GameConfig.RemoteConfig.stagesSystemVersion < 1)
            {
                stageOffset += Vector2.up * (0.06f * (stages.Length - i - 1));
            }

            stage.offset = stageOffset;
        }

        return stages;
    }

    public static EBoardElementType? GetElementTypeForBaseElement(BaseElement baseElement)
    {
        return baseElement switch
        {
            Bombs _ => EBoardElementType.BOMB,
            Colors _ => null,
            Locks _ => EBoardElementType.TILE_LOCKED,
            Rotators _ => EBoardElementType.ROTATOR,
            Saws _ => EBoardElementType.BOMB,
            Tiles _ => EBoardElementType.TILE,
            Walls _ => EBoardElementType.WALL,
            _ => throw new ArgumentOutOfRangeException(nameof(baseElement))
        };
    }

    [Serializable]
    public class JsonFile
    {
        public Size size;
        public Solution solution;
        public Tiles[] tiles;
        public Saws[] saws;
        public Walls[] walls = Array.Empty<Walls>();
        public Bombs[] bombs = Array.Empty<Bombs>();
        public Locks[] locks;
        public Rotators[] rotators = Array.Empty<Rotators>();
        public Colors[] colors;
        public Vector2 offset;
        public int moves;
        [NonSerialized] public string name;

        public List<BaseElement[]> GetBaseElements()
        {
            return new List<BaseElement[]>
            {
                tiles,
                saws,
                walls,
                bombs,
                rotators,
                colors,
                locks
            };
        }

        public List<BaseElement> GetBaseElementsFlat()
        {
            return GetBaseElements().Where(r => r != null)
                .SelectMany(r => r).ToList();
        }
    }
}

[Serializable]
public class Size
{
    public int width;
    public int height;

    public override string ToString()
    {
        return $"({width},{height})";
    }
}

[Serializable]
public class Solution
{
    public Position[] steps;
}

[Serializable]
public class Tiles : BaseElement
{
    public int direction;
}

[Serializable]
public class Saws : BaseElement
{
}

[Serializable]
public class Walls : BaseElement
{
}

[Serializable]
public class Bombs : BaseElement
{
}

[Serializable]
public class Locks : BaseElement
{
    public int charges;
}

[Serializable]
public class Rotators : BaseElement
{
    public int[] hands;
    public int direction;
}

[Serializable]
public class Colors : BaseElement
{
    public string tileColor;
}

public class BaseElement
{
    public Position position;
}

[Serializable]
public class Position
{
    public int x;
    public int y;

    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }

    public Vector2Int ToVectorInt()
    {
        return new Vector2Int(x, y);
    }

    public override string ToString()
    {
        return $"({x},{y})";
    }
}
