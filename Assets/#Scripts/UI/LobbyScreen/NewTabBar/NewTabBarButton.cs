using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unpuzzle.UI.NewTabBar
{
    public class NewTabBarButton : MonoBehaviour
    {
        private const float _selectScale = 1f;
        private const float _selectedYShift = 16f;
        private const float _selectDuration = 0.3f;
        private const Ease _selectEase = Ease.OutBack;
        private const Ease _unselectEase = Ease.OutBack;
        private const float _unselectDuration = 0.3f;

        private static int _countButtons = -1;
        private static float _selectedWidth;
        private static float _unselectedWidth;

        [Header("Config")] [SerializeField] private Color _selectedColor;

        [SerializeField] private Color _unselectedColor;
        [SerializeField] private Color _lockedColor;

        [Space] [SerializeField] private bool swapSprites;

        [SerializeField] private Sprite _lockedSprite;
        [SerializeField] private Sprite _unselectedSprite;

        [Space] [SerializeField] private Image _buttonImage;

        [SerializeField] private Sprite _pressSprite;
        [SerializeField] private Sprite _unpressSprite;

        [Header("References")] [SerializeField]
        private RectTransform _rectTransform;

        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _holder;
        [SerializeField] private Image _selectImage;
        [SerializeField] private Image _lockedIcon;
        [SerializeField] private TextMeshProUGUI _lockLabel;
        [SerializeField] private GameObject _circlePin;

        private bool _isLocked;
        private bool _secondUpdateVisual;
        private float _selectValue;
        private Tween selectTween;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }

        public event Action<NewTabBarButton, bool> OnClick;

        private void OnButtonClick()
        {
            OnClick?.Invoke(this, !_isLocked);
        }

        public virtual void SetPin(TabBarPinType leaderboardPinType)
        {
            _circlePin.SetActive(leaderboardPinType == TabBarPinType.Pin);
        }

        public void SetSelected(bool isSelected, bool force, int countButtons)
        {
            if (_countButtons != countButtons)
            {
                _countButtons = countButtons;
                switch (_countButtons)
                {
                    case 3:
                        _unselectedWidth = 309f;
                        _selectedWidth = 460f;
                        break;

                    case 4:
                        _unselectedWidth = 250f;
                        _selectedWidth = 330f;
                        break;

                    default:
                        if (_countButtons < 3)
                        {
                            var kf = 460f / 309f;
                            _unselectedWidth = (_countButtons + kf - 1f) / _countButtons;
                            _selectedWidth = _unselectedWidth * kf;
                        }
                        else
                        {
                            var kf = 330f / 250f;
                            _unselectedWidth = (_countButtons + kf - 1f) / _countButtons;
                            _selectedWidth = _unselectedWidth * kf;
                        }

                        break;
                }
            }

            var unselectedColor = GetTargetColor(isSelected);
            var targetColor = unselectedColor;
            var targetScale = isSelected ? _selectScale : 1f;
            var targetYShift = isSelected ? _selectedYShift : 0f;

            if (swapSprites)
            {
                _selectImage.overrideSprite = GetSpriteOverride(isSelected);
                _selectImage.SetNativeSize();
            }

            var screenWidthScale = LobbyScreensSwitcher.GetScreenWidthScale();
            _buttonImage.sprite = isSelected ? _pressSprite : _unpressSprite;
            if (force)
            {
                _holder.localScale = Vector3.one * targetScale;
                _holder.anchoredPosition = new Vector2(0, targetYShift);
                _selectImage.SetAlpha(isSelected ? 1f : 0);
                _selectImage.color = targetColor;
                _selectValue = isSelected ? 1f : 0f;
                UpdateVisual();
                return;
            }

            var targetDuration = isSelected ? _selectDuration : _unselectDuration;
            var targetEase = isSelected ? _selectEase : _unselectEase;
            var targetFade = isSelected ? 1f : 0f;
            var targetSelect = isSelected ? 1f : 0f;

            selectTween?.Kill();
            var startSelect = _selectValue;
            var animationSetting = GlobalData.Instance.GetGameData().LobbyTabBarAnimationSetting;
            selectTween = DOTween.Sequence()
                .Append(DOTween.To(() => 0f, t =>
                {
                    _selectValue = Mathf.LerpUnclamped(startSelect, targetSelect, t);
                    UpdateVisual();
                }, 1f, animationSetting.Duration).SetEase(animationSetting.GetEaseFunction()));

            void UpdateVisual()
            {
                _rectTransform.sizeDelta = Vector2.LerpUnclamped(new Vector2(_unselectedWidth * screenWidthScale, 160f),
                    new Vector2(_selectedWidth * screenWidthScale, 187f), _selectValue);
            }
        }

        private Color GetTargetColor(bool isSelected)
        {
            if (swapSprites)
            {
                return Color.white;
            }

            if (_isLocked)
            {
                return _lockedColor;
            }

            return isSelected ? _selectedColor : _unselectedColor;
        }

        private Sprite GetSpriteOverride(bool isSelected)
        {
            if (_isLocked)
            {
                return _lockedSprite;
            }

            return isSelected ? null : _unselectedSprite;
        }

        public Tween TweenShown(bool isOpen, float duration)
        {
            _holder.DOKill();

            if (isOpen)
            {
                return _holder.DOAnchorPosY(0f, duration).SetEase(Ease.OutBack);
            }

            return _holder.DOAnchorPosY(-80f, duration).SetEase(Ease.Linear);
        }

        public void SetLocked(bool isLocked, int lockedLevel = 0)
        {
            _isLocked = isLocked;
            _lockLabel.gameObject.SetActive(isLocked && lockedLevel > 0);
            _lockLabel.text = $"Level {lockedLevel}";
            _lockedIcon.gameObject.SetActive(isLocked);

            if (swapSprites)
            {
                _selectImage.overrideSprite = GetSpriteOverride(false);
                _selectImage.SetNativeSize();
            }
            else
            {
                _selectImage.color = GetTargetColor(false);
            }
        }

        public RectTransform GetIcon()
        {
            return _selectImage.rectTransform;
        }
    }
}
