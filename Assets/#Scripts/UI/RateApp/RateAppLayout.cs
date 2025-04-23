using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SortCore
{
    public class RateAppLayout : MonoBehaviour
    {
        [SerializeField] private Button closeButton;

        [SerializeField] private BaseButtonController submitButton;

        [SerializeField] private CanvasGroup mainGroup;

        [SerializeField] private List<RateAppStar> stars;

        [SerializeField] private RectTransform content;

        private Sequence _rateSeq;
        private float _visualRate;

        private int currentRate = 5;

        public bool isShowed => gameObject.activeSelf;

        public void OnEnable()
        {
            closeButton.onClick.AddListener(OnCloseButtonClick);

            submitButton.onButtonClicked += OnSubmitButtonClick;
        }

        public void OnDisable()
        {
            closeButton.onClick.RemoveAllListeners();

            submitButton.onButtonClicked -= OnSubmitButtonClick;
        }

        public void SetRate(int rate)
        {
            currentRate = rate;

            UpdateStars();
        }

        private void UpdateStars()
        {
            _rateSeq?.Kill();
            _rateSeq = DOTween.Sequence()
                .SetLink(gameObject)
                .Append(DOTween.To(() => _visualRate, t =>
                {
                    _visualRate = t;
                    foreach (var s in stars)
                    {
                        s.UpdateState((int)_visualRate);
                    }
                }, currentRate, Mathf.Abs(_visualRate - currentRate) * 0.1f).SetEase(Ease.Linear));
        }

        public void Show()
        {
            gameObject.SetActive(true);

            UpdateStars();

            StartCoroutine(ShowAnimation());
        }

        private IEnumerator ShowAnimation()
        {
            mainGroup.interactable = false;
            mainGroup.alpha = 0f;
            closeButton.transform.localScale = Vector3.zero;
            submitButton.transform.localScale = Vector3.zero;

            content.DOMoveY(-20, 0.5f)
                .SetEase(Ease.OutCubic)
                .From()
                .SetLink(content.gameObject);

            closeButton.transform.DOScale(1f, 0.5f)
                .SetDelay(0.5f)
                .SetLink(closeButton.gameObject);

            foreach (var star in stars)
            {
                star.transform.localScale = Vector3.zero;
            }

            mainGroup.DOFade(1f, 0.5f);

            yield return new WaitForSeconds(0.8f);

            for (var i = 0; i < stars.Count; i++)
            {
                var star = stars[i];
                star.transform.DOScale(1f, 0.5f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * 0.1f)
                    .SetLink(star.gameObject);
            }

            yield return new WaitForSeconds(0.7f);

            submitButton.transform.DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack)
                .SetLink(submitButton.gameObject);

            yield return new WaitForSeconds(0.4f);

            mainGroup.interactable = true;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            ScreenSwitcher.Instance.NextStartScreen(ScreenType.RateApp);
        }

        private void OnCloseButtonClick()
        {
            Hide();
        }

        private void OnSubmitButtonClick()
        {
            Hide();

            if (currentRate >= GameConfig.RemoteConfig.openAppUrlAtRate)
            {
                Application.OpenURL(GameConfig.RemoteConfig.androidAppURL);
            }
        }
    }
}
