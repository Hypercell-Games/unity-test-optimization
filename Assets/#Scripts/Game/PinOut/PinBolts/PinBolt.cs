using System;
using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    public class PinBolt : MonoBehaviour, ICollisionElement
    {
        [SerializeField] private BoxCollider _collider;

        private bool _isDestroyed;

        public void TouchFeedack()
        {
        }

        public void StartFire()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
        }

        public void Press()
        {
            transform.DOScale(0.8f, 0.1f);
        }

        public void Up()
        {
            transform.DOScale(1f, 0.1f);
        }

        public void BoltDestroy()
        {
            if (_isDestroyed)
            {
                return;
            }

            _isDestroyed = true;

            transform.DOMove(transform.position + transform.up * 5f, 0.3f);
            transform.DOLocalRotate(-Vector3.up * 270f, 0.3f, RotateMode.LocalAxisAdd);
            transform.DOScale(0f, 0.2f)
                .SetDelay(0.1f);
        }
    }
}
