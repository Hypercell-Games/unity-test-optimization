using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Unpuzzle.UI.NewTabBar;

namespace Unpuzzle
{
    public class MapItemBoss : MonoBehaviour, IDotAnimation
    {
        [SerializeField] private Image _boss;
        [SerializeField] private Image _challenge;
        [SerializeField] private Image _dotColored;

        private Sequence _sequence;

        public void InitDotAnimation()
        {
            _sequence?.Kill();
            Init(true);

            transform.localScale = Vector3.one;
        }

        public (float, Sequence) ShowDotAnimation(Action onCupFlyStarted, Sequence mainSeq,
            NewTabBarScreen tabBarScreen)
        {
            _sequence?.Kill();


            transform.localScale = Vector3.one;
            _sequence = DOTween.Sequence()
                .SetLink(gameObject)
                .OnStart(() => Init(false))
                .Append(transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 3).SetEase(Ease.OutQuad))
                .AppendInterval(0.02f);
            var duration = 0.2f;

            return (duration, _sequence);
        }

        public void Init(bool boss, bool challenge, bool prev, RectTransform flyCups)
        {
            _dotColored.gameObject.SetActive(false);
            _boss.gameObject.SetActive(boss);
            _challenge.gameObject.SetActive(challenge && !boss);
            Init(prev);
        }

        private void Init(bool prev)
        {
            if (prev)
            {
                _boss.SetAlpha(0.5f);
                _challenge.SetAlpha(0.5f);
            }
            else
            {
                _boss.SetAlpha(1f);
                _challenge.SetAlpha(1f);
            }
        }
    }
}
