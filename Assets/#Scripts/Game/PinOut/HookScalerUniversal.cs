using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    [ExecuteInEditMode]
    public class HookScalerUniversal : MonoBehaviour, IHookScaler
    {
        [SerializeField] private bool _scaleEnabled;
        [SerializeField] private float _verticalScale = 1f;
        [SerializeField] private float _horizonatlScale = 1f;
        [SerializeField] private List<HookScalerElementUniversal> _scalableParts;
        [SerializeField] private List<ScalableColliderItemUniversal> _scalableColliders;
        [SerializeField] private List<HookScalerPositionItemQuad> _nonScalableParts;
        [SerializeField] private ScalableHiddenPinColliderUniversal _scalableHiddenPinCollider;
        [SerializeField] private TrailRenderer _trail;
        private float _lastHorizontal;

        private float _lastVertical;

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
    public class HookScalerElementUniversal
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 defaultPosition;
        [SerializeField] private Vector3 defaultSize;
        [SerializeField] private float height;
        [SerializeField] private float width;
        [SerializeField] private bool _horizontalScaling;
        [SerializeField] private bool _verticalScaling;
        [SerializeField] private bool _horizontalMoving;
        [SerializeField] private bool _verticalMoving;

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

            if (_verticalMoving)
            {
                pos.y += vertical * Mathf.Sign(defaultPosition.y) - 1 * Mathf.Sign(defaultPosition.y);
            }

            if (_horizontalMoving)
            {
                pos.x += horizontal * Mathf.Sign(defaultPosition.x) - 1 * Mathf.Sign(defaultPosition.x);
            }

            _transform.localPosition = pos;
            _transform.localScale = new Vector3(defaultSize.x * widthChange, defaultSize.y * heightChange,
                _transform.localScale.z);
        }
    }

    [Serializable]
    public class HookScalerPositionItemUniversal
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 defaultPosition;
        [SerializeField] private bool _horizontalChange;
        [SerializeField] private bool _verticalChange;
        [SerializeField] private bool _horizontalMoving;
        [SerializeField] private bool _verticalMoving;

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
            if (_verticalMoving)
            {
                pos.y += verical * Mathf.Sign(defaultPosition.y) - 1 * Mathf.Sign(defaultPosition.y);
            }

            if (_horizontalMoving)
            {
                pos.x += horizontal * Mathf.Sign(defaultPosition.x) - 1 * Mathf.Sign(defaultPosition.x);
            }

            _transform.localPosition = pos;
        }
    }

    [Serializable]
    public class ScalableHiddenPinColliderUniversal
    {
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private Vector3 defaultSize;
        [SerializeField] private bool _horizontalScaling;
        [SerializeField] private bool _verticalScaling;
        [SerializeField] private bool _horizontalMoving;
        [SerializeField] private bool _verticalMoving;

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
    public class ScalableColliderItemUniversal
    {
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private Vector3 defaultPosition;
        [SerializeField] private Vector3 defaultSize;
        [SerializeField] private bool _horizontalScaling;
        [SerializeField] [Range(0f, 10f)] private float _horizontalScalingFactor = 2f;
        [SerializeField] [Range(0f, 1f)] private float _horizontalPivot = 0.5f;
        [SerializeField] private bool _verticalScaling;
        [SerializeField] [Range(0f, 10f)] private float _verticalScalingFactor = 2f;
        [SerializeField] [Range(0f, 1f)] private float _verticalPivot = 0.5f;
        [SerializeField] private bool _horizontalMoving;
        [SerializeField] private bool _verticalMoving;

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

            if (_verticalMoving)
            {
                pos.y += vertical * Mathf.Sign(defaultPosition.y) - 1 * Mathf.Sign(defaultPosition.y);
            }

            if (_horizontalMoving)
            {
                pos.x += horizontal * Mathf.Sign(defaultPosition.x) - 1 * Mathf.Sign(defaultPosition.x);
            }

            _collider.transform.localPosition = pos;

            var size = _collider.size;
            var center = _collider.center;
            if (_verticalScaling)
            {
                size.y = defaultSize.y + (vertical - 1) * _verticalScalingFactor;
                center.y = size.y * Mathf.Lerp(-0.5f, 0.5f, _verticalPivot);
            }

            if (_horizontalScaling)
            {
                size.x = defaultSize.x + (horizontal - 1) * _horizontalScalingFactor;
                center.x = size.x * Mathf.Lerp(-0.5f, 0.5f, _horizontalPivot);
            }

            _collider.size = size;
            _collider.center = center;
        }
    }
}
