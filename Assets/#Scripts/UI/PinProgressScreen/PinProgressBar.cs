using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unpuzzle
{
    public class PinProgressBar : MonoBehaviour
    {
        public const string PROGRESS_TEXT = "<#333B66>{0}</color>/{1}";

        [SerializeField] private Slider _progress;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private RectTransform _prizeBox;
        [SerializeField] private ParticleSystem _stars;

        private int _collected;
        private int _full;

        public RectTransform PrizeBox => _prizeBox;

        public void SetProgress(int collected, int full)
        {
            _collected = collected;
            _full = full;
            _text.text = string.Format(PROGRESS_TEXT, collected, full);
            _progress.value = (float)collected / full;
            _progress.DOKill();
            _progress.DOValue((float)_collected / _full, 0.2f);
        }

        public void AddCollected(int count)
        {
            _collected += count;

            if (_collected > _full)
            {
                _collected = _full;
            }

            _text.text = string.Format(PROGRESS_TEXT, _collected, _full);
            _progress.DOKill();
            _progress.DOValue((float)_collected / _full, 0.2f);

            _prizeBox.DOKill();
            _prizeBox.transform.localScale = Vector3.one;
            _prizeBox.DOPunchScale(Vector3.one * 0.2f, 0.3f);

            _stars.Play();
        }
    }
}
