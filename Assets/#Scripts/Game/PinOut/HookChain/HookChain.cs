using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unpuzzle
{
    [ExecuteInEditMode]
    public class HookChain : MonoBehaviour, IHookScaler
    {
        [SerializeField] private HookChainBlock _block;
        [SerializeField] private HookChainBlock _wideBlock;
        [SerializeField] private HookChainLine _line;
        [SerializeField] private List<AbstractHookChainElement> _hookChainElement;
        [SerializeField] private Transform _parent;
        [SerializeField] private Transform _elementsStart;
        [SerializeField] private MeshRenderer _startMesh;
        [SerializeField] private TrailRenderer _trail;
        [SerializeField] private HookMaterialApplier _startMaterialApplier;
        [SerializeField] private Transform _downBoundPivot;

        private List<float> _scaling;

        public List<AbstractHookChainElement> Elements => _hookChainElement;

        public HookChainBlock Block => _block;
        public HookChainBlock WideBlock => _wideBlock;
        public HookChainLine Line => _line;

        private void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (_hookChainElement.Count == 0)
            {
                return;
            }

            var hookController = GetComponent<HookController>();
            if (hookController != null)
            {
                var listMaterialApplier = new List<HookMaterialApplier> { _startMaterialApplier };
                _hookChainElement.ForEach(a => listMaterialApplier.AddRange(a.MaterialAppliers));
                hookController.SetupMaterialApplier(listMaterialApplier);
                var colliders = _hookChainElement.SelectMany(e => e.Colliders).ToArray();
                hookController.SetupColliders(colliders);
            }

            var lastPos = _hookChainElement[_hookChainElement.Count - 1].NextChainLocalPosition();
            _downBoundPivot.localPosition = lastPos;
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                return;
            }

            CheckElementsChanging();
        }

        private void OnEnable()
        {
            _scaling = new List<float>();
            _hookChainElement ??= new List<AbstractHookChainElement>();
            _hookChainElement.ForEach(a => _scaling.Add(a.Scale));
        }

        public float GetPinLenght()
        {
            return (_elementsStart.position - _hookChainElement[_hookChainElement.Count - 1].NextChainGlobalPosition())
                .magnitude;
        }

        public void UpdateValue(string chains, string scales, string rotations)
        {
            var chainsType = chains.Split(';').Select(int.Parse).Cast<PinChainElementType>().ToList();
            var chainsScales = scales.Split(';').Select(float.Parse).ToList();
            var chainsRotation = rotations.Split(';').Select(float.Parse).ToList();

            _hookChainElement.ForEach(a => Destroy(a.gameObject));
            _scaling.Clear();
            _hookChainElement.Clear();

            for (var i = 0; i < chainsType.Count(); i++)
            {
                AbstractHookChainElement element = null;
                switch (chainsType[i])
                {
                    case PinChainElementType.block:
                        element = InstantiateAndAddElement(_block);
                        break;
                    case PinChainElementType.line:
                        element = InstantiateAndAddElement(_line);
                        break;
                    case PinChainElementType.wideBlock:
                        element = InstantiateAndAddElement(_wideBlock);
                        break;
                }

                element.Scale = chainsScales[i];
                _scaling.Add(element.Scale);
            }

            Reconstruct();

            for (var i = 0; i < _hookChainElement.Count; i++)
            {
                _hookChainElement[i].transform.localRotation = Quaternion.Euler(Vector3.up * chainsRotation[i]);
            }

            var hookController = GetComponent<HookController>();

            if (hookController != null)
            {
                var listMaterialApplier = new List<HookMaterialApplier> { _startMaterialApplier };
                _hookChainElement.ForEach(a => listMaterialApplier.AddRange(a.MaterialAppliers));
                hookController.SetupMaterialApplier(listMaterialApplier);
                var colliders = _hookChainElement.SelectMany(e => e.Colliders).ToArray();
                hookController.SetupColliders(colliders);
            }
        }

        public void AddElement(AbstractHookChainElement element)
        {
            element.transform.parent = _parent;
            _hookChainElement.Add(element);
            _scaling.Add(element.Scale);
            Reconstruct();
        }

        public AbstractHookChainElement InstantiateAndAddElement(AbstractHookChainElement element)
        {
            element = Instantiate(element);
            element.transform.parent = _parent;
            _hookChainElement.Add(element);

            return element;
        }

        public void RemoveElement()
        {
            var element = _hookChainElement[_hookChainElement.Count - 1];
            _scaling.RemoveAt(_hookChainElement.Count - 1);
            _hookChainElement.Remove(element);
            DestroyImmediate(element.gameObject);
            Reconstruct();
        }

        public void Reconstruct()
        {
            for (var i = 0; i < _hookChainElement.Count; i++)
            {
                _hookChainElement[i].SetScale(_hookChainElement[i].Scale);

                if (i == 0)
                {
                    _hookChainElement[i].transform.localPosition = _elementsStart.transform.localPosition;
                    _hookChainElement[i].transform.rotation = _elementsStart.transform.rotation;
                }
                else
                {
                    _hookChainElement[i].transform.localPosition = _hookChainElement[i - 1].NextChainLocalPosition();
                    _hookChainElement[i].transform.rotation = _elementsStart.transform.rotation;
                }

                _hookChainElement[i].SetCapActive(i == _hookChainElement.Count - 1);
                _scaling[i] = _hookChainElement[i].Scale;
            }

            if (_hookChainElement.Count > 0)
            {
                var length = (_elementsStart.position -
                              _hookChainElement[_hookChainElement.Count - 1].NextChainGlobalPosition()).magnitude;
                _trail.transform.localPosition =
                    _hookChainElement[_hookChainElement.Count - 1].NextChainLocalPosition();
                _parent.localPosition = Vector3.up * length / 2f;
            }

            var hook = GetComponent<HookController>();

            if (hook == null)
            {
                return;
            }

            var newListMeshes = new List<MeshRenderer>();
            newListMeshes.Add(_startMesh);

            _hookChainElement.ForEach(a => newListMeshes.AddRange(a.GetMeshes()));
            _hookChainElement.ForEach(a =>
            {
                if (a.HiddenCheckCollider)
                {
                    hook.AddCheckRevealPinColldier(a.HiddenCheckCollider);
                }
            });

            hook.SetupMeshes(newListMeshes);

            _hookChainElement.ForEach(a => a.SetHookController(hook));
        }

        private void CheckElementsChanging()
        {
            for (var i = 0; i < _hookChainElement.Count; i++)
            {
                if (_hookChainElement[i].Scale != _scaling[i])
                {
                    Reconstruct();
                    break;
                }
            }
        }
    }
}
