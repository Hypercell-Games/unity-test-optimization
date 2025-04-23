using DG.Tweening;
using UnityEngine;
using Unpuzzle.UI.NewTabBar;

namespace Unpuzzle
{
    public class NewTabBarSelectorWidget : MonoBehaviour
    {
        private const float _tweenDuration = 0.3f;
        private const Ease _tweenEase = Ease.OutBack;

        [SerializeField] private RectTransform _bubbleHighliht;
        [SerializeField] private RectTransform _iconHiglight;

        private Tween _positionTween;
        private Tween _scaleTween;

        public void UpdatePosition(NewTabBarButton tabBarButton, bool force)
        {
            var targetPosX = tabBarButton.transform.localPosition.x;

            if (force)
            {
                _bubbleHighliht.anchoredPosition = new Vector2(targetPosX, _bubbleHighliht.anchoredPosition.y);
                _iconHiglight.anchoredPosition = new Vector2(targetPosX, _iconHiglight.anchoredPosition.y);
                return;
            }

            _positionTween?.Kill();
            _positionTween = DOTween.Sequence().SetLink(gameObject)
                .Append(_bubbleHighliht.DOAnchorPosX(targetPosX, _tweenDuration * 0.8f).SetEase(Ease.OutQuad))
                .Join(_iconHiglight.DOAnchorPosX(targetPosX, _tweenDuration * 0.8f).SetEase(Ease.OutQuad));

            _scaleTween?.Kill();
            _scaleTween = DOTween.Sequence().SetLink(gameObject)
                .Append(_bubbleHighliht.DOScale(0.7f, _tweenDuration * 0.3f).SetEase(Ease.OutSine))
                .Join(_iconHiglight.DOScale(0.7f, _tweenDuration * 0.3f).SetEase(Ease.OutSine))
                .Append(_bubbleHighliht.DOScale(1, _tweenDuration * 0.9f).SetEase(_tweenEase, 1.5f))
                .Join(_iconHiglight.DOScale(1, _tweenDuration * 0.9f).SetEase(_tweenEase, 1.5f));
        }
    }
}
