using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIGameClickControls : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private void Start()
    {
    }

    private void OnDisable()
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public event Action<Vector2> onPointerDown;
    public event Action<Vector2> onPointerDrag;
    public event Action<Vector2> onPointerUp;

    private void OnPointerDown(Vector2 eventData)
    {
    }

    private void OnPointerUp(Vector2 eventData)
    {
    }
}
