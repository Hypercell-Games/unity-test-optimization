using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unpuzzle
{
    public class BoosterHintButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public enum BoosterState
        {
            hasBooster = 0,
            canBuy = 1,
            reward = 2
        }

        private const int boosterCost = 10;

        [SerializeField] private TextMeshProUGUI _count;
        [SerializeField] private TextMeshProUGUI _costLabel;
        [SerializeField] private Transform _tweenObject;

        [SerializeField] private GameObject _defaultState;
        [SerializeField] private GameObject _costState;
        [SerializeField] private GameObject _defaultStateBg;
        [SerializeField] private GameObject _rewardStateBg;
        [SerializeField] private GameObject _rewardIcon;

        private BoosterState _currentBoosterState;

        private HookController _currentTutorialHint;

        private bool _isInUsing;
        private bool _isPressed;
        private bool _subscribed;

        private HookController _touchedIcePin;

        private void Start()
        {
            UpdateCount();

            if (!_subscribed)
            {
                _subscribed = true;
                BoosterHintUtil.OnBoosterCountChange += UpdateCount;
            }
        }

        private void OnDestroy()
        {
            if (_subscribed)
            {
                BoosterHintUtil.OnBoosterCountChange -= UpdateCount;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isInUsing)
            {
                return;
            }

            var levelController = LevelsController.Instance.CurrentGameController.GetCurrentStage().Level;

            var hintList = levelController.Hints;

            HintLevelListItem hint = null;

            foreach (var currentHint in hintList.hintList)
            {
                if (currentHint.targetPin == null || !levelController.Hooks.Contains(currentHint.targetPin))
                {
                    continue;
                }

                if (currentHint.brokeIceBlockStep)
                {
                    if (currentHint.frozenPin == null)
                    {
                        continue;
                    }


                    if (!currentHint.frozenPin.IsFrozen)
                    {
                        continue;
                    }
                }

                if (currentHint.targetPin.IsFrozen)
                {
                    continue;
                }

                hint = currentHint;
                break;
            }

            if (hint == null)
            {
                return;
            }

            if (_currentBoosterState == BoosterState.hasBooster)
            {
                ExecuteBooster(hint.targetPin);
                return;
            }

            var gameData = GlobalData.Instance.GetGameData();
            var coins = gameData.SoftCurrency.Value;

            if (coins >= boosterCost)
            {
                gameData.RemoveSoftCcy(boosterCost, "booster_hint");

                ExecuteBooster(hint.targetPin);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isInUsing)
            {
                return;
            }

            if (_isPressed)
            {
                return;
            }

            _isPressed = true;

            _tweenObject.DOKill();
            _tweenObject.DOScale(0.9f, 0.1f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isInUsing)
            {
                return;
            }

            if (!_isPressed)
            {
                return;
            }

            _isPressed = false;

            _tweenObject.DOKill();
            _tweenObject.DOScale(1f, 0.1f);
        }

        private void ExecuteBooster(HookController pin)
        {
            if (pin != _currentTutorialHint)
            {
                BoosterHintUtil.BoosterUsed();

                _touchedIcePin = null;
                pin.EnableTutorial(true);
                pin.OnPostMoveOut += DisableHighlight;
                pin.OnBlockedByIce += IceTouched;
                PinOutInput.Instance.SetTutorialPin(pin);
            }

            UpdateCount();
            _currentTutorialHint = pin;
        }

        public void IceTouched(HookController pin)
        {
            if (pin == null || _touchedIcePin != null)
            {
                return;
            }

            _touchedIcePin = pin;
            pin.OnDestroyIce += DisableHighlight;
        }

        public void DisableHighlight(HookController pin)
        {
            if (pin != null)
            {
                if (_touchedIcePin == pin)
                {
                    var levelController = LevelsController.Instance.CurrentGameController.GetCurrentStage().Level;
                    var hintList = levelController.Hints;
                    var hintIndex = hintList.hintList.FindIndex(a => a.targetPin == pin);

                    if (hintIndex > -1)
                    {
                        hintList.hintList.RemoveAt(hintIndex);
                    }
                }

                pin.OnBlockedByIce -= IceTouched;
                pin.OnDestroyIce -= DisableHighlight;
                pin.OnPostMoveOut -= DisableHighlight;

                HintLevelListItem hint = null;
                _touchedIcePin = null;
            }

            PinOutInput.Instance.ClearTutorialPin();
        }

        public void UpdateCount()
        {
            UpdateBoosterState();

            _costState.SetActive(_currentBoosterState == BoosterState.canBuy);
            _defaultState.SetActive(_currentBoosterState == BoosterState.hasBooster);
            _defaultStateBg.SetActive(_currentBoosterState != BoosterState.reward);
            _rewardStateBg.SetActive(_currentBoosterState == BoosterState.reward);
            _rewardIcon.SetActive(_currentBoosterState == BoosterState.reward);

            _count.text = BoosterHintUtil.GetBoosterCount().ToString();
            _costLabel.text = boosterCost.ToString();
        }

        private void UpdateBoosterState()
        {
            var boosterCount = BoosterHintUtil.GetBoosterCount();

            if (boosterCount > 0)
            {
                _currentBoosterState = BoosterState.hasBooster;
                return;
            }

            var gameData = GlobalData.Instance.GetGameData();
            var cost = boosterCost;

            if (gameData.SoftCcyAmount < cost)
            {
                _currentBoosterState = BoosterState.reward;
                return;
            }

            _currentBoosterState = BoosterState.canBuy;
        }
    }
}
