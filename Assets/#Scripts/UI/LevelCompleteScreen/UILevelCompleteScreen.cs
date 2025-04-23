using System;
using System.Collections;
using TMPro;
using UnityEngine;
using static LevelCompleteButtonsController;

public class UILevelCompleteScreen : BaseScreen
{
    [Space] [SerializeField] public GameData _gameData;

    [Space] [SerializeField] private TMP_Text _finalText;

    [Header("Buttons")] [SerializeField] private LevelCompleteButtonsController _buttonsController;
    [Header("Effect")] [SerializeField] private ParticleSystem _confettiParticleSystem;

    [Header("SpriteChanger")] [SerializeField]
    private FinalSpriteChanger _finalSpriteChanger;

    [SerializeField] private GameObject _wowEffects;

    private readonly int _bonusMultiplier = 1;
    private int _coinsToReceive;
    private int _initialCurrency;

    private void Awake()
    {
        Hide(true);
    }

    private void OnEnable()
    {
        AddListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    public override void Show(bool force = false, Action callback = null)
    {
        _finalSpriteChanger.SetRandomSprite();
        _buttonsController.SetInteractable(true);
        _wowEffects.SetActive(true);

        base.Show(force, () =>
        {
            callback?.Invoke();
            PlayConfetti();
        });

        _buttonsController.SetLayout(Layout.Next);
    }

    private IEnumerator CO_RewardCollectAnimation(int coinsToReceive, int initialCurrency)
    {
        _gameData.AddSoftCcy(coinsToReceive);

        yield return null;
    }

    private void AddListeners()
    {
        _buttonsController.OnNormalRequested += ButtonNormalClicked;
    }

    private void RemoveListeners()
    {
        _buttonsController.OnNormalRequested -= ButtonNormalClicked;
    }

    private void ButtonNormalClicked()
    {
        ButtonClicked(false);
    }

    private void ButtonClicked(bool bonusCoins)
    {
        VibrationController.Instance.Play(EVibrationType.LightImpact);

        _buttonsController.Hide();

        StartCoroutine(CO_CollectAndExitAnimation(bonusCoins));
    }

    private IEnumerator CO_CollectAndExitAnimation(bool bonusCoins)
    {
        var coinMultiplier = bonusCoins ? _bonusMultiplier : 1;
        coinMultiplier = Mathf.Max(coinMultiplier - 1, 0);

        yield return CO_RewardCollectAnimation(_coinsToReceive * coinMultiplier, _initialCurrency);

        _wowEffects.SetActive(false);

        Hide();

        _gameData.GoToGameMode(new LobbyMode(_gameData));
    }

    private void PlayConfetti()
    {
        _confettiParticleSystem.Play();
    }
}
