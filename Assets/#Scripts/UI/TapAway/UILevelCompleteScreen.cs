using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Unpuzzle.Game;
using Unpuzzle.Game.Data;
using static LevelCompleteButtonsController;

namespace TapAway
{
    public class UILevelCompleteScreen : BaseScreen
    {
        [Space] [SerializeField] public GameData _gameData;

        [Space] [SerializeField] private TMP_Text _finalText;

        [Header("Buttons")] [SerializeField] private LevelCompleteButtonsController _buttonsController;

        [Header("Effect")] [SerializeField] private ParticleSystem _confettiParticleSystem;

        [Header("SpriteChanger")] [SerializeField]
        private FinalSpriteChanger _finalSpriteChanger;

        [SerializeField] private GameObject _wowEffects;

        private int _coinsToReceive = 101;
        private int _initialCurrency = 302;

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

        public event Action onNextLevel;

        public override void Show(bool force = false, Action callback = null)
        {
            _finalSpriteChanger.SetRandomSprite();
            _buttonsController.SetInteractable(true);
            _wowEffects.SetActive(true);

            _coinsToReceive = Mathf.RoundToInt(GameLogicUtil.CoinsAmountForLevel(LevelData.GetLevelNumber()) *
                                               GameConfig.RemoteConfig.rewardForDaily);
            _gameData.AddSoftCcy(_coinsToReceive);

            _initialCurrency = _gameData.SoftCcyAmount;

            HyperKit.Ads.HideBanner();

            base.Show(force, () =>
            {
                callback?.Invoke();
                _confettiParticleSystem.Play();
            });

            StartCoroutine(CO_RewardAnimationBarDisabled());

            _buttonsController.SetLayout(Layout.Next);
        }

        private IEnumerator CO_RewardAnimationBarDisabled()
        {
            _buttonsController.SetVisible(false);

            yield return new WaitForSeconds(0.5f);

            _buttonsController.Show(true);
        }

        private IEnumerator CO_RewardCollectAnimation(int coinsToReceive, int initialCurrency)
        {
            _gameData.SoftCurrency.ApplyChange(coinsToReceive);

            yield return null;
        }

        private void AddListeners()
        {
            _buttonsController.OnNormalRequested += ButtonNormalClicked;
            _buttonsController.OnBonusRequested += ButtonTripleClicked;
        }

        private void RemoveListeners()
        {
            _buttonsController.OnNormalRequested -= ButtonNormalClicked;
            _buttonsController.OnBonusRequested -= ButtonTripleClicked;
        }

        private void ButtonNormalClicked()
        {
            ButtonClicked(false);
        }

        private void ButtonTripleClicked()
        {
            HyperKit.Ads.ShowRewardedAd("ad_rewarded_triple_coins_tap_away",
                success =>
                {
                    if (!success)
                    {
                        return;
                    }

                    ButtonClicked(true);
                });
        }

        private void ButtonClicked(bool tripleCoins)
        {
            VibrationController.Instance.Play(EVibrationType.LightImpact);

            _buttonsController.Hide();

            HideInternal();

            StartCoroutine(CO_CollectAndExitAnimation(tripleCoins));
        }

        private IEnumerator CO_CollectAndExitAnimation(bool tripleCoins)
        {
            var coinMultiplier = tripleCoins ? 3 : 1;

            yield return CO_RewardCollectAnimation(_coinsToReceive * coinMultiplier, _gameData.SoftCcyAmount);

            yield return new WaitForSeconds(1f);

            _wowEffects.SetActive(false);

            Hide();
        }

        private void HideInternal()
        {
            HyperKit.Ads.ShowBanner();

            Hide();

            _buttonsController.SetInteractable(false);
            _wowEffects.SetActive(false);

            onNextLevel?.Invoke();
        }
    }
}
