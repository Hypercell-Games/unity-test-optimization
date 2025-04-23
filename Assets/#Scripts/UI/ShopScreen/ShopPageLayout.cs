using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unpuzzle;

public class ShopPageLayout : MonoBehaviour
{
    [Header("Config")] [SerializeField] private int spacingWithButtons = 36;

    [SerializeField] private int spacingNoButtons = 77;

    [Header("Controls")] [SerializeField] private Button _leftArrow;

    [SerializeField] private Button _rightArrow;

    [Header("Internal references")] [SerializeField]
    private Transform _content;

    [SerializeField] private CustomScrollSnap _scroll;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _lockLabel;
    [SerializeField] private GalleryDotsController _dotsController;

    [Space(10)] [SerializeField] private GameObject _bottomButtons;

    [Header("Prefabs")] [SerializeField] private ShopSlotsGridLayout gridLayoutPrefab;

    private int _activePageIndex;
    private List<ShopSlotsGridLayout> _slotsGridPages;

    private bool _subsequentPagesLock;

    private int PagesCount => _slotsGridPages.Count;
    private ShopSlotsGridLayout ActivePage => _slotsGridPages[_activePageIndex];

    private void Awake()
    {
        _leftArrow.onClick.AddListener(() =>
        {
            ChangePageAmount(-1);
            VibrationController.Instance.Play(EVibrationType.Selection);
        });
        _rightArrow.onClick.AddListener(() =>
        {
            ChangePageAmount(1);
            VibrationController.Instance.Play(EVibrationType.Selection);
        });

        _dotsController.OnPageRequested += SetActivePage;
        _scroll.OnDragEndSnappingToPage += OnActivePageChanged;

        OnPageChanged += HandleSubsequentPagesLock;
    }

    public event Action<int> OnPageChanged;

    public void SetupContents(List<BgSkinConfig> galleryItems, Action<BgSkinConfig> onItemRequested,
        Action<BgSkinConfig> onUnlockByAd, bool bottomButtons = false)
    {
        _slotsGridPages = new List<ShopSlotsGridLayout>();
        _content.ClearChildren();
        _subsequentPagesLock = false;


        _bottomButtons.SetActive(bottomButtons);

        var itemChunks = galleryItems.ChunkBy(ShopSlotsGridLayout.SLOTS_PER_PAGE);
        foreach (var itemChunk in itemChunks)
        {
            var slotsGridPage = Instantiate(gridLayoutPrefab, _content);
            slotsGridPage.SetupContents(itemChunk, onItemRequested, onUnlockByAd);
            slotsGridPage.GetComponent<VerticalLayoutGroup>().spacing =
                bottomButtons ? spacingWithButtons : spacingNoButtons;

            _slotsGridPages.Add(slotsGridPage);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_content.GetComponent<RectTransform>());

        _scroll.Reset();
        _dotsController.SetDotsCount(PagesCount);

        OnActivePageChanged(_activePageIndex = 0);
    }

    public void SetUseGrayScaleForDisabled(bool useGrayScale)
    {
        _slotsGridPages.ForEach(r => r.SetUseGrayScaleForDisabled(useGrayScale));
    }

    public int CountLockedItemsCurrentPage()
    {
        return ActivePage.CountLockedItems();
    }

    public bool IsItemAvailableOnCurrentPage(BgSkinConfig item)
    {
        return ActivePage.HasItem(item);
    }

    public void SetSelected(BgSkinConfig item, bool skipAnim = false)
    {
        var firstPageWithItem = _slotsGridPages.FirstOrDefault(r => r.HasItem(item));

        if (ActivePage != firstPageWithItem)
        {
            SetActivePage(Math.Max(0, _slotsGridPages.IndexOf(firstPageWithItem)));
        }

        _slotsGridPages.ForEach(r => r.SetSelected(item, skipAnim));
    }

    public bool HasSelected()
    {
        return _slotsGridPages.Any(r => r.HasSelected());
    }

    public IEnumerator CO_UnlockRandomWithAnimation(BgSkinConfig preferredSkin,
        Action<BgSkinConfig> onResult = null)
    {
        yield return ActivePage.CO_UnlockRandomWithAnimation(preferredSkin, onResult);
    }

    public void RefreshStates()
    {
        _slotsGridPages.ForEach(r => r.RefreshStates());
    }

    private void ChangePageAmount(int amount)
    {
        SetActivePage(_activePageIndex + amount);
    }

    public void SetActivePage(int index)
    {
        _scroll.SnapToIndex(index);
        OnActivePageChanged(index);
    }

    private void OnActivePageChanged(int index)
    {
        _dotsController.SetOn(index);

        _leftArrow.interactable = index != 0;
        _rightArrow.interactable = index != PagesCount - 1;

        _activePageIndex = index;

        OnPageChanged?.Invoke(index);
    }

    private void HandleSubsequentPagesLock(int pageIndex)
    {
        if (!_subsequentPagesLock)
        {
            return;
        }

        var showMessage = ShouldShowPageLockMessage(pageIndex);
        _lockLabel.gameObject.SetActive(showMessage);
        _bottomButtons.SetActive(!showMessage);
    }

    private bool ShouldShowPageLockMessage(int pageIndex)
    {
        if (!_subsequentPagesLock)
        {
            return false;
        }

        return pageIndex != 0 && _slotsGridPages[pageIndex - 1].CountLockedItems() > 0;
    }

    public void SetSubsequentPagesLocked(bool isLocked, string lockMessage = null)
    {
        _subsequentPagesLock = isLocked;
        _lockLabel.text = lockMessage;

        OnActivePageChanged(_activePageIndex);
    }

    public void SetBottomMessage(string bottomMessage)
    {
        _lockLabel.gameObject.SetActive(bottomMessage != null);
        _lockLabel.text = bottomMessage;
    }
}
