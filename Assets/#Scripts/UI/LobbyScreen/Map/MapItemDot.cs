using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Unpuzzle.UI.NewTabBar;

namespace Unpuzzle
{
    public class MapItemDot : MonoBehaviour, IDotAnimation
    {
        [SerializeField] private Image _dotGrayImage;
        [SerializeField] private Image _dotColorImage;

        private Sequence _sequence;

        public void InitDotAnimation()
        {
            _sequence?.Kill();
            Init(false);
        }

        public (float, Sequence) ShowDotAnimation(Action onCupFlyStarted, Sequence mainSeq,
            NewTabBarScreen tabBarScreen)
        {
            _sequence?.Kill();


            _sequence = DOTween.Sequence()
                .OnStart(() => Init(true))
                .SetLink(gameObject)
                .Append(transform.DOPunchScale(Vector3.one * 0.267f, 0.4f, 3).SetEase(Ease.OutQuad));
            return (0.2f, _sequence);
        }

        public void Init(bool colored)
        {
            _dotGrayImage.gameObject.SetActive(!colored);
            _dotColorImage.gameObject.SetActive(colored);
        }
    }
}

public interface IDotAnimation
{
    void InitDotAnimation();

    (float duration, Sequence seq) ShowDotAnimation(Action onCupFlyStarted, Sequence mainSeq,
        NewTabBarScreen tabBarScreen);
}
