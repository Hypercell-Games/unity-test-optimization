using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    public abstract class AbstractHookChainElement : MonoBehaviour
    {
        [SerializeField] private float _scale;
        [SerializeField] private List<HookPart> _hookParts;
        [SerializeField] private List<MeshRenderer> _meshRenderers;
        [SerializeField] private List<HookMaterialApplier> _materialAppliers;
        [SerializeField] private BoxCollider _hiddenCheckCollider;
        [SerializeField] private PinChainElementType _type;
        [SerializeField] private Collider[] _colliders = new Collider[0];

        public BoxCollider HiddenCheckCollider => _hiddenCheckCollider;
        public List<HookMaterialApplier> MaterialAppliers => _materialAppliers;
        public PinChainElementType Type => _type;
        public Collider[] Colliders => _colliders;

        public float Scale
        {
            get => _scale;
            set => _scale = value;
        }

        public abstract float GetSerializeScale();

        public abstract float Lenght();

        public abstract Vector3 NextChainLocalPosition();
        public abstract Vector3 NextChainGlobalPosition();

        public abstract void SetCapActive(bool active);

        public abstract void SetScale(float scale);

        public void SetHookController(HookController hookController)
        {
            _hookParts.ForEach(a => a.Controller = hookController);
        }

        public List<MeshRenderer> GetMeshes()
        {
            return _meshRenderers;
        }
    }

    public enum PinChainElementType
    {
        block = 0,
        line = 1,
        wideBlock = 2
    }
}
