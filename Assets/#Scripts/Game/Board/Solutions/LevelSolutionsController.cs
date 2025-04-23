using UnityEngine;
using static LevelJsonConverter;

public class LevelSolutionsController : MonoBehaviour
{
    private Solution levelSolution;

    public Vector2Int GetFirstElementSolution()
    {
        var firstStep = levelSolution.steps[0];
        return firstStep.ToVectorInt();
    }

    public void InitializeController(JsonFile jsonConverter)
    {
        levelSolution = jsonConverter.solution;
    }
}
