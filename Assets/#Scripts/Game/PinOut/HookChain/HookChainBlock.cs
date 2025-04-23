using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    public class HookChainBlock : AbstractHookChainElement
    {
        [SerializeField] private Transform _nextPos;
        [SerializeField] private GameObject _cap;
        [SerializeField] private float _defaultSize;
        [SerializeField] private float _defaultLenght;
        [SerializeField] private float _defaultPos;
        [SerializeField] private Transform _downAnchor;
        [SerializeField] private List<Transform> _tubes;
        [SerializeField] private List<BoxCollider> _boxColliders;
        [SerializeField] private float _defaultColliderSize;

        public override float Lenght()
        {
            return 1f;
        }

        public override Vector3 NextChainGlobalPosition()
        {
            return _nextPos.position;
        }

        public override Vector3 NextChainLocalPosition()
        {
            return transform.localPosition + _nextPos.localPosition + _downAnchor.localPosition;
        }

        public override void SetCapActive(bool active)
        {
            _cap.SetActive(active);
        }

        public override float GetSerializeScale()
        {
            var targetScale = _tubes[0].localScale.y;

            var scale = (targetScale - _defaultSize) * 3.2f;

            return scale;
        }

        public override void SetScale(float scale)
        {
            _downAnchor.localPosition = Vector3.down * scale;

            var targetLenght = scale + _defaultLenght;
            var targetScale = scale / 3.2f + _defaultSize;
            var lenghtByScale = _defaultLenght / _defaultSize;
            var tubeScale = Vector3.one;
            tubeScale.y = targetScale;

            var offset = scale / 2f;

            _boxColliders.ForEach(a =>
            {
                var pos = a.transform.localPosition;
                pos.y = _defaultPos - offset;
                a.transform.localPosition = pos;
                a.size = new Vector3(1, _defaultColliderSize + scale, 1);
            });

            _tubes.ForEach(a =>
            {
                var pos = a.localPosition;
                pos.y = _defaultPos - offset;
                a.localScale = tubeScale;
                a.localPosition = pos;
            });
        }
    }
}
