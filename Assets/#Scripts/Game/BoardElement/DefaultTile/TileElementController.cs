using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TileElementController : BaseBoardElementController, IMovableBoardElement, IClickableElement,
    IDestructibleElement
{
    [Header("Internal")] [SerializeField] private TileModelController _modelController;

    [SerializeField] private Transform _bouncePivotTransform;

    [Header("Speed")] [SerializeField] private float _speed = 1.5f;

    private readonly List<Transform> _bounceTargets = new();

    private int _lastSortOrder;

    private Tweener _punchScaleTween;

    public bool HasKey { get; private set; }

    public GameObject Key => _modelController.Key;

    public int RemainingActionsForUnlock { get; private set; }

    public Vector2Int MoveDirection { get; private set; }
    public EMoveDirection EMoveDirection { get; private set; }

    public Vector2 CurrentPosition { get; set; }
    public ETileColor Color { get; set; }

    private void Awake()
    {
        if (_modelController)
        {
            _modelController.Init(transform);
        }
    }

    public void OnClickElement(TileState tileState)
    {
        if (this == null)
        {
            return;
        }

        switch (tileState)
        {
            case TileState.IDLE:
                OnTileUnPressed();
                break;

            case TileState.PRESSED:
                OnTilePressed();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(tileState), tileState, null);
        }
    }

    public void PlayDestroyFX()
    {
        EnableTileEffect(ETileEffect.DIE);
    }

    public void MoveToPosition(Vector2 worldPosition, Action callback = null)
    {
        if (HasKey)
        {
            OnKeyFound?.Invoke(this);
        }

        ResetBounceTween();
        _modelController.SetTrailState(true);
        StartCoroutine(MoveTile(worldPosition, () => callback?.Invoke()));
    }

    public event Action<TileElementController> OnKeyFound;

    public override int ChangeSortOrder(int sortOrder)
    {
        _lastSortOrder = sortOrder;

        var highestSortOrder = base.ChangeSortOrder(sortOrder);
        highestSortOrder++;

        _modelController.ChangeSortOrder(highestSortOrder);

        return highestSortOrder;
    }

    public override void AddSortOrder(int value)
    {
        ChangeSortOrder(_lastSortOrder + value);
    }

    private void RefreshSortOrder()
    {
        ChangeSortOrder(_lastSortOrder);
    }

    public override void InitializeBoardElementInfo()
    {
        RecreateSpriteGroups();

        OnTileUnPressed();
    }

    private void RecreateSpriteGroups()
    {
        _sprites = _modelController.GetSpritesForSorting();
        _wrongDefaultGroups = _modelController.GetWrongDefaultGroups();

        RefreshSortOrder();
    }

    public void DoPunchScale()
    {
        if (_punchScaleTween != null)
        {
            _punchScaleTween.Restart();
            return;
        }

        _punchScaleTween = transform.DOPunchScale(Vector3.one * 0.05f, 0.2f, 1).SetEase(Ease.Linear)
            .OnComplete(() => { _punchScaleTween = null; })
            .SetLink(gameObject);
    }

    private IEnumerator MoveTile(Vector2 targetPosition, Action callback = null)
    {
        var speed = 0f;

        var distance = Mathf.Abs((targetPosition - (Vector2)transform.localPosition).magnitude);


        var speedIncrease = 1f;
        var time = 0f;
        var dir = (targetPosition - (Vector2)_elementTransform.localPosition).normalized;

        var targetVP = CameraManager.Instance.GetCameraItem(ECameraType.GAME).Camera
            .WorldToViewportPoint(targetPosition);
        var isOnFieldView = targetVP.x >= 0f && targetVP.x <= 1f && targetVP.y >= 0f && targetVP.y <= 1f;

        while (Vector3.Distance(_elementTransform.localPosition, targetPosition) >= 0.05f)
        {
            var deltaTime = Time.deltaTime;

            if (GameConfig.RemoteConfig.moveTileVersion <= 0)
            {
                speedIncrease *= 1.2f;
                speed = Mathf.Clamp(speedIncrease, 0f, _speed);
            }
            else
            {
                time += deltaTime;
                speed = Mathf.Exp(1f + time * 5f);
            }

            _elementTransform.localPosition =
                Vector3.MoveTowards(_elementTransform.localPosition, targetPosition, deltaTime * speed);
            yield return new WaitForEndOfFrame();
        }

        if (isOnFieldView)
        {
            _elementTransform.localPosition = targetPosition;
            callback?.Invoke();
        }
        else
        {
            callback?.Invoke();
            for (var time1 = 0f; time1 < 1f; time1 += Time.deltaTime)
            {
                _elementTransform.localPosition += (Vector3)dir * speed;
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public void ChangeRotation(int directionIndex)
    {
        var moveDirection = GetMoveDirection(directionIndex);
        MoveDirection = moveDirection.GetMoveVector();
        EMoveDirection = (EMoveDirection)directionIndex;
        _modelController.ChangeRotation(moveDirection, GetArrowRotation(moveDirection));
    }

    private void OnTilePressed()
    {
        SetWrongEffectState(false);
        _modelController.OnTilePressed();
    }

    private void OnTileUnPressed()
    {
        _modelController.OnTileUnPressed();
    }

    public void SetHasKey(bool hasKey)
    {
        HasKey = hasKey;

        _modelController.SetKeyEffect(hasKey);
    }

    protected override void OnStageLayerUpdate(bool isLocked)
    {
        _modelController.OnStageLayerUpdate(isLocked);
    }

    public void SetSkin(SingleSkinConfigEntry skin, float cellSize)
    {
        _modelController.SetSkin(skin, cellSize);

        RecreateSpriteGroups();
        OnTileUnPressed();
    }

    public void SetRemainingActions(int charges)
    {
        RemainingActionsForUnlock = charges;
        _modelController.UpdateActionsLock(RemainingActionsForUnlock);
        if (_lockedStageSpriteRenderer && RemainingActionsForUnlock > 0)
        {
            _lockedStageSpriteRenderer.transform.localScale = Vector3.one * 1.1f;
        }
    }

    public bool DecreaseRemainingActionsLock(GridCell cell)
    {
        RemainingActionsForUnlock--;

        return RemainingActionsChanged(cell);
    }

    public bool RemoveRemainingActions(GridCell cell)
    {
        RemainingActionsForUnlock = 0;

        return RemainingActionsChanged(cell);
    }

    private bool RemainingActionsChanged(GridCell cell)
    {
        _modelController.UpdateActionsLock(RemainingActionsForUnlock, true);
        if (HasAnyRemainingActions())
        {
            return false;
        }

        _modelController.FadeOutActionsLock();
        _boardElementController = EBoardElementType.TILE;
        cell.FireChanged();
        return true;
    }

    public bool HasAnyRemainingActions()
    {
        return RemainingActionsForUnlock > 0;
    }

    public void EnableTileEffect(ETileEffect tileEffect)
    {
        _modelController.EnableTileEffect(tileEffect);
    }

    public void DoBounce(Vector2 direction, float power, float delay)
    {
        DOTween.Sequence()
            .SetLink(gameObject)
            .SetDelay(delay)
            .OnComplete(() =>
            {
                var centerPoint = _bouncePivotTransform.parent.TransformPoint(Vector3.zero);
                var bounceOffsetTarget = new GameObject().transform;
                _bounceTargets.Add(bounceOffsetTarget);

                DOTween.Sequence()
                    .SetLink(gameObject)
                    .Append(bounceOffsetTarget.DOPunchPosition(direction * power, 0.2f, 1).SetEase(Ease.Linear))
                    .OnUpdate(() =>
                    {
                        var maxD = -1f;
                        var offset = _bounceTargets.FindLast(t =>
                        {
                            var d = t.position.magnitude;
                            if (d >= maxD)
                            {
                                maxD = d;
                                return true;
                            }

                            return false;
                        });
                        _bouncePivotTransform.position = centerPoint + offset.position;
                    })
                    .OnComplete(() =>
                    {
                        _bounceTargets.Remove(bounceOffsetTarget);
                    });
            });
    }

    private void ResetBounceTween()
    {
    }

    private EMoveDirection GetMoveDirection(int directionIndex)
    {
        return (EMoveDirection)directionIndex;
    }

    private Vector3 GetArrowRotation(EMoveDirection moveDirection)
    {
        return new Vector3(0, 0, 90 * (int)moveDirection);
    }

    public override void OnBeforeDestroy()
    {
        _modelController.OnBeforeDestroy();

        if (HasKey)
        {
            OnKeyFound?.Invoke(this);
        }
    }
}

public enum TileState
{
    IDLE = 0,
    PRESSED = 1
}

public enum ETileEffect
{
    DIE = 0,
    JELLY = 1
}
