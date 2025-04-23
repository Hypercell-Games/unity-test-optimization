using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;
using UnityEngine.UI;
using Unpuzzle;
using Unpuzzle.Game;
using Unpuzzle.UI.NewTabBar;

public class UIShopScreen : BaseScreen
{
    [Space] [SerializeField] private GameData _gameData;

    [Space] [SerializeField] private CoinsBuyButton _randomButton;

    [SerializeField] private TextButtonController _getCoinsButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private ShopTabsController _tabs;
    [SerializeField] private bool _showAtStart;

    [Header("Pages")] [SerializeField] private ShopPageLayout _pageLayout;

    private Tabs _currentTab = Tabs.None;

    private void Start()
    {
        _tabs.OnTabRequested += SetActiveTab;
        _pageLayout.OnPageChanged += OnPageChanged;

        if (_showAtStart)
        {
            Show(true);
        }
    }

    private void OnEnable()
    {
        _randomButton.OnBought += RandomBought;
        _getCoinsButton.onButtonClicked += GetCoinsClicked;
        _backButton.onClick.AddListener(BackClicked);
    }

    private void OnDisable()
    {
        _randomButton.OnBought -= RandomBought;
        _getCoinsButton.onButtonClicked -= GetCoinsClicked;
        _backButton.onClick.RemoveListener(BackClicked);
    }

    public event Action OnHide;

#if UNITY_EDITOR
    [ContextMenu("SetSkins")]
    private void SetSkins()
    {
        for (var i = 0; i < 17; i++)
        {
            PlayerPrefs.SetInt($"skins.unlocked.BgSkin{i + 1}", 1);
        }
    }
#endif

    private void OnPageChanged(int page)
    {
        RefreshBuyRandomButtonState();
    }

    private void SetActiveTab(int index)
    {
        _currentTab = (Tabs)index;

        _tabs.SetActiveTab(_currentTab);

        SetupSkins(_currentTab);
        UpdateButtons();

        _pageLayout.RefreshStates();
    }

    private int GetPageWithSelection()
    {
        return 0;
    }

    private void SetupSkins(Tabs tab)
    {
        _pageLayout.SetupContents(GetRandomSkins(true), SetSelectedRequest, UnlockByAd, true);
        _pageLayout.SetSubsequentPagesLocked(false);

        SetSelected(_gameData.GetSelectedSkin());
    }

    private void UpdateButtons()
    {
        _randomButton.SetCost(GetNextRandomSkinPrice());
        _getCoinsButton.SetText($"+{GameConfig.RemoteConfig.shopFreeCoinsAmount}");

        _tabs.SetTabNotificationActive(Tabs.Random, _currentTab != Tabs.Random && CanBuyNewRandom());
        _tabs.SetTabNotificationActive(Tabs.ByLevels, _currentTab != Tabs.ByLevels && CanUnlockAnyLevelSkin());
        _tabs.SetTabNotificationActive(Tabs.ByChests, false);
    }

    private int GetNextRandomSkinPrice()
    {
        var randomSkins = _gameData.SkinsConfig.BgSkins;
        var unlockedCount = randomSkins.Count(r => r.IsUnlocked());
        var randomSkinPrice = GameLogicUtil.GetShopRandomSkinPrice(Mathf.Max(0, unlockedCount - 1));
        return randomSkinPrice;
    }

    private List<BgSkinConfig> GetRandomSkins(bool inShopOrder = false)
    {
        return _gameData.SkinsConfig.BgSkins;
    }

    private List<BgSkinConfig> GetLevelSkins()
    {
        return _gameData.GetSkins();
    }

    private void UnlockByAd(BgSkinConfig item)
    {
        HyperKit.Ads.ShowRewardedAd("ad_rewarded_skin_shop", success =>
        {
            if (!success)
            {
                return;
            }

            item.Unlock();


            SetSelected(item);

            if (NewTabBarScreen.Instance)
            {
                NewTabBarScreen.Instance.UpdateButtonsStates();
            }
        });
    }

    private void SetSelectedRequest(BgSkinConfig obj)
    {
        if (!_canvasGroup.interactable)
        {
            return;
        }

        SetSelected(obj);
    }

    private void SetSelectedNoAnimation(BgSkinConfig item)
    {
        SetSelected(item, true);
    }

    private void SetSelected(BgSkinConfig item, bool skipAnim = false)
    {
        _pageLayout.SetSelected(item, skipAnim);
        _gameData.SetSkin(item);
    }

    private void BackClicked()
    {
        Hide();
    }

    private void GetCoinsClicked()
    {
        if (!_canvasGroup.interactable)
        {
            return;
        }

        HyperKit.Ads.ShowRewardedAd("ad_rewarded_shop_coins",
            OnCoinsReward);

        void OnCoinsReward(bool success)
        {
            if (!success)
            {
                return;
            }

            _gameData.SoftCurrency.ApplyChange(GameConfig.RemoteConfig.shopFreeCoinsAmount);
        }
    }

    private void RandomBought(int cost)
    {
        RefreshBuyRandomButtonState();

        StartCoroutine(CO_RandomBuy());
    }

    private IEnumerator CO_RandomBuy()
    {
        OnAnimationStart();

        var unlockedSkin = GetNextRandoSkin();
        yield return _pageLayout.CO_UnlockRandomWithAnimation(unlockedSkin, SetSelectedNoAnimation);


        UpdateButtons();

        OnAnimationEnd();
    }

    private BgSkinConfig GetNextRandoSkin()
    {
        var skinNames = GameLogicUtil.SplitString(GameConfig.RemoteConfig.randomSkinsUnlockOrder);

        foreach (var skinName in skinNames)
        {
            var skin = _gameData.SkinsConfig.GetRandomSkinToUnlock();
            if (!_pageLayout.IsItemAvailableOnCurrentPage(skin))
            {
                continue;
            }

            if (!skin.IsUnlocked())
            {
                return skin;
            }
        }

        return null;
    }

    public override void Show(bool force = false, Action callback = null)
    {
        base.Show(force, callback);

        SetActiveTab(GetPageWithSelection());

        if (gameObject.scene.name == Scenes.GAME)
        {
            PauseUtil.BossLevelPaused();
        }
    }

    public override void Hide(bool force = false, Action callback = null)
    {
        OnHide?.Invoke();

        callback += () =>
        {
            if (gameObject.scene.name == Scenes.GAME)
            {
                PauseUtil.BossLevelUnpaused();
            }
        };
        base.Hide(force, callback);
    }

    private void OnAnimationStart()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = true;

        _randomButton.SetForceDisabled(true);
        _getCoinsButton.SetDisabled(false);
        _tabs.SetInteractable(false);
    }

    private void OnAnimationEnd()
    {
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        _randomButton.SetForceDisabled(false);
        _getCoinsButton.SetDisabled(true);
        _tabs.SetInteractable(true);

        RefreshBuyRandomButtonState();
    }

    private void RefreshBuyRandomButtonState()
    {
        var lockedItemsCount = _pageLayout.CountLockedItemsCurrentPage();

        _randomButton.SetForceDisabled(lockedItemsCount <= 0);
    }

    public bool CanBuyNewRandom()
    {
        var skinsLeft = GetRandomSkins().Count(r => r.IsIntroduced && !r.IsUnlocked());
        return skinsLeft > 0;
    }

    public bool CanUnlockAnyLevelSkin()
    {
        return GetLevelSkins().Any(r => !r.IsUnlocked());
    }
}

public enum Tabs
{
    None = -1,
    Random = 0,
    ByLevels = 1,
    ByChests = 2
}
