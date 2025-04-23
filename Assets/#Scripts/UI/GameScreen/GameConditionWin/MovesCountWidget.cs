using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unpuzzle;

public class MovesCountWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _movesCountText;
    [SerializeField] private TextMeshProUGUI _movesCountTarget;
    [SerializeField] private TextMeshProUGUI _targetCount;
    [SerializeField] private RectTransform _starIcon;
    [SerializeField] private GameObject _targetWidget;
    [SerializeField] private RectTransform _starFlyParent;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _starPrefab;
    [SerializeField] private TargetInroduce _targetIntroduce;
    [SerializeField] private Camera _gameUiCamera;
    [SerializeField] private Transform _starParent;
    [SerializeField] private Transform _fakeStarParent;
    [SerializeField] private Transform _anchor;
    [SerializeField] private Image _progressImage;

    [SerializeField] private TrailRenderer _starTrail;

    private int _currentCount;
    private Vector3 _defaultScale;
    private int _maxTarget;

    private TextMeshProUGUI _moveCountLabel;

    private int _targetLeft;

    public RectTransform StarIcon => _starIcon;

    private void Awake()
    {
        _defaultScale = _starIcon.localScale;
    }

    public void SetActive(bool isActive, bool isTarget)
    {
        _moveCountLabel = isTarget ? _movesCountTarget : _movesCountText;

        _targetCount.gameObject.SetActive(isTarget);
        _movesCountTarget.gameObject.SetActive(isTarget);
        _movesCountText.gameObject.SetActive(!isTarget);
        _targetWidget.SetActive(isTarget);

        _moveCountLabel.gameObject.SetActive(isActive);
        _progressImage.fillAmount = 0.12f;
    }

    public void ShowIntroduce(PinOutLevelConfig levelInfo)
    {
        _targetIntroduce.ShowIntorduce(_maxTarget, levelInfo);
    }

    public void UpdateTarget(int count, bool maxValue)
    {
        if (maxValue)
        {
            _targetLeft = 0;
            _maxTarget = count;
        }
        else
        {
            _targetLeft = count;
        }

        _targetCount.text = $"{_targetLeft}/{_maxTarget}";
        UpdateProgressBar(_targetLeft, _maxTarget);
    }

    private void UpdateProgressBar(float value, float maxValue)
    {
        if (_maxTarget > 0)
        {
            _progressImage.fillAmount = Mathf.Lerp(0.1f, 1f, (float)_targetLeft / _maxTarget);
        }
        else
        {
            _progressImage.fillAmount = 0.1f;
        }
    }

    public void UpdateMoves(int count, bool tweenValue)
    {
        if (!tweenValue)
        {
            SetCount(count);
            return;
        }

        DOTween.Kill(gameObject);
        DOVirtual.Int(_currentCount, count, 0.2f, SetCount).SetEase(Ease.OutSine).SetLink(gameObject);
    }

    private void SetCount(int count)
    {
        _currentCount = count;

        var countText = count == int.MaxValue ? "?" : count.ToString();
        _moveCountLabel.text = $"Moves {countText}";
    }

    public void FlyStar(Transform star, float delay)
    {
        star.SetParent(_fakeStarParent);

        var localPosition = star.localPosition;
        var localRotation = star.localRotation;

        star.SetParent(_starParent);

        var starChild = star.childCount;
        for (var i = 0; i < starChild; i++)
        {
            star.GetChild(i).gameObject.SetActive(false);
        }

        star.localPosition = localPosition;
        star.localRotation = localRotation;

        var startPositon = star.localPosition;

        startPositon = _gameUiCamera.ScreenPointToRay(_gameUiCamera.WorldToScreenPoint(star.position))
            .GetPoint(_starParent.localPosition.z);

        startPositon = _starParent.InverseTransformPoint(startPositon);

        var endPosition = _starIcon.position;
        endPosition = _starParent.InverseTransformPoint(endPosition);

        var middlePos = endPosition;

        var timeToFly = Mathf.Max(Vector3.Distance(endPosition, startPositon) / 30f, 0.3f);
        middlePos.x = Mathf.Lerp(startPositon.x, endPosition.x, 0.5f);
        middlePos.y = Mathf.Lerp(startPositon.y, endPosition.y, 0.3f);


        star.DOLocalMove(startPositon, 0.2f);
        star.DOLocalRotate(new Vector3(0f, 90f, 0f), 0.2f);

        star.DOScale(1.1f, 0.4f)
            .SetDelay(0.2f)
            .OnComplete(() =>
            {
                var trail = Instantiate(_starTrail, star.position, Quaternion.identity, _starParent);

                DOTween.To(x =>
                    {
                        if (star != null)
                        {
                            trail.transform.position = star.position;
                        }

                        trail.widthMultiplier = x;
                    }, 1f, 0.42f, timeToFly - 0.1f)
                    .SetEase(Ease.OutSine)
                    .SetUpdate(UpdateType.Late)
                    .OnComplete(() =>
                    {
                        DOTween.To(x =>
                            {
                                if (star != null)
                                {
                                    trail.transform.position = star.position;
                                }
                            }, 0f, 1f, 1f)
                            .SetUpdate(UpdateType.Late)
                            .OnComplete(() =>
                            {
                                Destroy(trail.gameObject);
                            });
                    });

                star.DOLocalPath(new[] { middlePos, endPosition }, timeToFly, PathType.CatmullRom,
                        PathMode.Sidescroller2D)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        _starIcon.DOKill();
                        _starIcon.localScale = _defaultScale;
                        _starIcon.DOScale(_defaultScale * 1.2f, 0.1f)
                            .SetLoops(2, LoopType.Yoyo);
                        UpdateTarget(_targetLeft + 1, false);
                        Destroy(star.gameObject);
                    });

                star.DOScale(0.42f, timeToFly - 0.1f)
                    .SetEase(Ease.OutSine);
            });
    }
}
