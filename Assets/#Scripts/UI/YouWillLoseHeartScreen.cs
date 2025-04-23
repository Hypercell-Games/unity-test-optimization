using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unpuzzle
{
    public class YouWillLoseHeartScreen : BaseScreen
    {
        public enum ScreenType
        {
            Restart,
            Quit
        }

        [SerializeField] private RectTransform _panel;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TextButtonController _loseButton;
        [SerializeField] private Button _closeButton;

        private Sequence _showSeq;

        private void Awake()
        {
            _closeButton.onClick.AddListener(() =>
            {
                Hide();
            });
        }

        public void Init()
        {
            Hide(true);
        }

        private void OnRestartClick(Action onLoseLifeClick)
        {
            Hide();
            onLoseLifeClick?.Invoke();
        }

        private void OnQuitClick(Action onLoseLifeClick)
        {
            Hide();
            onLoseLifeClick?.Invoke();
        }

        public void Show(ScreenType screenType, Action onLoseLifeClick)
        {
            switch (screenType)
            {
                case ScreenType.Restart:


                    _titleText.text = "Are you sure?";
                    _loseButton.SetText("Yes");
                    _loseButton.SetOnButtonClick(() => OnRestartClick(onLoseLifeClick));
                    break;
                case ScreenType.Quit:


                    _titleText.text = "Are you sure?";
                    _loseButton.SetText("Yes");
                    _loseButton.SetOnButtonClick(() => OnQuitClick(onLoseLifeClick));
                    break;
            }

            Show();
        }

        public override void Show(bool force = false, Action callback = null)
        {
            base.Show(force, callback);

            _showSeq?.Kill();
            if (force)
            {
                _panel.localScale = Vector3.one;
                return;
            }

            _showSeq = DOTween.Sequence()
                .SetLink(gameObject)
                .Append(_panel.DOScale(1f, _showDuration).SetEase(Ease.OutBack));
        }

        public override void Hide(bool force = false, Action callback = null)
        {
            base.Hide(force, callback);

            if (force)
            {
                _panel.localScale = Vector3.one * 0.5f;
                return;
            }

            _showSeq = DOTween.Sequence()
                .SetLink(gameObject)
                .Append(_panel.DOScale(0.5f, _hideDuration).SetEase(Ease.InBack));
        }
    }
}
