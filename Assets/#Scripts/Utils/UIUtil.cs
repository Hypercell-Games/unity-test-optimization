using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UIUtil
{
    public static bool IsPointerOverUIObject(Vector2 pointerPosition)
    {
        var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = pointerPosition;
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
