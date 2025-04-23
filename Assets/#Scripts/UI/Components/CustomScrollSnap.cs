using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(ScrollRect))]
public class CustomScrollSnap : UIBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] public int startingIndex;
    [SerializeField] public bool wrapAround;
    [SerializeField] public float lerpTimeMilliSeconds = 200f;
    [SerializeField] public float triggerPercent = 5f;
    [Range(0f, 10f)] public float triggerAcceleration = 1f;

    [Space(10)] [SerializeField] public bool skipInitOnAwake;

    int actualIndex;
    CanvasGroup canvasGroup;
    int cellIndex;
    Vector2 cellSize;
    RectTransform content;
    bool indexChangeTriggered;
    bool isLerping;
    DateTime lerpStartedAt;
    public OnLerpCompleteEvent onLerpComplete;
    public OnReleaseEvent onRelease = new();
    Vector2 releasedPosition;
    ScrollRect scrollRect;
    Vector2 targetPosition;

    public int CurrentIndex
    {
        get
        {
            var count = LayoutElementCount();
            var mod = actualIndex % count;
            return mod >= 0 ? mod : count + mod;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (!skipInitOnAwake)
        {
            Reset();
        }
    }

    public void Reset()
    {
        if (scrollRect == null)
        {
            Init();
        }

        actualIndex = startingIndex;
        cellIndex = startingIndex;
        OnCellSizeChanged();
    }

    void LateUpdate()
    {
        if (isLerping)
        {
            LerpToElement();
            if (ShouldStopLerping())
            {
                isLerping = false;
                canvasGroup.blocksRaycasts = true;
                onLerpComplete.Invoke();
                onLerpComplete.RemoveListener(WrapElementAround);
            }
        }
    }

    public void OnDrag(PointerEventData data)
    {
        var dx = data.delta.x;
        var dt = Time.deltaTime * 1000f;
        var acceleration = Mathf.Abs(dx / dt);
        if (acceleration > triggerAcceleration && acceleration != Mathf.Infinity)
        {
            indexChangeTriggered = true;
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (IndexShouldChangeFromDrag(data))
        {
            var direction = data.pressPosition.x - data.position.x > 0f ? 1 : -1;
            var index = cellIndex + direction * CalculateScrollingAmount(data);

            var snappedIndex = SnapToIndex(index);
            OnDragEndSnappingToPage?.Invoke(snappedIndex);
        }
        else
        {
            StartLerping();
        }
    }

    public event Action<int> OnDragEndSnappingToPage;

    private void Init()
    {
        onLerpComplete = new OnLerpCompleteEvent();
        scrollRect = GetComponent<ScrollRect>();
        canvasGroup = GetComponent<CanvasGroup>();
        content = scrollRect.content;
    }

    public void OnCellSizeChanged()
    {
        if (content == null)
        {
            return;
        }

        if (content.TryGetComponent<GridLayoutGroup>(out var gridLayout))
        {
            cellSize = gridLayout.cellSize;
        }
        else
        {
            foreach (Transform child in content)
            {
                if (!child.TryGetComponent<RectTransform>(out var rect))
                {
                    continue;
                }


                cellSize = new Vector2(rect.rect.width, rect.rect.height);
            }
        }

        content.anchoredPosition = new Vector2(-cellSize.x * cellIndex, content.anchoredPosition.y);

        var count = LayoutElementCount();


        if (startingIndex < count)
        {
            MoveToIndex(startingIndex);
        }
    }

    public void PushLayoutElement(LayoutElement element)
    {
        element.transform.SetParent(content.transform, false);
        SetContentSize(LayoutElementCount());
    }

    public void PopLayoutElement()
    {
        var elements = content.GetComponentsInChildren<LayoutElement>();
        Destroy(elements[elements.Length - 1].gameObject);
        SetContentSize(LayoutElementCount() - 1);
        if (cellIndex == CalculateMaxIndex())
        {
            cellIndex -= 1;
        }
    }

    public void UnshiftLayoutElement(LayoutElement element)
    {
        cellIndex += 1;
        element.transform.SetParent(content.transform, false);
        element.transform.SetAsFirstSibling();
        SetContentSize(LayoutElementCount());
        content.anchoredPosition = new Vector2(content.anchoredPosition.x - cellSize.x, content.anchoredPosition.y);
    }

    public void ShiftLayoutElement()
    {
        Destroy(GetComponentInChildren<LayoutElement>().gameObject);
        SetContentSize(LayoutElementCount() - 1);
        cellIndex -= 1;
        content.anchoredPosition = new Vector2(content.anchoredPosition.x + cellSize.x, content.anchoredPosition.y);
    }

    public int LayoutElementCount()
    {
        return content.GetComponentsInChildren<LayoutElement>(false)
            .Count(e => e.transform.parent == content);
    }

    public int CalculateScrollingAmount(PointerEventData data)
    {
        var offset = scrollRect.content.anchoredPosition.x + cellIndex * cellSize.x;
        var normalizedOffset = Mathf.Abs(offset / cellSize.x);
        var skipping = (int)Mathf.Floor(normalizedOffset);
        if (skipping == 0)
        {
            return 1;
        }

        if ((normalizedOffset - skipping) * 100f > triggerPercent)
        {
            return skipping + 1;
        }

        return skipping;
    }

    public void SnapToNext()
    {
        SnapToIndex(cellIndex + 1);
    }

    public void SnapToPrev()
    {
        SnapToIndex(cellIndex - 1);
    }

    public int SnapToIndex(int newCellIndex)
    {
        var maxIndex = CalculateMaxIndex();
        if (wrapAround && maxIndex > 0)
        {
            actualIndex += newCellIndex - cellIndex;
            cellIndex = newCellIndex;
            onLerpComplete.AddListener(WrapElementAround);
        }
        else
        {
            newCellIndex = Mathf.Clamp(newCellIndex, 0, maxIndex);
            actualIndex += newCellIndex - cellIndex;
            cellIndex = newCellIndex;
        }

        onRelease.Invoke(cellIndex);
        StartLerping();

        return cellIndex;
    }

    public void MoveToIndex(int newCellIndex)
    {
        var maxIndex = CalculateMaxIndex();
        if (newCellIndex >= 0 && newCellIndex <= maxIndex)
        {
            actualIndex += newCellIndex - cellIndex;
            cellIndex = newCellIndex;
        }

        onRelease.Invoke(cellIndex);

        content.anchoredPosition = CalculateTargetPosition(cellIndex);
    }

    void StartLerping()
    {
        releasedPosition = content.anchoredPosition;
        targetPosition = CalculateTargetPosition(cellIndex);
        lerpStartedAt = DateTime.Now;
        canvasGroup.blocksRaycasts = false;
        isLerping = true;
    }

    int CalculateMaxIndex()
    {
        var cellPerFrame = Mathf.Max(1,
            Mathf.FloorToInt(scrollRect.GetComponent<RectTransform>().rect.size.x / cellSize.x));
        return LayoutElementCount() - cellPerFrame;
    }

    bool IndexShouldChangeFromDrag(PointerEventData data)
    {
        if (indexChangeTriggered)
        {
            indexChangeTriggered = false;
            return true;
        }

        var offset = scrollRect.content.anchoredPosition.x + cellIndex * cellSize.x;
        var normalizedOffset = Mathf.Abs(offset / cellSize.x);
        return normalizedOffset * 100f > triggerPercent;
    }

    void LerpToElement()
    {
        var t = (float)((DateTime.Now - lerpStartedAt).TotalMilliseconds / lerpTimeMilliSeconds);
        var newX = Mathf.Lerp(releasedPosition.x, targetPosition.x, t);
        content.anchoredPosition = new Vector2(newX, content.anchoredPosition.y);
    }

    void WrapElementAround()
    {
        if (cellIndex <= 0)
        {
            var elements = content.GetComponentsInChildren<LayoutElement>();
            elements[elements.Length - 1].transform.SetAsFirstSibling();
            cellIndex += 1;
            content.anchoredPosition = new Vector2(content.anchoredPosition.x - cellSize.x, content.anchoredPosition.y);
        }
        else if (cellIndex >= CalculateMaxIndex())
        {
            var element = content.GetComponentInChildren<LayoutElement>();
            element.transform.SetAsLastSibling();
            cellIndex -= 1;
            content.anchoredPosition = new Vector2(content.anchoredPosition.x + cellSize.x, content.anchoredPosition.y);
        }
    }

    void SetContentSize(int elementCount)
    {
    }

    private Vector2 CalculateTargetPosition(int index)
    {
        var rectTransform = content.transform.GetChild(index).GetComponent<RectTransform>();
        return new Vector2(-rectTransform.anchoredPosition.x + rectTransform.sizeDelta.x / 2f,
            content.anchoredPosition.y);
    }

    bool ShouldStopLerping()
    {
        return Mathf.Abs(content.anchoredPosition.x - targetPosition.x) < 0.001;
    }

    public class OnLerpCompleteEvent : UnityEvent
    {
    }

    public class OnReleaseEvent : UnityEvent<int>
    {
    }
}
