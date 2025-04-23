using System;
using System.Collections.Generic;
using UnityEngine;
using static LevelJsonConverter;

[CreateAssetMenu(fileName = "LevelItem", menuName = MenuPath, order = MenuOrder)]
public class LevelItemConfigs : ScriptableObject, ILevelItemConfigs
{
    private const string MenuPath = "Configs/LevelItemConfigs";
    private const int MenuOrder = int.MinValue + 121;

    [SerializeField] private BaseLevelItem _levelPrefab;

    [SerializeField] private LevelStage[] _levelStages;

    public BaseLevelItem LevelPrefab => _levelPrefab;

    public LevelStage[] LevelStages => _levelStages;

    public string Validate()
    {
        var path0 = $"->{name}\n-->";

        var errorMessage = _levelPrefab == null ? $"{path0}_levelPrefab is NULL" : null;
        errorMessage ??= _levelStages == null ? $"{path0}_levelStages is NULL" : null;
        errorMessage ??= _levelStages.Length == 0 ? $"{path0}_levelStages is EMPTY" : null;

        if (errorMessage != null)
        {
            return errorMessage;
        }

        for (var i = 0; i < _levelStages.Length; i++)
        {
            var stage = _levelStages[i];
            var stageJsonPath = stage.JsonPath;
            var path = $"{path0}{stageJsonPath}\n--->";
            var textAsset = Resources.Load<TextAsset>(stageJsonPath);

            errorMessage ??= textAsset == null ? $"{path}stageJsonPath is MISSING" : null;
            if (errorMessage != null)
            {
                return errorMessage;
            }

            var json = textAsset.text;
            JsonFile jsonFile;

            try
            {
                jsonFile = JsonUtility.FromJson<JsonFile>(json);
                jsonFile.name = stage.JsonPath;
            }
            catch (Exception)
            {
                return $"{path}JsonFile is INCORRECT.";
            }

            var size = jsonFile.size;
            var tiles = jsonFile.tiles;

            errorMessage ??= ValidateTiles(tiles, path, size);
            errorMessage ??= ValidateElementsPositions(jsonFile.saws, "saw", size, path);
            errorMessage ??= ValidateElementsPositions(jsonFile.rotators, "rotator", size, path);
            errorMessage ??= ValidateElementsPositions(jsonFile.locks, "lock", size, path);
            errorMessage ??= ValidateElementsPositions(jsonFile.walls, "wall", size, path);
            errorMessage ??= ValidateElementsPositions(jsonFile.bombs, "bomb", size, path);
            errorMessage ??= ValidateRotators(jsonFile, size, path);

            if (errorMessage != null)
            {
                return errorMessage;
            }
        }

        return string.Empty;
    }

    private string ValidateTiles(IReadOnlyList<Tiles> tiles, string path, Size size)
    {
        if (tiles == null)
        {
            return $"{path}Tiles is NULL.";
        }

        if (tiles.Count == 0)
        {
            return $"{path}Tiles is EMPTY.";
        }

        foreach (var tile in tiles)
        {
            var errorMessage = ValidateElementPositions(tile, "tile", size, path);
            if (errorMessage != null)
            {
                return errorMessage;
            }

            if (tile.direction < 0 || tile.direction > 3)
            {
                return $"{path}Wrong tile direction {tile.direction}.";
            }
        }

        return null;
    }

    private string ValidateElementsPositions(IReadOnlyList<BaseElement> elements, string elementName, Size size,
        string path)
    {
        if (elements == null)
        {
            return null;
        }

        for (var j = 0; j < elements.Count; j++)
        {
            var position = elements[j].position;
            if (!IsCorrectPosition(position, size))
            {
                return $"{path}Wrong {elementName} position {position} in board size {size}.";
            }
        }

        return null;
    }

    private string ValidateElementPositions(BaseElement element, string elementName, Size size, string path)
    {
        var position = element.position;
        if (!IsCorrectPosition(position, size))
        {
            return $"{path}Wrong {elementName} position {position} in board size {size}.";
        }

        return null;
    }

    private string ValidateRotators(JsonFile jsonFile, Size size, string path)
    {
        if (jsonFile.rotators == null)
        {
            return null;
        }

        for (var j = 0; j < jsonFile.rotators.Length; j++)
        {
            var rotator = jsonFile.rotators[j];

            var errorMessage = ValidateElementPositions(rotator, "rotator", size, path);
            if (errorMessage != null)
            {
                return errorMessage;
            }

            if (rotator.hands.Length % 2 != 0)
            {
                return $"{path}Wrong rotators hands array {rotator.hands.Length}.";
            }

            for (var k = 0; k < rotator.hands.Length; k += 2)
            {
                var dir = rotator.hands[k];
                if (dir < 0 || dir > 3)
                {
                    return $"{path}Wrong rotators hand direction {dir}.";
                }

                var length = rotator.hands[k + 1];
                if (length < 0)
                {
                    return $"{path}Wrong rotators hand length {length}.";
                }
            }
        }

        return null;
    }

    private bool IsCorrectPosition(Position position, Size size)
    {
        return position.x + 1 > 0 && position.x < size.width && position.y + 1 > 0 && position.y < size.height;
    }
}
