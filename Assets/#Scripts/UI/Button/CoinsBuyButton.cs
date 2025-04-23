using System;
using TMPro;
using UnityEngine;

public class CoinsBuyButton : TextButtonController
{
    [Header("Buy button config")] [SerializeField]
    private IntVariable currency;

    [SerializeField] private int cost;

    [Header("Internal references")] [SerializeField]
    private TextMeshProUGUI label;

    private bool forceDisabled;

    private void OnEnable()
    {
        onButtonClicked += TryBuy;
        currency.onChange.action += UpdateState;

        UpdateState(currency.Value);
    }

    private void OnDisable()
    {
        onButtonClicked -= TryBuy;
        currency.onChange.action -= UpdateState;
    }

    public event Action<int> OnBought;

    public void SetCost(int cost)
    {
        this.cost = cost;

        label.SetText(cost.ToString());

        UpdateState(currency.Value);
    }

    private void UpdateState(int coins)
    {
        var shouldEnable = coins >= cost && !forceDisabled;

        SetDisabled(shouldEnable);
    }

    public void SetForceDisabled(bool disable)
    {
        forceDisabled = disable;

        UpdateState(currency.Value);
    }

    private void TryBuy()
    {
        if (currency.Value < cost)
        {
            return;
        }

        currency.ApplyChange(-cost);

        OnBought?.Invoke(cost);
    }
}
