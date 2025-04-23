using System;
using System.Collections.Generic;
using UnityEngine;

public class GalleryDotsController : MonoBehaviour
{
    [SerializeField] private GalleryDotController dotPrefab;

    private readonly List<GalleryDotController> currentDots = new();

    public event Action<int> OnPageRequested;

    public void SetOn(int index)
    {
        for (var i = 0; i < currentDots.Count; i++)
        {
            currentDots[i].SetState(i == index);
        }
    }

    public void SetDotsCount(int count)
    {
        Clear();

        for (var i = 0; i < count; i++)
        {
            var dot = Instantiate(dotPrefab, transform);
            dot.OnClicked += () =>
            {
                OnPageRequested?.Invoke(currentDots.IndexOf(dot));
                VibrationController.Instance.Play(EVibrationType.Selection);
            };
            currentDots.Add(dot);
        }

        if (count > 0)
        {
            SetOn(0);
        }
    }

    private void Clear()
    {
        transform.ClearChildren();
        currentDots.Clear();
    }
}
