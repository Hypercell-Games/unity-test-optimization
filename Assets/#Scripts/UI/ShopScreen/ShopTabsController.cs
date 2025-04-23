using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopTabsController : MonoBehaviour
{
    [SerializeField] private List<ShopTabLayout> tabsLayouts;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Start()
    {
        foreach (var shopTabLayout in tabsLayouts)
        {
            shopTabLayout.OnTabRequested += OnTabActivationRequested;
        }
    }

    public event Action<int> OnTabRequested;

    private void OnTabActivationRequested(ShopTabLayout tab)
    {
        OnTabRequested?.Invoke(tabsLayouts.IndexOf(tab));
    }

    public void SetActiveTab(Tabs tab)
    {
        for (var i = 0; i < tabsLayouts.Count; i++)
        {
            tabsLayouts[i].SetActive(i == (int)tab);
        }
    }

    public void SetTabNotificationActive(Tabs tab, bool isActive)
    {
        tabsLayouts[(int)tab].SetNotificationActive(isActive);
    }

    public void SetInteractable(bool isInteractable)
    {
        canvasGroup.interactable = isInteractable;
        canvasGroup.blocksRaycasts = isInteractable;
    }
}
