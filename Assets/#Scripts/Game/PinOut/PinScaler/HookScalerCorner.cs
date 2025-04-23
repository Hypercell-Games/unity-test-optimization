using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    [ExecuteInEditMode]
    public class HookScalerCorner : MonoBehaviour, IHookScaler
    {
        [SerializeField] private bool _scaleEnabled;
        [SerializeField] private List<HookScalerElement> _xElements;
        [SerializeField] private List<HookScalerElement> _yElements;
        [SerializeField] private List<HookScalerPositionItem> _xPositionItems;
        [SerializeField] private List<HookScalerPositionItem> _yPositionItems;
        [SerializeField] private List<ScalableColliderItem> _xColliders;
        [SerializeField] private List<ScalableColliderItem> _yColliders;
        [SerializeField] private List<ScalableXYColliderItem> _xyColliders;
        [SerializeField] private float _xScale;
        [SerializeField] private float _yScale;

        private float _lastScaleX, _lastScaleY;

        public float ScaleX => _xScale;
        public float ScaleY => _yScale;

        public bool ScaleEnabled => _scaleEnabled;

        private void Awake()
        {
            if (!_scaleEnabled)
            {
                return;
            }

            RebuildElements();
        }

        private void Update()
        {
            if (Application.IsPlaying(gameObject))
            {
                return;
            }

            CheckElementsValues();

            if (!_scaleEnabled)
            {
                return;
            }

            if (_lastScaleX != _xScale || _lastScaleY != _yScale)
            {
                RebuildElements();
            }
        }

        public float GetPinLenght()
        {
            return 15f + Mathf.Max(_xScale, _yScale);
        }

        private void CheckElementsValues()
        {
            _xElements.ForEach(a => a.Check());
            _yElements.ForEach(a => a.Check());
            _xPositionItems.ForEach(a => a.Check());
            _yPositionItems.ForEach(a => a.Check());
            _xColliders.ForEach(a => a.Check());
            _yColliders.ForEach(a => a.Check());
            _xyColliders.ForEach(a => a.Check());
        }

        private void RebuildElements()
        {
            _lastScaleX = _xScale;
            _lastScaleY = _yScale;

            _xElements.ForEach(a => a.Scale(_xScale));
            _yElements.ForEach(a => a.Scale(_yScale));
            _xPositionItems.ForEach(a => a.Scale(_xScale));
            _yPositionItems.ForEach(a => a.Scale(_yScale));
            _xColliders.ForEach(a => a.Scale(_xScale));
            _yColliders.ForEach(a => a.Scale(_yScale));
            _xyColliders.ForEach(a => a.Scale(new Vector2(_yScale, _xScale)));
        }

        public void UpdateValues(bool scaled, Vector2 scale)
        {
            _scaleEnabled = scaled;

            _xScale = scale.x;
            _yScale = scale.y;

            RebuildElements();
        }

        [Serializable]
        public class HookScalerElement
        {
            [SerializeField] private Transform _transform;
            [SerializeField] private Vector3 defaultPosition;
            [SerializeField] private float defaultSize;
            [SerializeField] private float height;

            private bool _isWaitRef;

            public void Check()
            {
                if (_transform == null)
                {
                    _isWaitRef = true;
                    return;
                }

                if (!_isWaitRef)
                {
                    return;
                }

                _isWaitRef = false;

                defaultPosition = _transform.localPosition;
                defaultSize = _transform.localScale.y;
            }

            public void Scale(float scale)
            {
                var scaleFactor = height / defaultSize;
                var targetScale = (height + scale) / scaleFactor;
                _transform.localScale = new Vector3(_transform.localScale.x, targetScale, _transform.localScale.z);
                _transform.localPosition = defaultPosition + Vector3.up * scale / 2f;
            }
        }

        [Serializable]
        public class HookScalerPositionItem
        {
            [SerializeField] private Transform _transform;
            [SerializeField] private Vector3 defaultPosition;
            [SerializeField] [Range(0f, 1f)] private float weight = 1f;

            private bool _isWaitRef;

            public void Check()
            {
                if (_transform == null)
                {
                    _isWaitRef = true;
                    return;
                }

                if (!_isWaitRef)
                {
                    return;
                }

                _isWaitRef = false;

                defaultPosition = _transform.localPosition;
            }

            public void Scale(float scale)
            {
                var pos = defaultPosition;
                pos.y += scale * weight;
                _transform.localPosition = pos;
            }
        }

        [Serializable]
        public class ScalableColliderItem
        {
            [SerializeField] private BoxCollider _collider;
            [SerializeField] private Vector3 defaultPosition;
            [SerializeField] private float defaultSize;

            private bool _isWaitRef;

            public void Check()
            {
                if (_collider == null)
                {
                    _isWaitRef = true;
                    return;
                }

                if (!_isWaitRef)
                {
                    return;
                }

                _isWaitRef = false;

                defaultPosition = _collider.transform.localPosition;
                defaultSize = _collider.size.y;
            }

            public void Scale(float scale)
            {
                var size = _collider.size;
                size.y = defaultSize + scale;
                _collider.size = size;
                _collider.transform.localPosition = defaultPosition + Vector3.up * (scale * 0.5f);
            }
        }

        [Serializable]
        public class ScalableXYColliderItem
        {
            [SerializeField] private BoxCollider _collider;
            [SerializeField] private Vector3 defaultPosition;
            [SerializeField] private Vector2 defaultSize;

            private bool _isWaitRef;

            public void Check()
            {
                if (_collider == null)
                {
                    _isWaitRef = true;
                    return;
                }

                if (!_isWaitRef)
                {
                    return;
                }

                _isWaitRef = false;

                defaultPosition = _collider.transform.localPosition;
                defaultSize = _collider.size;
            }

            public void Scale(Vector2 scale)
            {
                var size = _collider.size;
                size.x = defaultSize.x + scale.x;
                size.y = defaultSize.y + scale.y;
                _collider.size = size;
                _collider.transform.localPosition =
                    defaultPosition +
                    Vector3.right * (-scale.x * 0.5f) +
                    Vector3.up * (scale.y * 0.5f);
            }
        }
    }
}
