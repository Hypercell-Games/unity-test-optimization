using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelCompleteButtonsController : MonoBehaviour
{
    public enum Layout
    {
        Bonus,
        Single,
        Next
    }

    [Header("Internal")] [SerializeField] private GameObject getTripleRewardGroup;

    [SerializeField] private GameObject getNormalRewardGroup;

    [Space] [SerializeField] private TextButtonController tripleButtonGet3x;

    [SerializeField] private TextButtonController tripleButtonGetNormal;
    [SerializeField] private TextButtonController normalButton;

    private void OnEnable()
    {
        tripleButtonGet3x.onButtonClicked += OnBonusRequested;
        tripleButtonGetNormal.onButtonClicked += OnNormalRequested;
        normalButton.onButtonClicked += OnNormalRequested;
    }

    private void OnDisable()
    {
        tripleButtonGet3x.onButtonClicked -= OnBonusRequested;
        tripleButtonGetNormal.onButtonClicked -= OnNormalRequested;
        normalButton.onButtonClicked -= OnNormalRequested;
    }

    public event Action OnNormalRequested;
    public event Action OnBonusRequested;

    private IEnumerable<TextButtonController> GetButtons()
    {
        yield return tripleButtonGet3x;
        yield return tripleButtonGetNormal;
        yield return normalButton;
    }

    public void SetLayout(Layout layout)
    {
        getTripleRewardGroup.SetActive(layout == Layout.Bonus || layout == Layout.Single);
        getNormalRewardGroup.SetActive(layout == Layout.Next);

        tripleButtonGet3x.gameObject.SetActive(layout == Layout.Bonus);
    }

    public void SetNormalAmountLabel(int amount)
    {
        tripleButtonGetNormal.SetText(amount.ToString());
    }

    public void SetBonusAmountLabel(int amount)
    {
        tripleButtonGet3x.SetText(amount.ToString());
    }

    public void SetInteractable(bool isInteractable)
    {
        Array.ForEach(GetButtons().ToArray(), r => r.SetInteractable(isInteractable));
    }

    public void SetVisible(bool isVisible)
    {
        Array.ForEach(GetButtons().ToArray(), r => r.SetVisible(isVisible));
    }

    public void Show(bool scaleIn = false)
    {
        if (!scaleIn)
        {
            Array.ForEach(GetButtons().ToArray(), r => r.Show());
            return;
        }

        normalButton.Show(true, scaleDuration: 0.75f);
        tripleButtonGet3x.Show(true, scaleDuration: 0.75f);
        tripleButtonGetNormal.Show(false, 0.3f, 0.5f);
    }

    public void Hide()
    {
        Array.ForEach(GetButtons().ToArray(), r => r.Hide());
    }
}
