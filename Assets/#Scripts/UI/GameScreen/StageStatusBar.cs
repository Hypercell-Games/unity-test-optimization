using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Unpuzzle
{
    public class StageStatusBar : MonoBehaviour
    {
        [SerializeField] private RectTransform _amountLabelAnchor;
        [SerializeField] private TextMeshProUGUI _amounLabel;
        [SerializeField] private RectTransform _bgParent;
        [SerializeField] private RectTransform _fillParent;
        [SerializeField] private RectTransform _upperEnd;
        [SerializeField] private RectTransform _downEnd;
        [SerializeField] private RectTransform _lineBg;
        [SerializeField] private RectTransform _poins;
        [SerializeField] private RectTransform _filler;

        private int _currentStage;

        private bool _inited;
        private int _maxStage;

        private float _nextSizeFiller;

        private List<RectTransform> _parts;
        private List<RectTransform> _points;

        public void Init(int stageCounts)
        {
            _currentStage = 0;
            _maxStage = stageCounts;
            _inited = false;
            _nextSizeFiller = 0f;

            var fillerSizeDelta = _filler.sizeDelta;
            fillerSizeDelta.y = 0;
            _filler.sizeDelta = fillerSizeDelta;

            if (stageCounts < 2)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            ClearParts();

            _parts = new List<RectTransform>();
            _points = new List<RectTransform>();

            var begin = Instantiate(_upperEnd, _bgParent);
            begin.anchoredPosition = Vector2.zero;

            _parts.Add(begin);

            for (var i = 0; i < stageCounts - 1; i++)
            {
                var item = Instantiate(_lineBg, _bgParent);
                item.anchoredPosition = _parts[_parts.Count - 1].anchoredPosition +
                                        Vector2.down * _parts[_parts.Count - 1].sizeDelta.y;

                _parts.Add(item);
            }

            for (var i = _parts.Count - 1; i >= 0; i--)
            {
                var part = _parts[i];
                var point = Instantiate(_poins, _fillParent);
                point.anchoredPosition = part.anchoredPosition + Vector2.down * part.sizeDelta.y;
                _points.Add(point);
            }

            _filler.anchoredPosition = _points[0].anchoredPosition;

            _amountLabelAnchor.anchoredPosition = _parts[_parts.Count - 1].anchoredPosition +
                                                  Vector2.down * _parts[_parts.Count - 1].sizeDelta.y;
            _amounLabel.text = $"0/{_maxStage}";

            _points.ForEach(a => a.gameObject.SetActive(false));

            var end = Instantiate(_downEnd, _bgParent);
            end.anchoredPosition = _parts[_parts.Count - 1].anchoredPosition +
                                   Vector2.down * _parts[_parts.Count - 1].sizeDelta.y;
            _parts.Add(end);

            _inited = true;
        }

        private void ClearParts()
        {
            if (_parts != null)
            {
                _parts.ForEach(a => Destroy(a.gameObject));
            }


            if (_points != null)
            {
                _points.ForEach(a => Destroy(a.gameObject));
            }
        }

        public void CompleteStage()
        {
            if (!_inited)
            {
                return;
            }

            if (_currentStage == _maxStage)
            {
                return;
            }

            _points[_currentStage].localScale = Vector3.zero;

            _points[_currentStage].gameObject.SetActive(true);

            _points[_currentStage].DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack);

            var fillerSizeDelta = _filler.sizeDelta;

            fillerSizeDelta.y = _nextSizeFiller;

            _filler.DOSizeDelta(fillerSizeDelta, 0.5f);

            _currentStage++;


            _amounLabel.text = $"{_currentStage}/{_maxStage}";


            if (_currentStage == _maxStage)
            {
                return;
            }

            var newFillerSizeDelta = _filler.sizeDelta;

            newFillerSizeDelta.y = Mathf.Abs(_points[_currentStage].anchoredPosition.y -
                                             _points[_currentStage - 1].anchoredPosition.y);

            _nextSizeFiller = Mathf.Abs(_points[_currentStage - 1].anchoredPosition.y - _filler.anchoredPosition.y) +
                              newFillerSizeDelta.y;

            newFillerSizeDelta.y = newFillerSizeDelta.y * 0.8f +
                                   Mathf.Abs(_points[_currentStage - 1].anchoredPosition.y -
                                             _filler.anchoredPosition.y);

            _filler.DOSizeDelta(newFillerSizeDelta, 0.5f)
                .SetDelay(0.5f);
        }
    }
}
