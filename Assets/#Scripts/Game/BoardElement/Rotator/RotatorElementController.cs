using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class RotatorElementController : BaseBoardElementController, IClickableElement
{
    [Header("Transform")] [SerializeField] private Transform _turningTop;

    [SerializeField] private Transform _holderTransform;
    [SerializeField] private RotatorsHolder _rotatorsHolderPrefab;
    [SerializeField] private SpriteRenderer _turningBaseSR;
    [SerializeField] private SpriteRenderer _turningTopSR;

    private Dictionary<int, float> _lineGroups = new();
    private Vector3 _startTurningTopPosition;

    public List<RotatorsHolder> Holders { get; } = new();

    private bool _isRotating { get; set; }
    public bool IsRotating => _isRotating;

    private void Update()
    {
        if (!_isRotating)
        {
            return;
        }

        foreach (var holder in Holders)
        {
            foreach (var holdersGroup in holder.RotatorsGroup)
            {
                if (holdersGroup.Cell.BaseBoardElementController != null)
                {
                    holdersGroup.Cell.BaseBoardElementController.transform.position =
                        holdersGroup.DotTransform.position;
                }
            }
        }
    }

    public void OnClickElement(TileState tileState)
    {
        switch (tileState)
        {
            case TileState.IDLE:

                break;

            case TileState.PRESSED:

                break;
        }
    }

    public event Action<BaseBoardElementController, ETileDestroyType> onDestroyTile;
    public event Action<GridCell> onClearTile;
    public event Action<Dictionary<GridCell, GridCell>> onUpdateTiles;

    public override void InitializeBoardElementInfo()
    {
        _startTurningTopPosition = _turningTop.localPosition;
    }

    public override int ChangeSortOrder(int sortOrder)
    {
        var highestSortOrder = base.ChangeSortOrder(sortOrder + 120);
        _turningBaseSR.sortingOrder = sortOrder;
        _turningTopSR.sortingOrder = sortOrder + 150;
        Holders.ForEach(h => h.SetSpriteOrders(sortOrder + 148));
        return highestSortOrder;
    }

    public void FillRotatorElements(List<GridCell> boardElements)
    {
        var elements = new List<GridCell>();

        foreach (var holder in Holders)
        {
            holder.onDestroyHolder += OnDestroyHolder;

            elements = GetElementPositionByDirection(boardElements, holder.Direction, holder.Length);
            Subscribe(elements, holder);
        }
    }

    public void Subscribe(List<GridCell> gridCells, RotatorsHolder holder)
    {
        for (var i = 0; i < gridCells.Count; i++)
        {
            holder.FillInfo(gridCells[i], i);
        }
    }

    public void InitializeRotatorInfo(List<GridCell> boardElements)
    {
        _lineGroups = SetDotPosition(boardElements);

        foreach (var holder in Holders)
        {
            var directionElement = _lineGroups.ToList().Find(x => x.Key == holder.Direction);

            holder.ScaleMultiplayer = directionElement.Value;
            holder.SetupLineScale();
        }
    }

    public List<GridCell> RotateOnTargetMultiplayer(List<GridCell> boardGridsCells, int multiplayer)
    {
        return FakeRotate(boardGridsCells, multiplayer);
    }

    public bool TryToRotate(List<GridCell> boardGridsCells)
    {
        if (_isRotating || Holders.Count == 0)
        {
            return false;
        }

        var availableRotateMultipliers = GetAvailableRotationDirections(boardGridsCells);
        if (availableRotateMultipliers == null || availableRotateMultipliers.Count == 0)
        {
            GetUpTurning(() =>
            {
                WrongRotationAnimation(boardGridsCells, () => { GetDownTurning(); });
            });

            return false;
        }

        var multiplayer = availableRotateMultipliers.First();

        foreach (var holder in Holders)
        {
            holder.ChangeDirection(multiplayer);
        }

        ChangeRotatingElementsSortOrder(boardGridsCells);

        Rotate(multiplayer, () => { OnRotatingDone(boardGridsCells); });

        return true;
    }

    public void FillHolders()
    {
        foreach (var holder in Holders)
        {
            holder.Initialize();
        }
    }

    public RotatorsHolder InstantiateTurningTop()
    {
        var rotatorsHolder = Instantiate(_rotatorsHolderPrefab, _holderTransform);
        rotatorsHolder.onDestroyHolder += RemoveHolder;

        Holders.Add(rotatorsHolder);

        return rotatorsHolder;
    }

    public List<int> GetAvailableRotationDirections(List<GridCell> boardGridsCells)
    {
        var availableMultipliers = new List<int>();
        var maxLength = GetLargestHolderLenght();

        var controlledCellsCount = CountControlledCells();
        if (controlledCellsCount <= 0)
        {
            return availableMultipliers;
        }

        bool CanRotate(int canRotateHoldersCount)
        {
            return canRotateHoldersCount == controlledCellsCount;
        }

        for (var direction = 1; direction < 4;)
        {
            var canRotateHoldersCount = 0;

            foreach (var holder in Holders)
            {
                for (var level = 0; level < maxLength; level++)
                {
                    var group = holder.GetGroupByLevelId(level);
                    if (group == null)
                    {
                        continue;
                    }

                    var targetDirection = (holder.Direction - direction) % 4;
                    if (targetDirection < 0)
                    {
                        targetDirection += 4;
                    }

                    var targetGroup = GetElementPositionByDirection(boardGridsCells, targetDirection, maxLength);
                    if (targetGroup == null || targetGroup.Count - 1 < level)
                    {
                        canRotateHoldersCount = 0;
                        direction++;
                        break;
                    }

                    if (group.Cell.GridPoint == targetGroup[level].GridPoint ||
                        group.Cell.BoardElementType == EBoardElementType.EMPTY)
                    {
                        continue;
                    }

                    if (!IsControlledCell(targetGroup[level]) && !IsAvailableRotation(group, targetGroup[level])
                       )
                    {
                        canRotateHoldersCount = 0;
                        direction++;
                        break;
                    }

                    canRotateHoldersCount++;

                    if (CanRotate(canRotateHoldersCount))
                    {
                        canRotateHoldersCount = 0;

                        availableMultipliers.Add(direction);

                        direction++;
                    }

                    if (direction <= 3)
                    {
                        continue;
                    }

                    return availableMultipliers;
                }
            }
        }

        return availableMultipliers;
    }

    private int CountControlledCells()
    {
        return Holders.SelectMany(holder => holder.RotatorsGroup)
            .Count(holdersGroup => holdersGroup.Cell.BaseBoardElementController != null);
    }

    public bool IsControlledCell(GridCell gridCell)
    {
        return GetRotatorControllingCell(gridCell) != null;
    }

    public RotatorGroup GetRotatorControllingCell(GridCell gridCell)
    {
        return Holders.SelectMany(holder => holder.RotatorsGroup)
            .FirstOrDefault(rotatorGroup => rotatorGroup.Cell.GridPoint == gridCell.GridPoint);
    }

    private bool IsAvailableRotation(RotatorGroup group, GridCell targetGroup)
    {
        var currentBoardElementType = group.Cell.BoardElementType;
        var targetBoardElementType = targetGroup.BoardElementType;

        if (IsNextPointLocker() || IsNextPointRotator() || IsSawsRotation() || IsBombsRotation() || IsAllTiles() ||
            IsWallsRotation())
        {
            return false;
        }

        return true;

        bool IsNextPointLocker()
        {
            return currentBoardElementType != EBoardElementType.EMPTY &&
                   targetBoardElementType == EBoardElementType.TILE_LOCKED;
        }

        bool IsAllTiles()
        {
            return currentBoardElementType == EBoardElementType.TILE &&
                   targetBoardElementType == EBoardElementType.TILE;
        }

        bool IsNextPointRotator()
        {
            return targetBoardElementType == EBoardElementType.ROTATOR;
        }


        bool IsSawsRotation()
        {
            return currentBoardElementType == EBoardElementType.SAW &&
                   (targetBoardElementType == EBoardElementType.SAW ||
                    targetBoardElementType == EBoardElementType.TILE_LOCKED);
        }

        bool IsBombsRotation()
        {
            return currentBoardElementType == EBoardElementType.BOMB &&
                   targetBoardElementType == EBoardElementType.BOMB;
        }

        bool IsWallsRotation()
        {
            return (currentBoardElementType == EBoardElementType.TILE &&
                    targetBoardElementType == EBoardElementType.WALL) ||
                   (currentBoardElementType == EBoardElementType.WALL &&
                    targetBoardElementType == EBoardElementType.TILE);
        }
    }

    private void RemoveHolder(RotatorsHolder rotatorsHolder)
    {
        Holders.Remove(rotatorsHolder);
        _rotatorsHolderPrefab.onDestroyHolder -= RemoveHolder;
    }

    private void WrongRotationAnimation(List<GridCell> boardGridsCells, Action callback = null)
    {
        var direction = new List<int>();
        var wrongGridCells = new List<GridCell>();

        foreach (var holder in Holders)
        {
            if (holder.RotatorsGroup.Count > 0)
            {
                direction.Add(holder.Direction);
            }
        }

        for (var i = 0; i < 4; i++)
        {
            var isContains = direction.Contains(i);

            if (!isContains)
            {
                var gridsCells = GetElementPositionByDirection(boardGridsCells, i, GetLargestHolderLenght());
                foreach (var gridCell in gridsCells)
                {
                    wrongGridCells.Add(gridCell);
                }
            }
        }

        foreach (var wrongCell in wrongGridCells)
        {
            if (wrongCell.BaseBoardElementController != null)
            {
                wrongCell.BaseBoardElementController.OnWrongClick();
            }
        }

        if (GameConfig.RemoteConfig.wrongClickHapticVersion <= 0)
        {
            VibrationController.Instance.Play(EVibrationType.MediumImpact);
        }
        else
        {
            VibrationController.Instance.Play(EVibrationType.Failure);
        }

        ShakeTile(callback);
    }

    private void GetUpTurning(Action callback = null)
    {
        _isRotating = true;

        _holderTransform.DOLocalMove(Vector3.up * 0.6f, 0.1f).SetLink(_holderTransform.gameObject);
        _turningTop.DOLocalMove(Vector3.up * 0.6f, 0.1f).OnComplete(() => { callback?.Invoke(); })
            .SetLink(_turningTop.gameObject);
    }

    private void GetDownTurning(Action callback = null)
    {
        _holderTransform.DOLocalMove(Vector3.zero, 0.1f).OnComplete(() =>
        {
            callback?.Invoke();
            OnRotatorReset();
        }).SetLink(_holderTransform.gameObject);
    }

    private void Rotate(int multiplayer, Action callback = null)
    {
        GetUpTurning(OnRotatorUp);

        void OnRotatorUp()
        {
            AnimateRotating(multiplayer, callback);
        }
    }

    private void AnimateRotating(int multiplayer, Action callback)
    {
        foreach (var holder in Holders)
        {
            if (holder != null)
            {
                var group = _lineGroups.ToList().Find(x => holder.Direction == x.Key);
                holder.ScaleMultiplayer = group.Value;
                holder.ChangeLineScale(false);
            }
        }

        _holderTransform.DOLocalRotate(_holderTransform.eulerAngles + new Vector3(0, 0, -multiplayer * 90f), 0.25f)
            .SetLink(_holderTransform.gameObject)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                GetDownTurning(() => { callback?.Invoke(); });
            });
    }

    private void OnRotatorReset()
    {
        _turningTop.DOLocalMove(_startTurningTopPosition, 0.1f).OnComplete(() => { _isRotating = false; })
            .SetLink(_turningTop.gameObject);
    }

    private void OnRotatingDone(List<GridCell> boardGridsCells)
    {
        var movedTiles = new Dictionary<GridCell, GridCell>();
        var holdersUpdateGroup = new List<HoldersUpdateInfo>();
        var destroyedTiles = new Dictionary<BaseBoardElementController, ETileDestroyType>();

        foreach (var holder in Holders.ToList())
        {
            foreach (var rotatorsGroup in holder.RotatorsGroup.ToList())
            {
                var previousCell = boardGridsCells.FirstOrDefault(r => r.GridPoint == rotatorsGroup.Cell.GridPoint);
                var targetCell =
                    GetElementPositionByDirection(boardGridsCells, holder.Direction, GetLargestHolderLenght())[
                        rotatorsGroup.GridLevel];

                var rotatedElement = previousCell.BoardElementType;
                var targetElement = targetCell.BoardElementType;

                var destroy = false;
                if (!IsPartOfAnyHolder(targetCell))
                {
                    if (rotatedElement == EBoardElementType.SAW && CanBeDestroyedBySaw(targetElement))
                    {
                        destroyedTiles.Add(targetCell.BaseBoardElementController, ETileDestroyType.SAW);
                    }

                    if (targetElement == EBoardElementType.SAW && CanBeDestroyedBySaw(rotatedElement))
                    {
                        destroy = true;
                        destroyedTiles.Add(previousCell.BaseBoardElementController, ETileDestroyType.SAW);
                    }
                }

                if (!destroy)
                {
                    var previousCellCopy = new GridCell(previousCell);


                    var newCellCopy = new GridCell(targetCell);

                    movedTiles.Add(targetCell, previousCell);
                    var holdersUpdateInfo = new HoldersUpdateInfo
                    {
                        RotatorGroup = rotatorsGroup, NewGridCell = newCellCopy, PreviousGridCell = previousCellCopy
                    };

                    holdersUpdateGroup.Add(holdersUpdateInfo);
                }
            }
        }

        foreach (var holder in holdersUpdateGroup)
        {
            UpdateHoldersGroup(holder.RotatorGroup, holder.NewGridCell, holder.PreviousGridCell);
        }

        DestroyTiles(destroyedTiles);

        onUpdateTiles?.Invoke(movedTiles);
    }

    private bool CanBeDestroyedBySaw(EBoardElementType targetElement)
    {
        return targetElement == EBoardElementType.TILE ||
               targetElement == EBoardElementType.WALL ||
               targetElement == EBoardElementType.BOMB;
    }

    private bool IsPartOfAnyHolder(GridCell targetCell)
    {
        foreach (var holder in Holders)
        {
            foreach (var rotatorsGroup in holder.RotatorsGroup)
            {
                if (rotatorsGroup.Cell.GridPoint == targetCell.GridPoint)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private List<GridCell> FakeRotate(List<GridCell> boardGridsCells, int direction)
    {
        var targetClickedGroup = new Dictionary<GridCell, GridCell>();
        var destroyedTiles = new List<BaseBoardElementController>();
        var cleared = new List<GridCell>();

        foreach (var holder in Holders.ToList())
        {
            foreach (var rotatorsGroup in holder.RotatorsGroup.ToList())
            {
                var targetDirection = (holder.Direction - direction) % 4;
                if (targetDirection < 0)
                {
                    targetDirection += 4;
                }

                var gridCell = rotatorsGroup.Cell;
                var elementsInDirection =
                    GetElementPositionByDirection(boardGridsCells, targetDirection, GetLargestHolderLenght());
                var targetCell = elementsInDirection[rotatorsGroup.GridLevel];
                var targetElement = targetCell.BoardElementType;
                var rotatedElement = gridCell.BoardElementType;

                var destroy = false;
                if (!IsPartOfAnyHolder(targetCell))
                {
                    if (rotatedElement == EBoardElementType.SAW && CanBeDestroyedBySaw(targetElement))
                    {
                        destroyedTiles.Add(targetCell.BaseBoardElementController);
                    }

                    if (targetElement == EBoardElementType.SAW && CanBeDestroyedBySaw(rotatedElement))
                    {
                        destroy = true;
                        destroyedTiles.Add(gridCell.BaseBoardElementController);
                    }
                }

                if (!destroy)
                {
                    var target = GetNewGridCell(targetCell);
                    var clicked = GetNewGridCell(gridCell);

                    targetClickedGroup.Add(target, clicked);
                    cleared.Add(gridCell);
                }
            }
        }

        TempDestroyTiles(destroyedTiles);

        TempClearTiles(boardGridsCells, cleared);
        TempUpdateTiles(boardGridsCells, targetClickedGroup);

        return boardGridsCells;

        GridCell GetNewGridCell(GridCell targetGridCelleo)
        {
            var newGridCell = new GridCell();

            newGridCell.BaseBoardElementController = targetGridCelleo.BaseBoardElementController;
            newGridCell.GridPoint = targetGridCelleo.GridPoint;
            newGridCell.WorldPoint = targetGridCelleo.WorldPoint;
            newGridCell.SortOrder = targetGridCelleo.SortOrder;
            newGridCell.BoardElementType = targetGridCelleo.BoardElementType;

            return newGridCell;
        }

        void TempClearTiles(List<GridCell> boardGridsCells, List<GridCell> cleared)
        {
            foreach (var group in cleared)
            {
                ClearTileInfo(ref boardGridsCells, group);
            }

            void ClearTileInfo(ref List<GridCell> boardGridsCells, GridCell tile)
            {
                for (var i = 0; i < boardGridsCells.Count; i++)
                {
                    if (boardGridsCells[i].GridPoint != tile.GridPoint)
                    {
                        continue;
                    }

                    var gridCell = new GridCell();

                    gridCell.BaseBoardElementController = null;
                    gridCell.BoardElementType = EBoardElementType.EMPTY;
                    gridCell.GridPoint = boardGridsCells[i].GridPoint;
                    gridCell.WorldPoint = boardGridsCells[i].WorldPoint;
                    gridCell.SortOrder = boardGridsCells[i].SortOrder;

                    boardGridsCells[i] = gridCell;
                }
            }
        }

        void TempUpdateTiles(List<GridCell> boardGridsCells, Dictionary<GridCell, GridCell> targetClickedGroup)
        {
            foreach (var tile in targetClickedGroup)
            {
                UpdateTileInfo(ref boardGridsCells, tile.Key, tile.Value);
            }

            void UpdateTileInfo(ref List<GridCell> boardGridsCells, GridCell targetTile, GridCell clickedTile)
            {
                for (var i = 0; i < boardGridsCells.Count; i++)
                {
                    if (boardGridsCells[i].GridPoint != targetTile.GridPoint)
                    {
                        continue;
                    }

                    var gridCell = new GridCell();

                    gridCell.BaseBoardElementController = clickedTile.BaseBoardElementController;
                    gridCell.BoardElementType = clickedTile.BoardElementType;
                    gridCell.GridPoint = boardGridsCells[i].GridPoint;
                    gridCell.WorldPoint = boardGridsCells[i].WorldPoint;
                    gridCell.SortOrder = boardGridsCells[i].SortOrder;

                    boardGridsCells[i] = gridCell;
                }
            }
        }
    }

    private void TempDestroyTiles(List<BaseBoardElementController> destroyedTiles)
    {
        foreach (var destroyedTile in destroyedTiles)
        {
            destroyedTile.DoActionWithTile(EActionWithElement.DESTROY);
        }
    }

    private void DestroyTiles(Dictionary<BaseBoardElementController, ETileDestroyType> destroyedTiles)
    {
        foreach (var destroyTile in destroyedTiles)
        {
            onDestroyTile?.Invoke(destroyTile.Key, destroyTile.Value);
        }
    }

    private void UpdateHoldersGroup(RotatorGroup rotatorsGroup, GridCell newCell, GridCell previousCell)
    {
        rotatorsGroup.Cell = newCell;
        rotatorsGroup.Cell.BaseBoardElementController = previousCell.BaseBoardElementController;
        rotatorsGroup.Cell.BoardElementType = previousCell.BoardElementType;
    }

    private void OnDestroyHolder(RotatorsHolder rotatorsHolder)
    {
        rotatorsHolder.onDestroyHolder -= OnDestroyHolder;
    }

    private int GetLargestHolderLenght()
    {
        return Holders.Select(holder => holder.Length).Prepend(0).Max();
    }

    private Dictionary<int, float> SetDotPosition(List<GridCell> boardElements)
    {
        var group = new Dictionary<int, float>();
        for (var direction = 0; direction < 4; direction++)
        {
            var Grids = GetElementPositionByDirection(boardElements, direction, 1);
            if (Grids != null)
            {
                var distance = Grids[0].WorldPoint - (Vector2)transform.position;
                group.Add(direction, 1f / transform.localScale.x * distance.magnitude);
            }
        }

        return group;
    }

    private List<GridCell> GetElementPositionByDirection(List<GridCell> boardElements, int direction, int length)
    {
        var targetElements = new List<GridCell>();

        for (var i = 1; i <= length; i++)
        {
            var rotatorPosition = new Vector2(GridPosition.x, GridPosition.y);
            Vector2 gridPosition = default;

            gridPosition = rotatorPosition + GetDirection(direction) * i;

            var targetElement = boardElements.Find(x => x.GridPoint == gridPosition);

            if (targetElement != null)
            {
                targetElements.Add(targetElement);
            }
        }

        if (targetElements.Count == 0)
        {
            return null;
        }

        return targetElements;
    }

    private Vector2 GetDirection(int direction)
    {
        return ((EMoveDirection)direction).GetDirection();
    }

    private BaseBoardElementController GetElementFromList(List<GridCell> boardElements, GridCell cell)
    {
        var boardElement = boardElements.Find(element => element.GridPoint == cell.GridPoint)
            .BaseBoardElementController;

        return boardElement;
    }

    private void ChangeRotatingElementsSortOrder(List<GridCell> boardGridsCells)
    {
        foreach (var holder in Holders)
        {
            foreach (var rotatorsGroup in holder.RotatorsGroup)
            {
                var element = GetElementFromList(boardGridsCells, rotatorsGroup.Cell);
                if (element != null)
                {
                    element.AddSortOrder(120);
                }
            }
        }
    }

    public class HoldersUpdateInfo
    {
        public GridCell NewGridCell;
        public GridCell PreviousGridCell;
        public RotatorGroup RotatorGroup;
    }
}
