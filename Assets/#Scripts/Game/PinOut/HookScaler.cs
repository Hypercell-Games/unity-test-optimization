using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    [ExecuteInEditMode]
    public class HookScaler : MonoBehaviour, IHookScaler
    {
        [SerializeField] private bool _scaleEnabled;
        [SerializeField] private float _scale = 1f;
        [SerializeField] private List<HookScalerElement> _scalableParts;
        [SerializeField] private List<ScalableColliderItem> _scalableColliders;
        [SerializeField] private List<HookScalerPositionItem> _nonScalableParts;

        private float _lastScale;

        public float Scale => _scale;

        public bool ScaleEnabled => _scaleEnabled;

        private void Awake()
        {
            if (!_scaleEnabled)
            {
                return;
            }

            _lastScale = _scale;
            _scalableParts.ForEach(a => a.Scale(_scale));
            _nonScalableParts.ForEach(a => a.Scale(_scale));
            _scalableColliders.ForEach(a => a.Scale(_scale));
        }

        private void Update()
        {
            if (Application.IsPlaying(gameObject))
            {
                return;
            }

            _scalableColliders.ForEach(a => a.Check());
            _scalableParts.ForEach(a => a.Check());
            _nonScalableParts.ForEach(a => a.Check());

            if (!_scaleEnabled)
            {
                return;
            }

            if (_scale != _lastScale)
            {
                _lastScale = _scale;
                _scalableParts.ForEach(a => a.Scale(_scale));
                _nonScalableParts.ForEach(a => a.Scale(_scale));
                _scalableColliders.ForEach(a => a.Scale(_scale));
            }
        }

        public float GetPinLenght()
        {
            return 15f + Scale;
        }

        public void UpdateValues(bool enabled, float scale)
        {
            _scaleEnabled = enabled;

            _scale = _scaleEnabled ? scale : 1f;
            _lastScale = _scale;
            _scalableParts.ForEach(a => a.Scale(_scale));
            _nonScalableParts.ForEach(a => a.Scale(_scale));
            _scalableColliders.ForEach(a => a.Scale(_scale));
        }

        public void SetScale(float value)
        {
            UpdateValues(true, value);
        }
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

        public void Scale(float scale, bool positionChangeToo = false)
        {
            var heightChange = (height + (scale - 1) * 2f) / height;
            _transform.localScale =
                new Vector3(_transform.localScale.x, defaultSize * heightChange, _transform.localScale.z);

            if (positionChangeToo)
            {
                _transform.localPosition = new Vector3(_transform.localPosition.x,
                    defaultPosition.y + Mathf.Sign(defaultPosition.y) * (scale - 1), _transform.localPosition.z);
            }
        }
    }

    [Serializable]
    public class HookScalerPositionItem
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 defaultPosition;

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
            pos.y += scale * Mathf.Sign(defaultPosition.y) - 1 * Mathf.Sign(defaultPosition.y);
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

        public void Scale(float scale, bool positionChangeToo = false)
        {
            var size = _collider.size;
            size.y = defaultSize + (scale - 1) * 2f;
            _collider.size = size;

            if (positionChangeToo)
            {
                _collider.transform.localPosition = new Vector3(_collider.transform.localPosition.x,
                    defaultPosition.y + Mathf.Sign(defaultPosition.y) * (scale - 1),
                    _collider.transform.localPosition.z);
            }
        }
    }
}
