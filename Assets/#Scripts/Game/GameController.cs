using System.Collections;
using UnityEngine;
using Zenject;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameData _gameData;
    [Inject] UIGameScreen _gameScreen;
    [Inject] LevelsController _levelsController;

    private void Start()
    {
        _levelsController.CreateLevel(true);
    }

    [Inject]
    protected virtual void Initialize(LevelsController levelsController)
    {
        levelsController.onNewLevelStart += OnLevelCreate;
        levelsController.onKeyFound += OnKeyFound;
    }

    private void OnKeyFound(Transform key)
    {
        StartCoroutine(CO_KeyFound(key));
    }

    private IEnumerator CO_KeyFound(Transform key)
    {
        yield return _gameScreen.TweenCollectKeyFrom(key);

        _gameData.KeysAmount.ApplyChange(1);
    }

    private void OnLevelCreate()
    {
        _gameScreen.Show();
    }
}
