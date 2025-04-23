using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unpuzzle
{
    public class MapItemLevel : MonoBehaviour
    {
        [Header("Normal")] [SerializeField] private Image _normalNextImage;

        [SerializeField] private Image _normalCurrentImage;
        [SerializeField] private Image _normalPrevImage;

        [Header("Hard")] [SerializeField] private Transform _harDefaultRay;

        [SerializeField] private Transform _hardCurrentRay;
        [SerializeField] private Image _hardNextImage;
        [SerializeField] private Image _hardCurrentImage;
        [SerializeField] private Image _hardPrevImage;

        [Header("Epic")] [SerializeField] private Transform _epicDefaultRay;

        [SerializeField] private Transform _epicCurrentRay;
        [SerializeField] private Image _epicNextImage;
        [SerializeField] private Image _epicCurrentImage;
        [SerializeField] private Image _epicPrevImage;

        [Header("Text")] [SerializeField] private TMP_Text _nextText;

        [SerializeField] private TMP_Text _currentText;
        [SerializeField] private TMP_Text _prevText;

        [Header("Prize")] [SerializeField] private Image _prizeSmall;

        [SerializeField] private Image _prizeBig;

        [Header("Crown")] [SerializeField] private Image _crownSmall;

        [SerializeField] private Image _crownHardSmall;
        [SerializeField] private Image _crownEpicSmall;

        [SerializeField] private AnimationCurve _nextAnimationCurve;

        private LevelDifficultyType _levelDifficultyType = LevelDifficultyType.Normal;
        private Sequence _sequence;

        public void Init(int currentLevel, int itemLevel, LevelDifficultyType levelDifficultyType, bool hasPrize)
        {
            _levelDifficultyType = levelDifficultyType;
            var order = Mathf.Clamp(itemLevel - currentLevel, -1, 1);

            _normalNextImage.gameObject.SetActive(false);
            _normalCurrentImage.gameObject.SetActive(false);
            _normalPrevImage.gameObject.SetActive(false);

            _hardNextImage.gameObject.SetActive(false);
            _hardCurrentImage.gameObject.SetActive(false);
            _hardPrevImage.gameObject.SetActive(false);
            _hardCurrentRay.gameObject.SetActive(false);
            _harDefaultRay.gameObject.SetActive(false);

            _epicNextImage.gameObject.SetActive(false);
            _epicCurrentImage.gameObject.SetActive(false);
            _epicPrevImage.gameObject.SetActive(false);
            _epicCurrentRay.gameObject.SetActive(false);
            _epicDefaultRay.gameObject.SetActive(false);

            _nextText.gameObject.SetActive(false);
            _currentText.gameObject.SetActive(false);
            _prevText.gameObject.SetActive(false);

            _prizeSmall.gameObject.SetActive(false);
            _prizeBig.gameObject.SetActive(false);

            _crownSmall.gameObject.SetActive(false);
            _crownHardSmall.gameObject.SetActive(false);
            _crownEpicSmall.gameObject.SetActive(false);

            switch (order)
            {
                case 1:
                    _nextText.gameObject.SetActive(true);
                    _nextText.SetText(itemLevel.ToString());
                    _prizeSmall.gameObject.SetActive(hasPrize);
                    break;
                case 0:
                    _currentText.gameObject.SetActive(true);
                    _currentText.SetText(itemLevel.ToString());
                    _prizeBig.gameObject.SetActive(hasPrize);
                    break;
                case -1:
                    _prevText.gameObject.SetActive(true);
                    _prevText.SetText(itemLevel.ToString());
                    break;
            }

            switch (levelDifficultyType)
            {
                case LevelDifficultyType.Normal:
                    switch (order)
                    {
                        case 1:
                            _normalNextImage.gameObject.SetActive(true);
                            break;
                        case 0:
                            _normalCurrentImage.gameObject.SetActive(true);
                            break;
                        case -1:
                            _normalPrevImage.gameObject.SetActive(true);
                            _crownSmall.gameObject.SetActive(true);
                            break;
                    }

                    break;

                case LevelDifficultyType.Hard:
                    switch (order)
                    {
                        case 1:
                            _hardNextImage.gameObject.SetActive(true);
                            _harDefaultRay.gameObject.SetActive(true);
                            break;
                        case 0:
                            _hardCurrentImage.gameObject.SetActive(true);
                            _hardCurrentRay.gameObject.SetActive(true);
                            break;
                        case -1:
                            _hardPrevImage.gameObject.SetActive(true);
                            _crownHardSmall.gameObject.SetActive(true);
                            break;
                    }

                    break;

                case LevelDifficultyType.Epic:
                    switch (order)
                    {
                        case 1:
                            _epicNextImage.gameObject.SetActive(true);
                            _epicDefaultRay.gameObject.SetActive(true);
                            break;
                        case 0:
                            _epicCurrentImage.gameObject.SetActive(true);
                            _epicCurrentRay.gameObject.SetActive(true);
                            break;
                        case -1:
                            _epicPrevImage.gameObject.SetActive(true);
                            _crownEpicSmall.gameObject.SetActive(true);
                            break;
                    }

                    break;
            }
        }

        private MapItemComponents GetMapItemComponents(LevelDifficultyType levelDifficultyType)
        {
            var mapItemComponents = new MapItemComponents();
            switch (levelDifficultyType)
            {
                case LevelDifficultyType.Hard:
                    mapItemComponents.prevImage = _hardPrevImage;
                    mapItemComponents.currentImage = _hardCurrentImage;
                    mapItemComponents.nextImage = _hardNextImage;
                    mapItemComponents.crownImage = _crownHardSmall;
                    break;
                case LevelDifficultyType.Epic:
                    mapItemComponents.prevImage = _epicPrevImage;
                    mapItemComponents.currentImage = _epicCurrentImage;
                    mapItemComponents.nextImage = _epicNextImage;
                    mapItemComponents.crownImage = _crownEpicSmall;
                    break;
                case LevelDifficultyType.Normal:
                default:
                    mapItemComponents.prevImage = _normalPrevImage;
                    mapItemComponents.currentImage = _normalCurrentImage;
                    mapItemComponents.nextImage = _normalNextImage;
                    mapItemComponents.crownImage = _crownSmall;
                    break;
            }

            return mapItemComponents;
        }

        public void InitPrevAnimation()
        {
            _sequence?.Kill();
            var mapItemComponents = GetMapItemComponents(_levelDifficultyType);


            mapItemComponents.crownImage.gameObject.SetActive(false);
            mapItemComponents.crownImage.rectTransform.anchoredPosition =
                mapItemComponents.crownImage.rectTransform.anchoredPosition.AddY(30f);

            mapItemComponents.nextImage.gameObject.SetActive(false);
            mapItemComponents.currentImage.gameObject.SetActive(true);
            mapItemComponents.prevImage.gameObject.SetActive(false);

            _nextText.gameObject.SetActive(false);
            _currentText.gameObject.SetActive(true);
            _prevText.gameObject.SetActive(false);

            _currentText.text = _prevText.text;
        }

        public void ShowPrevAnimation()
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence().SetLink(gameObject);


            var mapItemComponents = GetMapItemComponents(_levelDifficultyType);
            mapItemComponents.crownImage.gameObject.SetActive(true);
            mapItemComponents.crownImage.SetAlpha(0f);
            mapItemComponents.crownImage.transform.localScale = Vector3.one * 0.5f;

            var lastT = 0f;
            var switched = false;

            void ChangeScale(float t)
            {
                lastT = t;
                t = DOVirtual.EasedValue(0f, 1f, t, Ease.InOutBack);
                t = Mathf.LerpUnclamped(1f, 0.6f, t);
                if (switched)
                {
                    t /= 0.6f;
                }

                transform.localScale = Vector3.one * t;
            }

            _sequence.Insert(0f, DOTween.To(() => 0f, t =>
                {
                    ChangeScale(t);
                }, 1f, 0.4f).SetEase(Ease.Linear))
                .InsertCallback(0.4f * 0.75f, () =>
                {
                    switched = true;

                    mapItemComponents.nextImage.gameObject.SetActive(false);
                    mapItemComponents.currentImage.gameObject.SetActive(false);
                    mapItemComponents.prevImage.gameObject.SetActive(true);

                    _nextText.gameObject.SetActive(false);
                    _currentText.gameObject.SetActive(false);
                    _prevText.gameObject.SetActive(true);

                    ChangeScale(lastT);
                });
            _sequence.Insert(0.4f,
                mapItemComponents.crownImage.rectTransform
                    .DOAnchorPosY(mapItemComponents.crownImage.rectTransform.anchoredPosition.y - 30f, 0.7f)
                    .SetEase(Ease.OutBounce));
            _sequence.Insert(0.4f, mapItemComponents.crownImage.DOFade(1f, 0.1f).SetEase(Ease.OutExpo));
            _sequence.Insert(0.4f, mapItemComponents.crownImage.transform.DOScale(1f, 0.05f));
        }

        public void InitCurrentAnimation()
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
            var mapItemComponents = GetMapItemComponents(_levelDifficultyType);
            mapItemComponents.nextImage.gameObject.SetActive(true);
            mapItemComponents.currentImage.gameObject.SetActive(false);
            mapItemComponents.prevImage.gameObject.SetActive(false);

            _nextText.gameObject.SetActive(true);
            _currentText.gameObject.SetActive(false);
            _prevText.gameObject.SetActive(false);
            _nextText.text = _currentText.text;
        }

        public void ShowCurrentAnimation(Action callback)
        {
            transform.DOKill();
            transform.localScale = Vector3.one;

            var lastT = 0f;
            var switched = false;

            void ChangeScale(float t)
            {
                lastT = t;

                t = _nextAnimationCurve.Evaluate(t);
                t = Mathf.LerpUnclamped(1f, 1f / 0.6f, t);
                if (switched)
                {
                    t *= 0.6f;
                }

                transform.localScale = Vector3.one * t;
            }

            var mapItemComponents = GetMapItemComponents(_levelDifficultyType);
            DOTween.Sequence()
                .SetLink(gameObject)
                .Append(DOTween.To(() => 0f, t =>
                {
                    ChangeScale(t);
                }, 1f, 0.4f).SetEase(Ease.Linear))
                .InsertCallback(0.4f * 0.2f, () =>
                {
                    switched = true;

                    mapItemComponents.nextImage.gameObject.SetActive(false);
                    mapItemComponents.currentImage.gameObject.SetActive(true);
                    mapItemComponents.prevImage.gameObject.SetActive(false);

                    _nextText.gameObject.SetActive(false);
                    _currentText.gameObject.SetActive(true);
                    _prevText.gameObject.SetActive(false);

                    ChangeScale(lastT);
                })
                .OnComplete(() =>
                {
                    callback?.Invoke();
                });
        }

        private struct MapItemComponents
        {
            public Image nextImage;
            public Image currentImage;
            public Image prevImage;
            public Image crownImage;
        }
    }

    public enum LevelDifficultyType
    {
        Normal = 0,
        Hard = 1,
        Epic = 2
    }
}
