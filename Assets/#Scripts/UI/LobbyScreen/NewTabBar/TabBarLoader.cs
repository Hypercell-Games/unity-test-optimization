using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    public class TabBarLoader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        private Tweener _fillTween;

        public float FillProgress => 1f;

        private void Update()
        {
            _fillTween?.ManualUpdate(0.016f, 0.016f);
        }

        public void Show(bool force)
        {
            gameObject.SetActive(true);

            _canvasGroup.DOKill();

            if (force)
            {
                _canvasGroup.alpha = 1;
                return;
            }

            _canvasGroup.DOFade(1, 0.2f).SetEase(Ease.InOutSine);
        }

        public void Hide(bool force)
        {
            _canvasGroup.DOKill();

            if (force)
            {
                _canvasGroup.alpha = 0;
                return;
            }

            _canvasGroup.DOFade(0, 0.2f).SetEase(Ease.InOutSine)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void TweenProgress(float startingValue = 0, float? time = null)
        {
            ForceProgress(startingValue);
        }

        public void SetProgress(float progress)
        {
        }

        public void ForceProgress(float progress)
        {
        }
    }
}
