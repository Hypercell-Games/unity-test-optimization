using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SortCore
{
    public class RateAppStar : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private int rateIndex;

        [SerializeField] private Image _activeImage;

        [SerializeField] private RateAppLayout rateAppLayout;

        private bool? _isActive;
        private Sequence _showStarSeq;

        public void OnPointerDown(PointerEventData eventData)
        {
            rateAppLayout.SetRate(rateIndex);
        }

        public void UpdateState(int rate)
        {
            var active = rate >= rateIndex;
            if (_isActive == active)
            {
                return;
            }

            _isActive = active;
            _showStarSeq?.Kill();
            if (_isActive.Value)
            {
                _showStarSeq = DOTween.Sequence()
                    .SetLink(gameObject)
                    .Append(_activeImage.DOFade(1f, 0.2f))
                    .Join(_activeImage.rectTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
            }
            else
            {
                _showStarSeq = DOTween.Sequence()
                    .SetLink(gameObject)
                    .Append(_activeImage.DOFade(0f, 0.2f))
                    .Join(_activeImage.rectTransform.DOScale(0.5f, 0.2f));
            }
        }
    }
}
