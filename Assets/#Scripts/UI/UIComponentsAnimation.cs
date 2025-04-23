using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unpuzzle
{
    public class UIComponentsAnimation : MonoBehaviour
    {
        [SerializeField] private UIComponentAnimation[] _uiComponentAnimations;

        private Sequence _showSeq;

        public void Init()
        {
            _showSeq?.Kill();
            for (var i = 0; i < _uiComponentAnimations.Length; i++)
            {
                var uiComponentAnimation = _uiComponentAnimations[i];
                uiComponentAnimation.Init();
            }
        }

        public void Show()
        {
            _showSeq?.Kill();
            _showSeq = DOTween.Sequence()
                .SetLink(gameObject);
            for (var i = 0; i < _uiComponentAnimations.Length; i++)
            {
                var uiComponentAnimation = _uiComponentAnimations[i];
                _showSeq.Join(uiComponentAnimation.Animate());
            }
        }
    }

    [Serializable]
    public class UIComponentAnimation
    {
        [SerializeField] private Component _component;
        [SerializeField] private float _atPosition;
        [SerializeField] private float _duration;

        [Header("Fade")] [SerializeField] private float _fadeStart;

        [SerializeField] private Ease _fadeEase = Ease.OutQuad;

        [Header("Scale")] [SerializeField] private float _scaleStart;

        [SerializeField] private Ease _scaleEase = Ease.OutQuad;

        public void Init()
        {
            _component.gameObject.SetActive(false);
            _component.transform.localScale = Vector3.one * _scaleStart;
            switch (_component)
            {
                case Image image:
                    image.SetAlpha(_fadeStart);
                    break;
                case CanvasGroup canvasGroup:
                    canvasGroup.alpha = 0f;
                    break;
                case TMP_Text text:
                    text.SetAlpha(_fadeStart);
                    break;
            }
        }

        public Sequence Animate()
        {
            var seq = DOTween.Sequence()
                .SetLink(_component.gameObject)
                .SetDelay(_atPosition);

            seq.Append(_component.transform.DOScale(1f, _duration).SetEase(_scaleEase).OnStart(() =>
            {
                _component.gameObject.SetActive(true);
            }));

            switch (_component)
            {
                case Image image:
                    seq.Join(image.DOFade(1f, _duration).SetEase(_fadeEase));
                    break;
                case CanvasGroup canvasGroup:
                    seq.Join(canvasGroup.DOFade(1f, _duration).SetEase(_fadeEase));
                    break;
                case TMP_Text text:
                    seq.Join(text.DOFade(1f, _duration).SetEase(_fadeEase));
                    break;
            }

            return seq;
        }
    }
}
