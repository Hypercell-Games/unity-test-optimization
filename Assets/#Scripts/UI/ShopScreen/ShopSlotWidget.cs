using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unpuzzle;

public class ShopSlotWidget : MonoBehaviour, IPointerClickHandler
{
    public enum State
    {
        Selected,
        Available,
        Locked,
        LockedAd,
        LockedActive,
        HiddenSlot
    }

    private static readonly int GrayscaleAmountID = Shader.PropertyToID("_GrayscaleAmount");

    [Header("Internal references")] [SerializeField]
    private TextButtonController getButton;

    [SerializeField] private Image checkmarkImage;
    [SerializeField] private Image contentImage;
    [SerializeField] private Image bgImage;

    [SerializeField] private Image bgSelected;
    [SerializeField] private Image bgUnselected;
    [SerializeField] private Image bgLockSelected;
    [SerializeField] private GameObject questionmark;

    [SerializeField] private CanvasGroup canvasGroup;

    private Tween denyTween;

    public BgSkinConfig CurrentItem { get; private set; }
    public State CurrentState { get; private set; }
    public bool IsSelected { get; private set; }

    public bool UseGrayScaleForDisabled { get; set; }

    private void Start()
    {
        DOTween.Sequence(getButton).SetLink(gameObject)
            .Append(getButton.transform.DOScale(1.1f, 0.4f).SetEase(Ease.InOutSine))
            .Append(getButton.transform.DOScale(1f, 0.6f).SetEase(Ease.InOutSine))
            .SetLoops(-1);
    }

    private void OnEnable()
    {
        getButton.onButtonClicked += GetByAdRequested;
    }

    private void OnDisable()
    {
        getButton.onButtonClicked -= GetByAdRequested;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CurrentState != State.Available)
        {
            Deny();
            return;
        }

        OnSelectionRequest?.Invoke(CurrentItem);

        VibrationController.Instance.Play(EVibrationType.Success);
    }

    public event Action<BgSkinConfig> OnGetByAdRequest;
    public event Action<BgSkinConfig> OnSelectionRequest;

    public void SetContent(BgSkinConfig item)
    {
        contentImage.material = Instantiate(contentImage.material);

        CurrentItem = item;

        SetThumb(item);
        SetSelected(false);
        UpdateState();
    }

    public void SetState(State state)
    {
        if (CurrentItem == null)
        {
            state = State.HiddenSlot;
        }

        CurrentState = state;

        canvasGroup.alpha = state == State.HiddenSlot ? 0 : 1;

        if (state == State.HiddenSlot)
        {
            return;
        }

        bgSelected.gameObject.SetActive(state == State.Selected);
        if (bgUnselected)
        {
            bgUnselected.gameObject.SetActive(state != State.Selected && !IsDisabledSprite(state));
        }

        bgLockSelected.gameObject.SetActive(state == State.LockedActive);

        checkmarkImage.gameObject.SetActive(state == State.Selected);
        getButton.gameObject.SetActive(state == State.LockedAd);

        var disabledState = IsDisabledSprite(state);
        questionmark.SetActive(disabledState && !UseGrayScaleForDisabled);
        contentImage.gameObject.SetActive(!disabledState || UseGrayScaleForDisabled);
        SetImageGrayscale(IsDisabledSprite(state) ? 1 : 0);
    }

    public void SetSelected(bool isSelected, bool skipAnim = false)
    {
        IsSelected = isSelected;

        if (isSelected && !skipAnim)
        {
            ThrobSmall();
        }

        UpdateState();
    }

    public void SetLockedSelected(bool isSelected)
    {
        IsSelected = isSelected;

        UpdateState();
    }

    private void SetThumb(BgSkinConfig item)
    {
        contentImage.sprite = item.Thumb;
    }

    private void GetByAdRequested()
    {
        OnGetByAdRequest?.Invoke(CurrentItem);
    }

    private void SetImageGrayscale(float amount)
    {
        contentImage.material.SetFloat(GrayscaleAmountID, amount);
    }

    private bool IsDisabledSprite(State state)
    {
        return state == State.LockedActive || state == State.Locked;
    }

    public void UpdateState()
    {
        SetState(GetItemState(CurrentItem));
    }

    private State GetItemState(BgSkinConfig item)
    {
        if (item == null)
        {
            return State.HiddenSlot;
        }

        if (IsSelected)
        {
            return State.Selected;
        }

        if (item.IsUnlocked())
        {
            return State.Available;
        }

        if (item.IsIntroduced)
        {
            return State.LockedAd;
        }

        return State.Locked;
    }

    public void UnlockItem()
    {
        if (CurrentItem.IsUnlocked())
        {
            return;
        }

        UpdateState();

        Throb();
    }

    public void ThrobSmall(float scaleDuration = 1)
    {
        DOTween.Kill(this, true);

        DOTween.Sequence()
            .Append(transform.DOScale(1.05f, 0.1f * scaleDuration).SetEase(Ease.OutSine))
            .Append(transform.DOScale(1f, 0.2f * scaleDuration).SetEase(Ease.InSine))
            .SetLink(gameObject);
    }

    private void Throb()
    {
        DOTween.Kill(this, true);

        DOTween.Sequence()
            .Append(transform.DOScale(1.3f, 0.15f).SetEase(Ease.OutSine))
            .Append(transform.DOScale(1f, 0.3f).SetEase(Ease.InSine))
            .SetLink(gameObject);

        DOTween.Sequence()
            .Append(transform.DOLocalRotate(new Vector3(0, 0, -10), 0.15f).SetEase(Ease.OutSine))
            .Append(transform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.InSine))
            .SetLink(gameObject);
    }

    private void Deny()
    {
        denyTween?.Kill(true);
        denyTween = transform.DOPunchPosition(new Vector3(10, 0, 0), 0.3f);

        VibrationController.Instance.Play(EVibrationType.Failure);
    }

    public bool IsLocked()
    {
        return !(CurrentState == State.Selected || CurrentState == State.Available || CurrentState == State.HiddenSlot);
    }

    public bool IsHidden()
    {
        return CurrentState == State.HiddenSlot;
    }
}
