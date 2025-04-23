using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    [ExecuteInEditMode]
    public class HookPinTargetSwitcher : MonoBehaviour
    {
        [SerializeField] private HookController _hookController;
        [SerializeField] private GameObject _targetPin;
        [SerializeField] private GameObject _default;
        [SerializeField] private GameObject _key;
        [SerializeField] private GameObject _emoji;
        [SerializeField] private GameObject _activatorHandles;
        [SerializeField] private List<EyeRotator> _emojisEyes;

        [SerializeField] private MeshRenderer _defaultRenderer;
        [SerializeField] private MeshRenderer _starRenderer;
        [SerializeField] private MeshRenderer _activatorRenderer;
        [SerializeField] private ParticleSystem _activatorParticles;
        [SerializeField] private ParticleSystem _activationEffect;
        [SerializeField] private ParticleSystem _pinFireHandle;

        [SerializeField] private GameObject _starParticleSystem;

        private bool _isActivatorInActive;
        private bool _isKey;

        private bool _isTarget;

        public GameObject TargetPin => _targetPin;

        public GameObject Key => _key;

        private void Awake()
        {
            if (_hookController != null)
            {
                _hookController.AddTargetSwitcher(this);
            }


            if (DebugPanel.isEyeHandleEnabled && Application.isPlaying)
            {
                _default.gameObject.SetActive(false);
                _default = _emoji;

                var eyeRayLenght = DebugPanel.isEyeLookAtCursor ? 0.94f : 0f;

                _emojisEyes.ForEach(a => a.RayLenght = eyeRayLenght);
            }
        }

        public void Start()
        {
            if (!Application.isPlaying)
            {
                UpdatePin();
            }

            UpdateMaterial();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (_hookController == null)
                {
                    return;
                }

                if (_isTarget != _hookController.IsTarget || _isKey != _hookController.IsContainKey)
                {
                    UpdatePin();
                }
            }
        }

        public void SetTutorialLayer()
        {
            var layer = LayerMask.NameToLayer("TutorialPin");
            if (_defaultRenderer != null)
            {
                _defaultRenderer.gameObject.layer = layer;
            }

            if (_starRenderer != null)
            {
                _starRenderer.gameObject.layer = layer;
            }
        }

        public void UpdateMaterial()
        {
            if (ColorHolder.Instance == null)
            {
                return;
            }

            if (_hookController.IsGhost)
            {
                SetInActiveMaterial(ColorHolder.Instance.TransparentMaterial);
                return;
            }

            if (_hookController.IsFire)
            {
                SetInActiveMaterial(ColorHolder.Instance.FireMaterial);
                return;
            }


            if (_hookController.IsInActive)
            {
                if (_hookController.BlockedPins.Count > 0)
                {
                    _isActivatorInActive = true;
                }

                SetInActiveMaterial(ColorHolder.Instance.GetMaterial(PinMaterialId.defaultMat).InActiveMaterial);
                return;
            }

            if (_defaultRenderer)
            {
                _defaultRenderer.material = ColorHolder.Instance.GetDefaultHandlePinMaterial();
            }

            if (_starRenderer)
            {
                _starRenderer.material = ColorHolder.Instance.DefaultStarMaterial;
            }

            if (_isActivatorInActive)
            {
                _activationEffect.Play();
                _isActivatorInActive = false;
            }

            _activatorRenderer.materials = new[]
            {
                ColorHolder.Instance.ActivatorHandleMain, ColorHolder.Instance.ActivatorHandleSecond
            };

            _activatorParticles.gameObject.SetActive(true);

            _starParticleSystem.SetActive(true);
        }

        public void SetInActiveMaterial(Material inActive)
        {
            if (_defaultRenderer)
            {
                _defaultRenderer.material = inActive;
            }

            if (_starRenderer)
            {
                _starRenderer.material = inActive;
            }

            _activatorRenderer.materials = new[] { inActive, inActive };

            _activatorParticles.gameObject.SetActive(false);

            _starParticleSystem.SetActive(false);
        }

        public List<Material> SetBurnMaterial(Vector3 position)
        {
            var materialList = new List<Material>();
            if (_defaultRenderer)
            {
                var material = _defaultRenderer.material;
                material.shader = ColorHolder.Instance.FireShader;
                materialList.Add(material);
            }

            if (_starRenderer)
            {
                var material = _starRenderer.material;
                material.shader = ColorHolder.Instance.FireShader;
                materialList.Add(material);
            }

            return materialList;
        }

        public void DefaultAppear()
        {
            _default.SetActive(true);
            _default.transform.localScale = Vector3.zero;
            _default.transform.DOScale(1f, 0.5f);
        }

        public void UpdatePin()
        {
            if (_hookController == null)
            {
                return;
            }

            _isTarget = _hookController.IsTarget;
            _isKey = _hookController.IsContainKey;

            _pinFireHandle.gameObject.SetActive(_hookController.IsFire);

            if (_hookController.BlockedPins.Count > 0)
            {
                _activatorHandles.SetActive(true);
                _targetPin.SetActive(false);
                _default.SetActive(false);
                _key.SetActive(false);
                return;
            }

            _activatorHandles.SetActive(false);
            _targetPin.SetActive(_hookController.IsTarget);
            _default.SetActive(!_hookController.IsTarget);
            _key.SetActive(_hookController.IsContainKey);
        }
    }
}
