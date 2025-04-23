using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    [ExecuteInEditMode]
    public class HookColoringEdtior : MonoBehaviour
    {
#if UNITY_EDITOR
        private static ColorHolder _colorHolder;
        private PinColorID _overridedColorId;
        private HookController _hookController;
        private static int _colorIndex;
        private PinMaterialId _overridedMaterialId;
        public List<HookController> _lastBlockedPins = new();
        public List<HookController> _lastBlockers = new();
        private bool _isBlocked;
        private bool _isGhost;
        private bool _isFire;
#endif

#if UNITY_EDITOR

        private void Start()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_colorHolder == null)
            {
                _colorHolder = Resources.Load<ColorHolder>("ColorHolder");
            }

            _hookController = GetComponent<HookController>();

            if (_hookController == null)
            {
                return;
            }

            UpdateColors();
        }

        public void UpdateColors()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_hookController == null)
            {
                return;
            }

            if (_colorHolder == null)
            {
                _colorHolder = Resources.Load<ColorHolder>("ColorHolder");
            }

            _overridedColorId = _hookController.OverrideColor;
            _overridedMaterialId = _hookController.OverridedMaterial;
            _isBlocked = _lastBlockers.Count > 0;
            _isGhost = _hookController.IsGhost;
            _isFire = _hookController.IsFire;

            if (_isGhost)
            {
                SetMaterial(_colorHolder.TransparentMaterial);
                return;
            }

            if (_isFire)
            {
                SetMaterial(_colorHolder.FireMaterial);
                return;
            }

            if (_isBlocked)
            {
                SetMaterial(_colorHolder.GetMaterial(_overridedMaterialId).InActiveMaterial);
                return;
            }

            if (_hookController.IsGhost)
            {
                SetMaterial(_colorHolder.TransparentMaterial);
                return;
            }

            var pinMaterial = _colorHolder.GetMaterial(_hookController.OverridedMaterial);

            var color = _hookController.IsColorOverrided
                ? _colorHolder.GetColor(pinMaterial, _overridedColorId).PrimaryColor
                : _colorHolder.GetColor(pinMaterial, _colorHolder.GetColor(_colorIndex).ColorId).PrimaryColor;

            _colorIndex++;

            var material = new Material(_hookController.IsMaterialOverrided
                ? _colorHolder.GetMaterial(_hookController.OverridedMaterial).Material
                : _colorHolder.GetMaterial(PinMaterialId.defaultMat).Material);

            material.color = color;

            SetMaterial(material);
        }

        private void SetMaterial(Material material)
        {
            for (var i = 0; i < _hookController.MeshRenderers.Count; i++)
            {
                _hookController.MeshRenderers[i].material = material;
            }
        }

        void Update()
        {
            if (_hookController == null)
            {
                return;
            }

            EditorCheckActivator();

            if (_isFire != _hookController.IsFire)
            {
                UpdateColors();
                return;
            }

            if (_isGhost != _hookController.IsGhost)
            {
                UpdateColors();
                return;
            }

            if ((!_isBlocked && _lastBlockers.Count > 0) || (_isBlocked && _lastBlockers.Count == 0))
            {
                UpdateColors();
                return;
            }

            if (_hookController.IsColorOverrided && _hookController.OverrideColor != _overridedColorId)
            {
                UpdateColors();
            }

            if (_hookController.IsMaterialOverrided && _hookController.OverridedMaterial != _overridedMaterialId)
            {
                UpdateColors();
            }
        }

        private void EditorCheckActivator()
        {
            var blockedPins = _hookController.BlockedPins;

            var isChanged = false;

            if (_hookController.BlockedPins.Count == _lastBlockedPins.Count)
            {
                for (var i = 0; i < blockedPins.Count; i++)
                {
                    if (_hookController.BlockedPins[i] != _lastBlockedPins[i])
                    {
                        isChanged = true;
                        break;
                    }
                }
            }
            else
            {
                isChanged = true;
            }

            if (!isChanged)
            {
                return;
            }

            foreach (var item in _lastBlockedPins)
            {
                var colorEditor = item.GetComponent<HookColoringEdtior>();

                if (colorEditor == null)
                {
                    continue;
                }

                colorEditor._lastBlockers.Remove(_hookController);
            }

            _lastBlockedPins.Clear();

            foreach (var item in _hookController.BlockedPins)
            {
                var colorEditor = item.GetComponent<HookColoringEdtior>();
                _lastBlockedPins.Add(item);

                if (colorEditor == null)
                {
                    continue;
                }

                colorEditor._lastBlockers.Add(_hookController);
            }
        }
#endif
    }
}
