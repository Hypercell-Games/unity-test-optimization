using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unpuzzle
{
    public class BoosterRemoverButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public enum BoosterState
        {
            hasBooster = 0,
            canBuy = 1,
            reward = 2
        }

        private const int removePinCount = 4;
        private const int boosterCost = 30;

        [SerializeField] private BoosterRemoverFX _removeFX;
        [SerializeField] private Camera _uiCamera;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Transform _startAnchor;
        [SerializeField] private Transform _flyParent;
        [SerializeField] private TextMeshProUGUI _count;
        [SerializeField] private TextMeshProUGUI _costLabel;
        [SerializeField] private Transform _tweenObject;

        [SerializeField] private GameObject _defaultState;
        [SerializeField] private GameObject _costState;
        [SerializeField] private GameObject _defaultStateBg;
        [SerializeField] private GameObject _rewardStateBg;
        [SerializeField] private GameObject _rewardIcon;

        private BoosterState _currentBoosterState;

        private bool _isInUsing;
        private bool _isPressed;

        private void Start()
        {
            UpdateCount();

            BoosterRemoverUtil.OnBoosterCountChange += UpdateCount;
        }

        private void OnDestroy()
        {
            BoosterRemoverUtil.OnBoosterCountChange -= UpdateCount;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isInUsing)
            {
                return;
            }

            var levelController = LevelsController.Instance.CurrentGameController.GetCurrentStage().Level;

            var pinsCount = removePinCount;

            var hookCount = 0;

            if (pinsCount >= 1.0f)
            {
                hookCount = Mathf.RoundToInt(pinsCount);
            }
            else
            {
                hookCount = Mathf.CeilToInt(pinsCount * levelController.Hooks.Count(a => !a.Removed));
            }


            if (hookCount == 0)
            {
                return;
            }

            var iceBlocks = levelController.BoosterRemoveGetIceBlocks(hookCount);

            hookCount -= iceBlocks.Count;

            var pins = levelController.BoosterRemoveGetPins(hookCount);

            if (pins.Count == 0 && iceBlocks.Count == 0)
            {
                return;
            }

            if (_currentBoosterState == BoosterState.hasBooster)
            {
                ExecuteBooster(pins, iceBlocks, levelController);
                return;
            }

            var gameData = GlobalData.Instance.GetGameData();
            var coins = gameData.SoftCurrency.Value;

            if (coins >= boosterCost)
            {
                gameData.RemoveSoftCcy(boosterCost, "booster_remover");

                ExecuteBooster(pins, iceBlocks, levelController);
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

        private void ExecuteBooster(List<HookController> pins, List<IIceBlock> iceBlocks, PinOutLevel level)
        {
            if (pins == null && pins.Count == 0)
            {
                return;
            }

            var startPos = _uiCamera
                .ScreenPointToRay(_uiCamera.WorldToScreenPoint(
                    _startAnchor.parent.TransformPoint(_startAnchor.localPosition +
                                                       Vector3.right * transform.localPosition.x)))
                .GetPoint(_flyParent.localPosition.z);
            pins.ForEach(a => a.SetRemoved());
            var delay = 0f;

            foreach (var iceBlock in iceBlocks)
            {
                var fx = Instantiate(_removeFX, startPos, Quaternion.identity);
                fx.transform.SetParent(_flyParent);
                fx.RemoveIceBlock(iceBlock, _flyParent, _mainCamera, _uiCamera, delay);
                delay += 0.1f;
            }

            foreach (var pin in pins)
            {
                var fx = Instantiate(_removeFX, startPos, Quaternion.identity);
                fx.transform.SetParent(_flyParent);
                fx.RemovePin(pin, _flyParent, _mainCamera, _uiCamera, delay);
                delay += 0.1f;
            }

            level.OnPreMove();

            _isInUsing = true;

            _tweenObject.DOKill();
            _tweenObject.DOScale(0.9f, 0.2f)
                .OnComplete(() =>
                {
                    _tweenObject.DOScale(1f, 0.2f)
                        .SetDelay(delay + 0.7f)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() => { _isInUsing = false; });
                });

            BoosterRemoverUtil.BoosterUsed();
            UpdateCount();
        }

        public void UpdateCount()
        {
            UpdateBoosterState();

            _costState.SetActive(_currentBoosterState == BoosterState.canBuy);
            _defaultState.SetActive(_currentBoosterState == BoosterState.hasBooster);
            _defaultStateBg.SetActive(_currentBoosterState != BoosterState.reward);
            _rewardStateBg.SetActive(_currentBoosterState == BoosterState.reward);
            _rewardIcon.SetActive(_currentBoosterState == BoosterState.reward);

            _count.text = BoosterRemoverUtil.GetBoosterCount().ToString();
            _costLabel.text = boosterCost.ToString();
        }

        private void UpdateBoosterState()
        {
            var boosterCount = BoosterRemoverUtil.GetBoosterCount();

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
