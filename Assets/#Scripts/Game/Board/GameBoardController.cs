using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Unpuzzle.Game;
using static LevelJsonConverter;

public class GameBoardController : MonoBehaviour
{
    [SerializeField] private GameData _gameData;
    [SerializeField] private List<BaseLevelElementsController> _levelElementsController = new();
    [SerializeField] private LevelSolutionsController _levelSolutionController;
    private readonly MovesController _movesController = new();

    private readonly List<TileElementController> _tilesInMoveToDestroy = new();
    private CameraManager _cameraManager;
    private GridCell _clickedElement;
    private FailStateController _failStateController;
    private Coroutine _finder;

    private GameBoardInteraction _gameBoardInteraction;
    private Camera _gameCamera;

    private GridController _gridController;

    private BaseBoardElementController _hintCell;

    private bool _isStageCompleted;
    private bool _isStageStarted;
    private JsonFile _jsonFile;
    private KeysController _keysController;
    private int _levelNum;

    private int _levelTilesCount;
    private Coroutine _logger;
    private int _moveCount;
    private Action<Vector2> _onGetStageSolution;
    private Action<TileElementController> _onKeyFound;

    private Action<ELevelCompleteReason> _onStageCompleted;
    private Vector2 _pointerDownPosition = Vector2.negativeInfinity;

    private IClickableElement _pressedTile;
    private int _rightClickRage;
    private int _stageNum;

    private bool isReverseTileSound;

    public Action<int, bool> OnMovesLeftChanged;

    private List<GridCell> BoardElements => _gridController._boardElements;

    public int MovesLeft => _movesController.MovesLeft;

    private void Awake()
    {
        InitializeData();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        UnSubscribe();
    }

    private void OnDestroy()
    {
        if (_gridController == null)
        {
            return;
        }

        BoardSolutionFinder.Reset(_gridController.Grid);
    }

    public void InitializeBoard(int levelNum, int stageNum, int sortOrder, JsonFile jsonFile,
        Action<ELevelCompleteReason> onStageCompleted, Action<Vector2> onGetStageSolution, int targetKeyStage,
        Action<TileElementController> onKeyFound)
    {
        _levelNum = levelNum;
        _stageNum = stageNum;
        _onStageCompleted = onStageCompleted;
        _onGetStageSolution = onGetStageSolution;
        _onKeyFound = onKeyFound;
        _moveCount = 0;
        _gameCamera = CameraManager.Instance.GetCameraItem(ECameraType.GAME).Camera;
        _jsonFile = jsonFile;
        _keysController = new KeysController(_levelNum, targetKeyStage);
        _failStateController = new FailStateController(this);
        _failStateController.OnFailed += OnStageFailed;

        SetBgColor();
        FillGrid(_jsonFile, sortOrder);
    }

    private void SetBgColor()
    {
        if (!ColorUtility.TryParseHtmlString(GameConfig.RemoteConfig.backgroundColor, out var bgColor))
        {
            return;
        }

        _gameCamera.backgroundColor = bgColor;
    }

    private void FillGrid(JsonFile jsonFile, int sortOrder)
    {
        var gameCamera = _cameraManager.GetCameraItem(ECameraType.GAME).Camera;

        _gridController = new GridController(jsonFile.size.width, jsonFile.size.height);
        _gridController.CreateGridElements(gameCamera, sortOrder, jsonFile.offset.x, jsonFile.offset.y);

        foreach (var levelElementsController in _levelElementsController)
        {
            levelElementsController.InitializeController(jsonFile, OnInstantiateElement);
        }

        _levelSolutionController.InitializeController(jsonFile);
        _keysController.SetupRandomKeyIfShould(_stageNum, _gridController);

        _movesController.OnMovesLeftChanged += (r, t) => OnMovesLeftChanged?.Invoke(r, t);
        _movesController.SetMoves(GameLogicUtil.GetMovesForLevel(jsonFile));
    }

    public void AddMoves(int movesCount)
    {
        _movesController.AddMoves(movesCount);
    }

    public void SetStage(int stage, bool fast)
    {
        _gridController.SetBoardElementsStage(stage, fast);

        AnimateStageIntroduction(stage, fast);
    }

    private void AnimateStageIntroduction(int stage, bool fast)
    {
        if (GameConfig.RemoteConfig.stagesSystemVersion <= 0)
        {
            return;
        }

        DOTween.Kill(gameObject);

        if (fast)
        {
            transform.localPosition = 6f * stage * Vector3.right;
        }
        else
        {
            transform.DOLocalMoveX(6f * stage, 1f).SetEase(Ease.OutQuart);
        }
    }

    public void StartStage()
    {
        _isStageStarted = true;

        var worldPoint = _gridController.GetCell(_levelSolutionController.GetFirstElementSolution()).WorldPoint;
        _onGetStageSolution?.Invoke(worldPoint);

        FindSolution();
    }

    private void Subscribe()
    {
        _gameBoardInteraction.onPointerDown += OnPointerDown;
        _gameBoardInteraction.onPointerUp += OnPointerUp;
        _gameBoardInteraction.onPointerDrag += OnPointerDrag;
        _gameData.SelectedSkin.onChange.action += UpdateCurrentSkin;
    }

    private void UnSubscribe()
    {
        _gameBoardInteraction.onPointerDown -= OnPointerDown;
        _gameBoardInteraction.onPointerUp -= OnPointerUp;
        _gameBoardInteraction.onPointerDrag -= OnPointerDrag;
        _gameData.SelectedSkin.onChange.action -= UpdateCurrentSkin;
    }

    public int GetMoveCount()
    {
        return _moveCount;
    }

    private bool TryToGetMovedPosition(GridCell clickedElement, Action<GridCell, Action> callback = null)
    {
        var clickedTile = (TileElementController)clickedElement.BaseBoardElementController;
        var boardElement = clickedElement.BaseBoardElementController;
        var movedPoint = clickedElement;

        var lineElements = _gridController.GetElementsInDirectionLine(clickedElement);
        var previousElement = lineElements.FirstOrDefault();

        foreach (var element in lineElements)
        {
            if (element.BoardElementType == EBoardElementType.EMPTY)
            {
                movedPoint = element;
            }
            else if (element.BoardElementType == EBoardElementType.SAW)
            {
                OnCompletedActionOnBoard();
                movedPoint = element;

                _rightClickRage++;
                if (previousElement.BaseBoardElementController == element.BaseBoardElementController)
                {
                    clickedTile.DoActionWithTile(EActionWithElement.DESTROY);
                    _gridController.ClearTile(clickedElement);
                    DestroyTile(clickedTile, ETileDestroyType.SAW);
                }
                else
                {
                    _gridController.ClearTile(clickedElement);
                    Vector3 movedPosition = (previousElement.WorldPoint + movedPoint.WorldPoint) * 0.5f;

                    _tilesInMoveToDestroy.Add(clickedTile);
                    clickedTile.MoveToPosition(movedPosition, () =>
                    {
                        _tilesInMoveToDestroy.Remove(clickedTile);
                        clickedTile.DoActionWithTile(EActionWithElement.MOVE);
                        DestroyTile(clickedTile, ETileDestroyType.SAW);
                    });
                }

                FindSolution();
                return true;
            }
            else if (element.BoardElementType == EBoardElementType.BOMB)
            {
                OnCompletedActionOnBoard();
                movedPoint = element;

                _rightClickRage++;
                var elementController = element.BaseBoardElementController;
                if (previousElement.BaseBoardElementController == element.BaseBoardElementController)
                {
                    elementController.DoActionWithTile(EActionWithElement.DESTROY);
                    _gridController.ClearTile(element);
                    DestroyTile(elementController, ETileDestroyType.NONE);
                }
                else
                {
                    Vector3 movedPosition = (previousElement.WorldPoint + movedPoint.WorldPoint) * 0.5f;

                    _tilesInMoveToDestroy.Add(clickedTile);
                    clickedTile.DoActionWithTile(EActionWithElement.MOVE);
                    clickedTile.MoveToPosition(movedPosition, () =>
                    {
                        _tilesInMoveToDestroy.Remove(clickedTile);
                        DestroyTile(elementController, ETileDestroyType.NONE);
                    });

                    _gridController.MoveCellContents(clickedElement, previousElement);
                    _gridController.ClearTile(element);
                }

                FindSolution();
                return true;
            }
            else
            {
                if (previousElement.BoardElementType != EBoardElementType.EMPTY &&
                    previousElement.BoardElementType != EBoardElementType.NONE)
                {
                    if (GameConfig.RemoteConfig.wrongClickHapticVersion <= 0)
                    {
                        VibrationController.Instance.Play(EVibrationType.MediumImpact);
                    }
                    else
                    {
                        VibrationController.Instance.Play(EVibrationType.Failure);
                    }

                    _rightClickRage = 0;

                    previousElement.BaseBoardElementController.OnWrongClick();
                    if (GameConfig.RemoteConfig.wrongClickAnimationVersion < 2)
                    {
                        clickedTile.ShakeTile();
                    }

                    void BounceElements()
                    {
                        if (GameConfig.RemoteConfig.wrongClickAnimationVersion != 2)
                        {
                            return;
                        }

                        var clickedCell = BoardElements.FirstOrDefault(c =>
                            c.BaseBoardElementController as TileElementController == clickedTile);
                        if (clickedCell == null)
                        {
                            return;
                        }

                        var elementsInLine = _gridController.GetElementsInDirectionLine(clickedCell);
                        elementsInLine.Insert(0, clickedCell);
                        BounceLineElements(elementsInLine, clickedTile.MoveDirection);
                    }

                    BounceElements();
                }

                callback?.Invoke(movedPoint, null);
                return true;
            }

            previousElement = element;
        }

        return false;
    }

    private void BounceLineElements(List<GridCell> grids, Vector2 moveDirection)
    {
        var gameConfig = GameConfig.RemoteConfig;
        var power = gameConfig.tileBouncePower;
        var delayTime = gameConfig.tileBounceDelay;
        var anyTileFound = false;

        for (var i = 0; i < grids.Count; i++)
        {
            var gridCell = grids[i];
            if (gridCell.BoardElementType == EBoardElementType.EMPTY)
            {
                if (!anyTileFound)
                {
                    continue;
                }

                break;
            }

            if (!(gridCell.BaseBoardElementController is TileElementController tile))
            {
                continue;
            }

            var delay = delayTime * i;
            tile.DoBounce(moveDirection * _gridController.CellSize, power, delay);

            power *= gameConfig.tileBounceStep;
            anyTileFound = true;
        }
    }

    private void OnPointerDown(Vector2 worldInputPosition)
    {
        if (!_isStageStarted || _isStageCompleted)
        {
            return;
        }

        if (_pressedTile != null)
        {
            CancelCurrentPress();
        }

        _clickedElement = _gridController.GetCellByWorldPosition(worldInputPosition);
        _pressedTile = GetTile(worldInputPosition);

        if (_clickedElement != null)
        {
            _pointerDownPosition = _clickedElement.WorldPoint;
        }
        else
        {
            _pressedTile = null;
            _pointerDownPosition = Vector2.negativeInfinity;
        }

        if (_pressedTile == null ||
            ((BaseBoardElementController)_pressedTile).BoardElementType == EBoardElementType.TILE_LOCKED)
        {
            _pressedTile = null;
            return;
        }

        if (GameConfig.RemoteConfig.tapOnTileHapticVersion > 0)
        {
            VibrationController.Instance.Play(EVibrationType.LightImpact);
        }

        _moveCount++;
        _pressedTile.OnClickElement(TileState.PRESSED);
    }

    private void OnPointerUp(Vector2 worldInputPosition)
    {
        if (!_isStageStarted || _isStageCompleted)
        {
            return;
        }

        if (!_movesController.HasMoves())
        {
            return;
        }

        if (_pressedTile == null)
        {
            return;
        }

        _pressedTile.OnClickElement(TileState.IDLE);
        _pressedTile = null;

        if (_clickedElement == null)
        {
            return;
        }

        if (Vector2.Distance(_pointerDownPosition, worldInputPosition) > _gridController.CellSize * 2f)
        {
            return;
        }

        _movesController.DecreaseMoves();

        var moveSucceeded = OnPointerClick(worldInputPosition);
        if (!moveSucceeded && _tilesInMoveToDestroy.Count == 0)
        {
            _failStateController.FailCheckMovesOnly();
        }
    }

    private bool OnPointerClick(Vector2 worldInputPosition)
    {
        var clickedElement = _clickedElement;

        if (clickedElement == null || clickedElement.BaseBoardElementController == null)
        {
            return false;
        }

        if (GameConfig.RemoteConfig.tapOnTileHapticVersion <= 0)
        {
            VibrationController.Instance.Play(EVibrationType.LightImpact);
        }

        return OnObjectClick(clickedElement);
    }

    private bool OnObjectClick(GridCell clickedElement)
    {
        return clickedElement.BoardElementType switch
        {
            EBoardElementType.TILE => OnClickTile(clickedElement),
            EBoardElementType.ROTATOR => OnClickRotator(clickedElement),
            EBoardElementType.TILE_LOCKED => OnClickLocker(clickedElement),
            EBoardElementType.SAW => false,
            EBoardElementType.EMPTY => false,
            EBoardElementType.NONE => false,
            EBoardElementType.WALL => false,
            EBoardElementType.BOMB => false,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void OnPointerDrag(Vector2 worldInputPosition)
    {
        if (!_isStageStarted || _isStageCompleted)
        {
            return;
        }

        if (_pressedTile == null)
        {
            return;
        }

        if (Vector2.Distance(_pointerDownPosition, worldInputPosition) > _gridController.CellSize * 2f)
        {
            CancelCurrentPress();
        }
    }

    private void CancelCurrentPress()
    {
        if (_pressedTile == null)
        {
            return;
        }

        _pressedTile.OnClickElement(TileState.IDLE);
        _pressedTile = null;
        _clickedElement = null;
    }

    private IClickableElement GetTile(Vector2 worldInputPosition)
    {
        var element = _gridController.GetCellByWorldPosition(worldInputPosition);
        var elementController = element?.BaseBoardElementController;
        if (!(elementController is IClickableElement clickableTile))
        {
            return null;
        }

        return clickableTile;
    }

    private bool OnClickLocker(GridCell clickedTile)
    {
        var tile = (TileElementController)clickedTile.BaseBoardElementController;
        tile.DoPunchScale();
        tile.EnableTileEffect(ETileEffect.JELLY);

        return false;
    }

    private void OnCompletedActionOnBoard()
    {
        UpdateActionsLockedCells();
    }

    private void UpdateActionsLockedCells()
    {
        foreach (var cell in _gridController.GetLockedCells())
        {
            ((TileElementController)cell.BaseBoardElementController).DecreaseRemainingActionsLock(cell);
        }
    }

    private bool OnClickTile(GridCell clickedTile)
    {
        if (clickedTile.BoardElementType != EBoardElementType.TILE)
        {
            return false;
        }

        if (_gridController.GetRotatorControlling(clickedTile)?.IsRotating ?? false)
        {
            return false;
        }

        var tile = (TileElementController)clickedTile.BaseBoardElementController;

        if (!TryToGetMovedPosition(clickedTile, OnGetPoint))
        {
            var tileGO = new GameObject();
            var tileElement = tileGO.AddComponent<TileElementController>();
            tileElement = (TileElementController)clickedTile.BaseBoardElementController;

            var targetPosition = GetOutGridPosition(clickedTile);
            var tvp = _gameCamera.WorldToViewportPoint(targetPosition);
            const float dist = 3f;
            if (tvp.x < 0f)
            {
                targetPosition.x -= dist;
            }
            else if (tvp.x > 1f)
            {
                targetPosition.x += dist;
            }
            else if (tvp.y < 0f)
            {
                targetPosition.y -= dist;
            }
            else if (tvp.y > 1f)
            {
                targetPosition.y += dist;
            }

            _tilesInMoveToDestroy.Add(tile);
            tile.MoveToPosition(targetPosition, () =>
            {
                _tilesInMoveToDestroy.Remove(tile);
                tile.DoActionWithTile(EActionWithElement.MOVE);
                DestroyTile(tileElement, ETileDestroyType.OUT_GRID);
            });
            OnTileMoved();

            return true;
        }

        return false;

        void OnGetPoint(GridCell targetTile, Action action = null)
        {
            if (targetTile.GridPoint != clickedTile.GridPoint)
            {
                tile.MoveToPosition(targetTile.WorldPoint, () =>
                {
                    BounceElements();
                    action?.Invoke();
                    CheckStageEnding();
                });

                if (targetTile.BoardElementType != EBoardElementType.SAW)
                {
                    _gridController.UpdateTileInfo(targetTile, clickedTile);
                }

                OnTileMoved();

                void BounceElements()
                {
                    var elementsInLine = _gridController.GetElementsInDirectionLine(targetTile);
                    elementsInLine.Insert(0, targetTile);
                    BounceLineElements(elementsInLine, tile.MoveDirection);
                }
            }
        }

        void OnTileMoved()
        {
            _rightClickRage++;

            OnCompletedActionOnBoard();

            clickedTile.BaseBoardElementController.DoActionWithTile(EActionWithElement.MOVE);

            _gridController.ClearTile(clickedTile);
            FindSolution();
        }
    }

    private void DestroyTile(BaseBoardElementController tile, ETileDestroyType eTileDestroyType)
    {
        if (tile.BoardElementType == EBoardElementType.TILE)
        {
            _levelTilesCount--;
        }

        if (tile.BoardElementType == EBoardElementType.BOMB && tile is BombController bomb)
        {
            VibrationController.Instance.Play(EVibrationType.MediumImpact);

            bomb.Explode();

            if (eTileDestroyType != ETileDestroyType.BOMB)
            {
                var explodedCells = bomb.FindTargets(_gridController);

                foreach (var cell in explodedCells)
                {
                    if (cell.BoardElementType == EBoardElementType.TILE_LOCKED)
                    {
                        ((TileElementController)cell.BaseBoardElementController).RemoveRemainingActions(cell);
                    }
                    else
                    {
                        cell.BaseBoardElementController.DoActionWithTile(EActionWithElement.DESTROY);
                        DestroyTile(cell.BaseBoardElementController, ETileDestroyType.BOMB);
                        _gridController.ClearTile(cell);
                    }
                }
            }
        }
        else
        {
            if (eTileDestroyType == ETileDestroyType.SAW)
            {
                VibrationController.Instance.Play(EVibrationType.MediumImpact);

                tile.GetComponent<IDestructibleElement>()?.PlayDestroyFX();
            }

            if (eTileDestroyType == ETileDestroyType.BOMB)
            {
                tile.GetComponent<IDestructibleElement>()?.PlayDestroyFX();
            }
        }

        tile.OnBeforeDestroy();
        Destroy(tile.gameObject);

        CheckStageEnding();
    }

    private bool OnClickRotator(GridCell clickedElement)
    {
        if (_gridController.RotateGridWithRotator(clickedElement))
        {
            FindSolution();
            return true;
        }

        return false;
    }

    private Vector2 GetOutGridPosition(GridCell clickedElement)
    {
        var clickedTile = (TileElementController)clickedElement.BaseBoardElementController;
        var worldPosition = (Vector2)_cameraManager.GetCameraItem(ECameraType.GAME).Camera
            .ScreenToWorldPoint(new Vector2(Screen.width + 100f, Screen.height + 100f));

        var targetPosition = worldPosition * clickedTile.MoveDirection;
        Vector2 outGridPosition;

        if (targetPosition.y == 0)
        {
            outGridPosition = new Vector2(targetPosition.x, clickedElement.WorldPoint.y);
        }
        else
        {
            outGridPosition = new Vector2(clickedElement.WorldPoint.x, targetPosition.y);
        }

        return outGridPosition;
    }

    private void OnInstantiateElement(BaseBoardElementController boardElementController)
    {
        var elementTransform = boardElementController.transform;
        var cell = _gridController.GetCell(boardElementController.GridPosition);

        elementTransform.localPosition = cell.WorldPoint;
        elementTransform.localScale = Vector3.one * _gridController.CellSize / 2f;

        _gridController.SetCellController(cell.GridPoint.x, cell.GridPoint.y, boardElementController);

        SetupElement(boardElementController);
    }

    private void SetupElement(BaseBoardElementController boardElementController)
    {
        switch (boardElementController.BoardElementType)
        {
            case EBoardElementType.TILE:
            case EBoardElementType.TILE_LOCKED:
                _levelTilesCount++;
                var boardElementControllerGridPosition = boardElementController.GridPosition;
                var tileElement = _jsonFile.tiles.ToList()
                    .Find(element => element.position.ToVector() == boardElementControllerGridPosition);
                var lockElement = _jsonFile.locks.ToList()
                    .Find(element => element.position.ToVector() == boardElementControllerGridPosition);
                var colorElement = _jsonFile.colors == null || _jsonFile.colors.Length == 0
                    ? null
                    : _jsonFile.colors.ToList().Find(element =>
                        element.position.ToVector() == boardElementControllerGridPosition);
                var tileElementController = (TileElementController)boardElementController;
                var tileColor =
                    colorElement != null && Enum.TryParse<ETileColor>(colorElement.tileColor, out var tileColor0)
                        ? tileColor0
                        : ETileColor.None;
                tileElementController.Color = tileColor;

                tileElementController.ChangeRotation(tileElement.direction);
                tileElementController.SetRemainingActions(lockElement?.charges ?? 0);
                tileElementController.OnKeyFound += _onKeyFound;
                break;

            case EBoardElementType.ROTATOR:
                var rotatorElementController = (RotatorElementController)boardElementController;
                rotatorElementController.InitializeRotatorInfo(BoardElements);
                FillRotatorElements(boardElementController);
                break;

            case EBoardElementType.EMPTY:
            case EBoardElementType.SAW:
            case EBoardElementType.NONE:
            case EBoardElementType.WALL:
            case EBoardElementType.BOMB:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateCurrentSkin(string skinName) { }

    public void SetSkin(int index, bool debug, int levelNum, int stage) { }

    public void SetSkin(BaseSkinConfigEntry skin)
    {
        SetSkin(skin, _levelNum, _stageNum);
    }

    public void SetSkin(BaseSkinConfigEntry skin, int levelNum, int stage)
    {
        foreach (var tile in _gridController.GetElementControllersOfType<TileElementController>())
        {
            tile.SetSkin(skin.ProduceSingleSkinConfig(levelNum, stage, tile.Color), _gridController.CellSize);
        }

        _gridController.UpdateSortOrders();
    }

    private void FillRotatorElements(BaseBoardElementController boardElementController)
    {
        var rotatorElement = _jsonFile.rotators.ToList().Find(element =>
            new Vector2(element.position.x, element.position.y) == boardElementController.GridPosition);
        var rotatorElementController = (RotatorElementController)boardElementController;

        rotatorElementController.onClearTile += _gridController.ClearTile;
        rotatorElementController.onUpdateTiles += _gridController.UpdateMultipleTilesInfo;
        rotatorElementController.onDestroyTile += OnDestroyTile;

        rotatorElementController.FillRotatorElements(BoardElements);

        void OnDestroyTile(BaseBoardElementController tile, ETileDestroyType tileDestroyType)
        {
            tile.DoActionWithTile(EActionWithElement.DESTROY);
            DestroyTile(tile, tileDestroyType);
        }
    }

    private void CheckStageEnding(bool skipFailCheck = false)
    {
        if (_isStageCompleted)
        {
            return;
        }

        if (IsStageCompleted())
        {
            _isStageCompleted = true;
            UnSubscribe();

            _onStageCompleted?.Invoke(ELevelCompleteReason.WIN);
            return;
        }


        if (!skipFailCheck && _tilesInMoveToDestroy.Count == 0)
        {
            _failStateController.TryScheduleFailCheck(_gridController);
        }
    }

    private void OnStageFailed(bool isOutOfMoves)
    {
        if (_isStageCompleted)
        {
            return;
        }

        if (isOutOfMoves)
        {
            _onStageCompleted?.Invoke(ELevelCompleteReason.LOSE_OUT_OF_MOVES);
            return;
        }

        _isStageCompleted = true;
        UnSubscribe();

        StartCoroutine(PlayLoseAnimation(0.1f,
            () => _onStageCompleted?.Invoke(ELevelCompleteReason.LOSE)));
    }

    private IEnumerator PlayLoseAnimation(float delay, Action callback)
    {
        var tiles = _gridController.GetElementControllersOfType<TileElementController>();
        for (var i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] == null)
            {
                continue;
            }

            var isLast = i == tiles.Count - 1;
            tiles[i].OnWrongClick(isLast ? callback : null);

            yield return new WaitForSeconds(delay);
        }
    }

    private bool IsStageCompleted()
    {
        return _levelTilesCount == 0;
    }

    private void InitializeData()
    {
        _cameraManager = CameraManager.Instance;
        _gameBoardInteraction = GameBoardInteraction.Instance;
    }

    private void FindSolution()
    {
        if (BoardElements.TrueForAll(c => c.BoardElementType != EBoardElementType.TILE))
        {
            if (DebugPanel.Instance)
            {
                DebugPanel.Instance.HideSolutionTime();
            }

            return;
        }

        IEnumerator Co_Find()
        {
            if (!GameConfig.RemoteConfig.hintWhenPlayerStuck)
            {
                if (Gamestoty.debugDevice && DebugPanel.Instance)
                {
                    DebugPanel.Instance.HideSolutionTime();
                }

                yield break;
            }

            if (Gamestoty.debugDevice && DebugPanel.Instance)
            {
                DebugPanel.Instance.ShowFindSolutionInProcess();
            }

            HideHint();
            BoardSolutionFinder.FindSolution(_gridController.Grid, _jsonFile);

            var hintDelay = GameConfig.RemoteConfig.hintWhenPlayerStuckDelay;

            yield return new WaitForSecondsRealtime(hintDelay);

            while (!BoardSolutionFinder.SoultionFinderFinished)
            {
                yield return null;
            }

            var found = BoardSolutionFinder.HasSolution;
            if (found && BoardSolutionFinder.SolutionCell != null &&
                BoardSolutionFinder.SolutionCell.BaseBoardElementController != null)
            {
                BoardSolutionFinder.SolutionCell.BaseBoardElementController.ShowHint();
                _hintCell = BoardSolutionFinder.SolutionCell.BaseBoardElementController;
            }
        }

        if (_finder != null)
        {
            StopCoroutine(_finder);
            _finder = null;
        }

        _finder = StartCoroutine(Co_Find());

        if (Gamestoty.debugDevice)
        {
            IEnumerator Co_Log()
            {
                var prevSec = 0f;
                while (!BoardSolutionFinder.SoultionFinderFinished)
                {
                    var sw = BoardSolutionFinder.SW;
                    if (sw != null)
                    {
                        var sec = (int)(sw.Elapsed.TotalSeconds * 10.0) * 0.1f;
                        if (sec != prevSec)
                        {
                            prevSec = sec;
                            if (DebugPanel.Instance)
                            {
                                DebugPanel.Instance.ShowFindSolutionInProcess(sec);
                            }
                        }
                    }

                    yield return null;
                }

                if (DebugPanel.Instance)
                {
                    DebugPanel.Instance.ShowFindSolutionTime(BoardSolutionFinder.SW, BoardSolutionFinder.HasSolution);
                }
            }

            if (_logger != null)
            {
                StopCoroutine(_logger);
                _logger = null;
            }

            _logger = StartCoroutine(Co_Log());
        }
    }

    private void HideHint()
    {
        if (!_hintCell)
        {
            return;
        }

        _hintCell.HideHint();
        _hintCell = null;
    }
}
