using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    [ExecuteInEditMode]
    public class HookScalerQuad : MonoBehaviour, IHookScaler
    {
        [SerializeField] private bool _scaleEnabled;
        [SerializeField] private float _verticalScale = 1f;
        [SerializeField] private float _horizonatlScale = 1f;
        [SerializeField] private List<HookScalerElementQuad> _scalableParts;
        [SerializeField] private List<ScalableColliderItemQuad> _scalableColliders;
        [SerializeField] private List<HookScalerPositionItemQuad> _nonScalableParts;
        [SerializeField] private ScalableHiddenPinCollider _scalableHiddenPinCollider;
        [SerializeField] private TrailRenderer _trail;

        private float _lastVertical, _lastHorizontal;

        public float Scale => _verticalScale;

        public float HorizontalScale => _horizonatlScale;
        public float VerticalScale => _verticalScale;

        public bool ScaleEnabled => _scaleEnabled;

        private void Awake()
        {
            if (!_scaleEnabled)
            {
                return;
            }

            _lastVertical = _verticalScale;
            _lastHorizontal = _horizonatlScale;
            _scalableParts.ForEach(a => a.Scale(_verticalScale, _horizonatlScale));
            _nonScalableParts.ForEach(a => a.Scale(_verticalScale, _horizonatlScale));
            _scalableColliders.ForEach(a => a.Scale(_verticalScale, _horizonatlScale));
            _scalableHiddenPinCollider.Scale(_verticalScale, _horizonatlScale);
            _trail.startWidth = 17f + (_horizonatlScale - 1) * 2f;
            _trail.endWidth = _trail.startWidth;
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
            _scalableHiddenPinCollider.Check();

            if (!_scaleEnabled)
            {
                return;
            }

            if (_verticalScale != _lastVertical || _horizonatlScale != _lastHorizontal)
            {
                _lastVertical = _verticalScale;
                _lastHorizontal = _horizonatlScale;

                _scalableParts.ForEach(a => a.Scale(_verticalScale, _horizonatlScale));
                _nonScalableParts.ForEach(a => a.Scale(_verticalScale, _horizonatlScale));
                _scalableColliders.ForEach(a => a.Scale(_verticalScale, _horizonatlScale));
                _scalableHiddenPinCollider.Scale(_verticalScale, _horizonatlScale);
                _trail.startWidth = 17f + (_horizonatlScale - 1) * 2f;
                _trail.endWidth = _trail.startWidth;
            }
        }

        public float GetPinLenght()
        {
            return 15f + _verticalScale;
        }

        public void UpdateValues(bool scaled, Vector2 scale)
        {
            _scaleEnabled = scaled;

            _verticalScale = _scaleEnabled ? scale.y : 1f;
            _horizonatlScale = _scaleEnabled ? scale.x : 1f;

            _lastVertical = _verticalScale;
            _lastHorizontal = _horizonatlScale;
            _scalableParts.ForEach(a => a.Scale(_verticalScale, _horizonatlScale));
            _nonScalableParts.ForEach(a => a.Scale(_verticalScale, _horizonatlScale));
            _scalableColliders.ForEach(a => a.Scale(_verticalScale, _horizonatlScale));
            _scalableHiddenPinCollider.Scale(_verticalScale, _horizonatlScale);
            _trail.startWidth = 17f + (_horizonatlScale - 1) * 2f;
            _trail.endWidth = _trail.startWidth;
        }
    }

    [Serializable]
    public class HookScalerElementQuad
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 defaultPosition;
        [SerializeField] private Vector3 defaultSize;
        [SerializeField] private float height;
        [SerializeField] private float width;
        [SerializeField] private bool _horizontalScaling;
        [SerializeField] private bool _verticalScaling;

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
            defaultSize = _transform.localScale;
        }

        public void Scale(float vertical, float horizontal)
        {
            var heightChange = _verticalScaling ? (height + (vertical - 1) * 2f) / height : 1;
            var widthChange = _horizontalScaling ? (width + (horizontal - 1) * 2f) / width : 1f;
            var pos = defaultPosition;

            if (_horizontalScaling)
            {
                pos.y += vertical * Mathf.Sign(defaultPosition.y) - 1 * Mathf.Sign(defaultPosition.y);
            }

            if (_verticalScaling)
            {
                pos.x += horizontal * Mathf.Sign(defaultPosition.x) - 1 * Mathf.Sign(defaultPosition.x);
            }

            _transform.localPosition = pos;
            _transform.localScale = new Vector3(defaultSize.x * widthChange, defaultSize.y * heightChange,
                _transform.localScale.z);
        }
    }

    [Serializable]
    public class HookScalerPositionItemQuad
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 defaultPosition;
        [SerializeField] private bool _horizontalChange;
        [SerializeField] private bool _verticalChange;

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

        public void Scale(float verical, float horizontal)
        {
            var pos = defaultPosition;
            if (_verticalChange)
            {
                pos.y += verical * Mathf.Sign(defaultPosition.y) - 1 * Mathf.Sign(defaultPosition.y);
            }

            if (_horizontalChange)
            {
                pos.x += horizontal * Mathf.Sign(defaultPosition.x) - 1 * Mathf.Sign(defaultPosition.x);
            }

            _transform.localPosition = pos;
        }
    }

    [Serializable]
    public class ScalableHiddenPinCollider
    {
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private Vector3 defaultSize;
        [SerializeField] private bool _horizontalScaling;
        [SerializeField] private bool _verticalScaling;

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

            defaultSize = _collider.size;
        }

        public void Scale(float vertical, float horizontal)
        {
            var size = _collider.size;
            if (_verticalScaling)
            {
                size.y = defaultSize.y + (vertical - 1) * 2f;
            }

            if (_horizontalScaling)
            {
                size.x = defaultSize.x + (horizontal - 1) * 2f;
            }

            _collider.size = size;
        }
    }

    [Serializable]
    public class ScalableColliderItemQuad
    {
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private Vector3 defaultPosition;
        [SerializeField] private Vector3 defaultSize;
        [SerializeField] private bool _horizontalScaling;
        [SerializeField] private bool _verticalScaling;
        [SerializeField] private bool _isCenterHorizontal;
        [SerializeField] private bool _isCenterVertical;

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

        public void Scale(float vertical, float horizontal)
        {
            var pos = defaultPosition;

            if (_verticalScaling && !_isCenterHorizontal)
            {
                pos.x += horizontal * Mathf.Sign(defaultPosition.x) - 1 * Mathf.Sign(defaultPosition.x);
            }

            if (_horizontalScaling && !_isCenterVertical)
            {
                pos.y += vertical * Mathf.Sign(defaultPosition.y) - 1 * Mathf.Sign(defaultPosition.y);
            }

            _collider.transform.localPosition = pos;

            var size = _collider.size;

            if (_horizontalScaling)
            {
                size.x = defaultSize.x + (horizontal - 1) * 2f;
            }

            if (_verticalScaling)
            {
                size.y = defaultSize.y + (vertical - 1) * 2f;
            }

            _collider.size = size;
        }
    }
}
