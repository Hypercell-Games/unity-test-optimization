using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GalleryDotController : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Sprite onImage;
    [SerializeField] private Sprite offImage;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnClicked?.Invoke();
    }

    public event Action OnClicked;

    public void SetState(bool isOn)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = isOn ? onImage : offImage;
    }
}
