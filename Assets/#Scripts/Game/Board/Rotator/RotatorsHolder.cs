using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RotatorsHolder : MonoBehaviour
{
    [Header("Prefab")] [SerializeField] private Transform _dotPrefab;

    [Header("Transforms")] [SerializeField]
    private Transform _groupTransform;

    [SerializeField] private Transform _rotatorTransform;
    [SerializeField] private Transform _lineTransform;

    [Header("Sort")] [SerializeField] private List<SpriteRenderer> _sprites;

    private int _sortOrder;

    public List<RotatorGroup> RotatorsGroup = new();
    private int rotatorsStartCount;
    public Transform RotatorTransform => _rotatorTransform;
    public List<Transform> DotsTransforms { get; set; } = new();
    public int Direction { get; set; }
    public float ScaleMultiplayer { get; set; } = 2;
    public int Length { get; set; } = 1;
    private float _targetLineScaleX => Length * ScaleMultiplayer;
    public event Action onHolderClear;
    public event Action<RotatorsHolder> onDestroyHolder;

    public void Initialize()
    {
    }

    public void FillInfo(GridCell gridCell, int levelId)
    {
        if (gridCell.BaseBoardElementController == null)
        {
            Destroy(DotsTransforms[levelId].gameObject);
            return;
        }

        var newGroup = new RotatorGroup
        {
            GridLevel = levelId, Cell = gridCell, DotTransform = DotsTransforms[levelId]
        };

        RotatorsGroup.Add(newGroup);

        newGroup.Cell.BaseBoardElementController.onActionWithElement += OnTiledAction;
        rotatorsStartCount++;
    }

    public void OnTiledAction(BaseBoardElementController boardElement, EActionWithElement actionWithElement)
    {
        var element = GetRotatorGroup(boardElement);
        DestroyDotTransform(element);
        boardElement.onActionWithElement -= OnTiledAction;
    }

    public RotatorGroup GetRotatorGroup(BaseBoardElementController boardElement)
    {
        return RotatorsGroup.Find(x => x.Cell.BaseBoardElementController == boardElement);
    }

    public RotatorGroup GetRotatorGroup(int gridLevel)
    {
        var rotatorGroup = RotatorsGroup.Find(x => x.GridLevel == gridLevel);
        return rotatorGroup;
    }

    public void ChangeDirection(int multiplayer)
    {
        for (var i = 0; i < multiplayer; i++)
        {
            Direction -= 1;
            if (Direction == -1)
            {
                Direction = 3;
            }
        }
    }

    public RotatorGroup GetGroupByLevelId(int levelId)
    {
        foreach (var rotatorGroup in RotatorsGroup)
        {
            if (rotatorGroup.GridLevel == levelId)
            {
                return rotatorGroup;
            }
        }

        return null;
    }

    public void DestroyDotTransform(RotatorGroup rotatorGroup)
    {
        if (rotatorGroup != null)
        {
            if (rotatorGroup.GridLevel == rotatorsStartCount - 1)
            {
                Length--;
                ChangeLineScale();
            }

            Destroy(rotatorGroup.DotTransform.gameObject);

            RotatorsGroup.Remove(rotatorGroup);
            if (RotatorsGroup.Count == 0)
            {
                _lineTransform.gameObject.SetActive(false);
                onDestroyHolder?.Invoke(this);
            }
        }
    }

    public void SetSpriteOrders(int sortOrder)
    {
        _sortOrder = sortOrder;
        for (var i = 0; i < _sprites.Count; i++)
        {
            _sprites[i].sortingOrder = sortOrder + 1;
        }

        DotsTransforms.ForEach(d => d.GetComponent<SpriteRenderer>().sortingOrder = sortOrder + 2);
    }

    public void SetupLineScale()
    {
        ChangeLineScale();
        InstantiateDots();
        ChangeHolderDirection();
    }

    public void ChangeLineScale(bool force = true)
    {
        var targetScale = new Vector2(_targetLineScaleX, _lineTransform.localScale.y);

        if (force)
        {
            _lineTransform.localScale = targetScale;
        }
        else
        {
            _lineTransform.DOScale(targetScale, 0.25f).SetLink(_lineTransform.gameObject);
            for (var i = 0; i < DotsTransforms.Count; i++)
            {
                var dotsTransform = DotsTransforms[i];
                if (dotsTransform != null)
                {
                    dotsTransform
                        .DOLocalMove(new Vector2(dotsTransform.localPosition.x, ScaleMultiplayer * (i + 1)), 0.25f)
                        .SetLink(dotsTransform.gameObject);
                }
            }
        }
    }

    private void ChangeHolderDirection()
    {
        RotatorTransform.localEulerAngles = GetArrowDirection(Direction);
    }

    private Vector3 GetArrowDirection(int directionInt)
    {
        return new Vector3(0f, 0f, 90f * directionInt);
    }

    private void InstantiateDots()
    {
        for (var i = 1; i <= Length; i++)
        {
            var dotTransform = Instantiate(_dotPrefab, _groupTransform);
            dotTransform.localPosition = i * ScaleMultiplayer * Vector2.up;
            DotsTransforms.Add(dotTransform);
        }

        SetSpriteOrders(_sortOrder);
    }
}

public class RotatorGroup
{
    public GridCell Cell;
    public Transform DotTransform;
    public int GridLevel;
}
