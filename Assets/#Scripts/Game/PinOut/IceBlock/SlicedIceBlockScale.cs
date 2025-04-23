using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    [ExecuteInEditMode]
    public class SlicedIceBlockScale : AbstrackIceBlockScale
    {
        [SerializeField] private float _scale;
        [SerializeField] private BoxCollider _boxCollider;
        [SerializeField] private List<Transform> _endAcnhors;
        [SerializeField] private List<Transform> _fillAnchors;
        [SerializeField] private float _defaultEndPos = 8f;
        [SerializeField] private float _defaultFillLenght = 12f;
        [SerializeField] private float _defaultColliderLenght = 20f;

        private float _lastScale;

#if UNITY_EDITOR
        private void Update()
        {
            if (_scale != _lastScale)
            {
                UpdateScale();
            }
        }
#endif

        public override Vector3 GetSize()
        {
            return new Vector3(_scale, 1, 1);
        }

        public override void SetSize(Vector3 size)
        {
            _scale = size.x;
            UpdateScale();
        }

        public void SetScale(float scale)
        {
            _scale = scale;
            UpdateScale();
        }

        private void UpdateScale()
        {
            _lastScale = _scale;

            foreach (var t in _endAcnhors)
            {
                var position = t.localPosition;
                position.y = Mathf.Sign(position.y) * (_defaultEndPos + _scale / 2f);
                t.localPosition = position;
            }

            foreach (var t in _fillAnchors)
            {
                var scale = t.localScale;
                scale.y = (_defaultFillLenght + _scale) / _defaultFillLenght;
                t.localScale = scale;
            }

            var colliderSize = _boxCollider.size;
            colliderSize.y = _defaultColliderLenght + _scale;
            _boxCollider.size = colliderSize;
        }
    }
}
