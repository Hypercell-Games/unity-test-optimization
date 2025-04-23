using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    [ExecuteInEditMode]
    public class HookScalerPartLine : MonoBehaviour, IHookScaler
    {
        [SerializeField] private bool _scaleEnabled;
        [SerializeField] private float _scaleLeft = 1f;
        [SerializeField] private float _scaleRight = 1f;
        [SerializeField] private List<HookScalerElement> _scalablePartsLeft;
        [SerializeField] private List<HookScalerElement> _scalablePartsRight;
        [SerializeField] private List<ScalableColliderItem> _scalableCollidersLeft;
        [SerializeField] private List<ScalableColliderItem> _scalableCollidersRight;
        [SerializeField] private List<HookScalerPartLinePositionItem> _nonScalablePartsLeft;
        [SerializeField] private List<HookScalerPartLinePositionItem> _nonScalablePartsRight;
        [SerializeField] private List<HookScallerMiddlePart> _nonScalMiddlePart;

        private float _lastScaleLeft, _lastScaleRight;

        public float LeftScale => _scaleLeft;
        public float RightScale => _scaleRight;

        public bool ScaleEnabled => _scaleEnabled;

        private void Awake()
        {
            if (!_scaleEnabled)
            {
                return;
            }

            _lastScaleLeft = _scaleLeft;
            _lastScaleRight = _scaleRight;
            _scalablePartsLeft.ForEach(a => a.Scale(_scaleLeft, true));
            _scalableCollidersLeft.ForEach(a => a.Scale(_scaleLeft, true));
            _nonScalablePartsLeft.ForEach(a => a.Scale(_scaleLeft));


            _scalablePartsRight.ForEach(a => a.Scale(_scaleRight, true));
            _scalableCollidersRight.ForEach(a => a.Scale(_scaleRight, true));
            _nonScalablePartsRight.ForEach(a => a.Scale(_scaleRight));

            _nonScalMiddlePart.ForEach(a => a.Scale(_scaleLeft, _scaleRight));
        }

        private void Update()
        {
            if (Application.IsPlaying(gameObject))
            {
                return;
            }

            _scalableCollidersLeft.ForEach(a => a.Check());
            _scalableCollidersRight.ForEach(a => a.Check());
            _scalablePartsRight.ForEach(a => a.Check());
            _scalablePartsLeft.ForEach(a => a.Check());
            _nonScalablePartsLeft.ForEach(a => a.Check());
            _nonScalablePartsRight.ForEach(a => a.Check());
            _nonScalMiddlePart.ForEach(a => a.Check());

            if (!_scaleEnabled)
            {
                return;
            }

            if (_scaleLeft != _lastScaleLeft || _lastScaleRight != _scaleRight)
            {
                _lastScaleLeft = _scaleLeft;
                _lastScaleRight = _scaleRight;

                _scalablePartsLeft.ForEach(a => a.Scale(_scaleLeft, true));
                _scalableCollidersLeft.ForEach(a => a.Scale(_scaleLeft, true));
                _nonScalablePartsLeft.ForEach(a => a.Scale(_scaleLeft));

                _scalableCollidersRight.ForEach(a => a.Scale(_scaleRight, true));
                _scalablePartsRight.ForEach(a => a.Scale(_scaleRight, true));
                _nonScalablePartsRight.ForEach(a => a.Scale(_scaleRight));


                _nonScalMiddlePart.ForEach(a => a.Scale(_scaleLeft, _scaleRight));
            }
        }

        public float GetPinLenght()
        {
            return 15f + _scaleRight + _scaleLeft;
        }

        public void UpdateValues(bool enabled, float scaleLeft, float scaleRight)
        {
            _scaleEnabled = enabled;

            _scaleLeft = _scaleEnabled ? scaleLeft : 1f;
            _scaleRight = _scaleEnabled ? scaleRight : 1f;

            _lastScaleLeft = _scaleLeft;
            _lastScaleRight = _scaleRight;

            _scalablePartsLeft.ForEach(a => a.Scale(_scaleLeft, true));
            _scalableCollidersLeft.ForEach(a => a.Scale(_scaleLeft, true));
            _nonScalablePartsLeft.ForEach(a => a.Scale(_scaleLeft));

            _scalableCollidersRight.ForEach(a => a.Scale(_scaleRight, true));
            _scalablePartsRight.ForEach(a => a.Scale(_scaleRight, true));
            _nonScalablePartsRight.ForEach(a => a.Scale(_scaleRight));

            _nonScalMiddlePart.ForEach(a => a.Scale(_scaleLeft, _scaleRight));
        }
    }

    [Serializable]
    public class HookScalerPartLinePositionItem
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
            pos.y += (scale - 1) * 2 * Mathf.Sign(defaultPosition.y);
            _transform.localPosition = pos;
        }
    }

    [Serializable]
    public class HookScallerMiddlePart
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

        public void Scale(float scaleLeft, float scaleRight)
        {
            var pos = defaultPosition;
            pos.y += scaleLeft - 1 - (scaleRight - 1);
            _transform.localPosition = pos;
        }
    }
}
