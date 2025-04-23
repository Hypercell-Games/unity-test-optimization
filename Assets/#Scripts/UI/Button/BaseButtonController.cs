using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public abstract class BaseButtonController : MonoBehaviour, IButtonController
{
    [Header("EventTrigger")] [SerializeField]
    private EventTrigger _eventTrigger;

    [Header("Image")] [SerializeField] protected Image _clickedImage;

    [Header("Sprite")] [SerializeField] protected Sprite _unPressedSprite;

    [SerializeField] protected Sprite _pressedSprite;

    [Header("Internal")] [SerializeField] protected CanvasGroup _canvasGroup;

    private bool disabled = false;

    private void Awake()
    {
        var pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener(e => OnButtonPressed());
        _eventTrigger.triggers.Add(pointerDown);

        pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerClick;
        pointerDown.callback.AddListener(e => OnButtonClicked());
        _eventTrigger.triggers.Add(pointerDown);

        pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerUp;
        pointerDown.callback.AddListener(e => OnButtonUnPressed());

        _eventTrigger.triggers.Add(pointerDown);
    }

    public event Action onButtonClicked;
    public event Action onButtonPressed;
    public event Action onButtonUnPressed;

    public void OnButtonClicked()
    {
        OnButtonClick();
    }

    public void OnButtonPressed()
    {
        OnButtonPress();
    }

    public void OnButtonUnPressed()
    {
        OnButtonUnPress();
    }

    public void SetOnButtonClick(Action action)
    {
        onButtonClicked = action;
    }

    protected virtual void OnButtonPress()
    {
        onButtonPressed?.Invoke();

        VibrationController.Instance?.Play(EVibrationType.LightImpact);
        _clickedImage.sprite = _pressedSprite;
    }

    protected virtual void OnButtonClick()
    {
        onButtonClicked?.Invoke();
    }

    protected virtual void OnButtonUnPress()
    {
        onButtonUnPressed?.Invoke();

        _clickedImage.sprite = _unPressedSprite;
    }

    public void SetDisabled(bool isAvailable)
    {
        SetInteractable(isAvailable);

        _canvasGroup.alpha = isAvailable ? 1 : 0.5f;
    }

    public void SetInteractable(bool isAvailable)
    {
        _clickedImage.raycastTarget = isAvailable;
        _canvasGroup.blocksRaycasts = isAvailable;
    }

    public bool IsDisabled()
    {
        return _clickedImage.raycastTarget;
    }

    public void SetVisible(bool isVisible)
    {
        _canvasGroup.alpha = isVisible ? 1 : 0;
        _canvasGroup.interactable = isVisible;
        _canvasGroup.blocksRaycasts = isVisible;
    }

    public void Show(bool scaleIn = false, float delay = 0, float scaleDuration = 1)
    {
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(1f, 1f * scaleDuration).SetDelay(delay);
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        if (scaleIn)
        {
            transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            transform.DOScale(1, 1f * scaleDuration).SetEase(Ease.OutElastic).SetDelay(delay);
        }
    }

    public void Hide()
    {
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutSine);
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}
