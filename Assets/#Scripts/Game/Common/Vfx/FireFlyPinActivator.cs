using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    public class FireFlyPinActivator : MonoBehaviour
    {
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private List<ParticleSystem> _particles;

        private void Awake()
        {
            _trailRenderer.enabled = false;
        }

        public void StartFly(Vector3 start, Vector3 end, Vector3 moveVector, Action onComplete)
        {
            var speed = 90f;
            _trailRenderer.enabled = true;
            transform.position = start;

            var middlePoint = Vector3.Lerp(start, end, 0.5f);

            var radiusModifier = middlePoint.magnitude / 2f;

            middlePoint += moveVector * radiusModifier;

            var bezierA = start + moveVector * radiusModifier * 2f;
            var bezierD = end + (start - end).normalized * radiusModifier;

            var bezierB = middlePoint + (bezierA - middlePoint) / 2f + moveVector * radiusModifier / 2f;
            var bezierC = middlePoint - (bezierA - middlePoint);

            transform.DOPath(new[] { middlePoint, bezierA, bezierB, end, bezierC, bezierD }, speed,
                    PathType.CubicBezier, PathMode.Ignore)
                .SetSpeedBased()
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    _particles.ForEach(a => a.Stop());
                    Destroy(gameObject, 2f);
                });
        }
    }
}
