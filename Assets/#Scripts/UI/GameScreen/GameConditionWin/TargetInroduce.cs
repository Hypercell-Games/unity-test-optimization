using System.Collections;
using DG.Tweening;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unpuzzle
{
    public class TargetInroduce : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _count;
        [SerializeField] private RectTransform _targetTransform;
        [SerializeField] private Transform _introduceObject;
        [SerializeField] private CanvasGroup _subLayer;
        [SerializeField] private Transform _startPosition;
        [SerializeField] private HorizontalLayoutGroup _introduceLayoutAnchor;
        [SerializeField] private CanvasGroup _targetCanvas;
        [SerializeField] private CanvasGroup _flyTargetCanvas;
        [SerializeField] private GameObject _movesLimitRoot;
        [SerializeField] private GameObject _timeLimitRoot;
        [SerializeField] private CanvasGroup _limitGroup;
        [SerializeField] private TMP_Text _movesLimitText;
        [SerializeField] private TMP_Text _timeLimitText;

        public void ShowIntorduce(int count, PinOutLevelConfig levelInfo)
        {
            if (gameObject.scene.name == Scenes.GAME)
            {
                PauseUtil.BossLevelPaused();
            }

            _movesLimitRoot.SetActive(levelInfo.LevelMode == PinOutLevelConfig.LevelModeType.Moves);
            _timeLimitRoot.SetActive(levelInfo.LevelMode == PinOutLevelConfig.LevelModeType.Time);
            if (levelInfo.LevelMode == PinOutLevelConfig.LevelModeType.Moves)
            {
                _movesLimitText.text = levelInfo.moves.ToString();
            }
            else if (levelInfo.LevelMode == PinOutLevelConfig.LevelModeType.Time)
            {
                _timeLimitText.text = TimeCountWidget.GetTimeStr(levelInfo.time);
            }

            _flyTargetCanvas.alpha = 1f;
            _targetCanvas.alpha = 0f;

            _count.text = $"{count}";

            _subLayer.alpha = 1f;
            _limitGroup.alpha = 1f;
            _limitGroup.transform.localScale = Vector3.one;

            _introduceLayoutAnchor.transform.localScale = Vector3.zero;
            _introduceObject.localScale = _startPosition.localScale;
            _introduceObject.position = _startPosition.position;

            gameObject.SetActive(true);
            _introduceLayoutAnchor.enabled = true;

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_introduceLayoutAnchor.transform);

            StartCoroutine(ShowAnimation());
        }

        private IEnumerator ShowAnimation()
        {
            _introduceLayoutAnchor.transform.DOScale(1f, 0.5f);

            yield return new WaitForSeconds(2.0f);


            _introduceLayoutAnchor.enabled = false;

            _subLayer.DOFade(0f, 0.5f);

            _introduceObject.DOMove(_targetTransform.transform.position, 0.7f)
                .SetEase(Ease.InCubic);

            _introduceObject.DOScale(_targetTransform.localScale, 0.4f)
                .SetDelay(0.3f);

            _flyTargetCanvas.DOFade(0f, 0.1f)
                .SetDelay(0.6f);
            _limitGroup.DOFade(0f, 0.5f);
            _limitGroup.transform.DOScale(_targetTransform.localScale.x / _introduceObject.localScale.x, 0.4f)
                .SetDelay(0.3f);

            yield return new WaitForSeconds(0.7f);

            _targetCanvas.DOFade(1f, 0.5f)
                .SetLink(_targetCanvas.gameObject)
                .OnComplete(() =>
                {
                    if (gameObject.scene.name == Scenes.GAME)
                    {
                        PauseUtil.BossLevelUnpaused();
                    }
                });
            gameObject.SetActive(false);
        }
    }
}
