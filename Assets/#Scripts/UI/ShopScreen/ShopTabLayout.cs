using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopTabLayout : MonoBehaviour, IPointerClickHandler
{
    [Header("Config")] [SerializeField] private Sprite activeIconSprite;

    [SerializeField] private Sprite inactiveIconSprite;

    [Header("References")] [SerializeField]
    private Sprite activeTabSprite;

    [SerializeField] private Sprite inactiveTabSprite;

    [Header("Internal references")] [SerializeField]
    private Image tabImage;

    [SerializeField] private Image iconImage;
    [SerializeField] private Image notificationImage;

    private void Start()
    {
        DOTween.Sequence(notificationImage).SetLink(gameObject)
            .Append(notificationImage.transform.DOScale(1.1f, 0.4f).SetEase(Ease.InOutSine))
            .Append(notificationImage.transform.DOScale(1f, 0.6f).SetEase(Ease.InOutSine))
            .SetLoops(-1);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnTabRequested?.Invoke(this);
    }

    public event Action<ShopTabLayout> OnTabRequested;

    public void SetActive(bool isActive)
    {
        tabImage.sprite = isActive ? activeTabSprite : inactiveTabSprite;
        iconImage.sprite = isActive ? activeIconSprite : inactiveIconSprite;
        iconImage.SetNativeSize();
    }

    public void SetNotificationActive(bool isActive)
    {
        notificationImage.gameObject.SetActive(isActive);
    }
}
