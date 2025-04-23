using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    public class IceBlockController : MonoBehaviour, ICollisionElement, IIceBlock
    {
        [SerializeField] private IceBlockType _type;
        [SerializeField] private ParticleSystem _brakeEffect;
        [SerializeField] private GameObject _iceBlockVisual;
        [SerializeField] private List<GameObject> _stages;
        [SerializeField] private AbstrackIceBlockScale _iceBlockScale;
        [SerializeField] private BoxCollider _collider;

        [Range(1, 3)] [SerializeField] private int _breakCount = 1;

        [NonSerialized] public IceBlockElementLevelData SaveData;

        public BoxCollider FrozenCollider => _collider;

        private void OnDrawGizmos()
        {
            var overlapTransfrom = _collider.transform;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(overlapTransfrom.TransformPoint(_collider.center),
                overlapTransfrom.TransformVector(_collider.size));

            var style = new GUIStyle();
            style.fontSize = 32;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.blue;
        }

        public void OnTriggerEnter(Collider other)
        {
            var hookPart = other.GetComponent<HookPart>();

            if (hookPart == null || hookPart.Controller == null)
            {
                return;
            }

            if (hookPart.Controller.IsReturning || hookPart.Controller.Removed)
            {
                return;
            }

            hookPart.Controller.OnTouchStaticObject(this);
        }

        public void TouchFeedack()
        {
            _breakCount--;

            VibrationController.Instance.Play(EVibrationType.LightImpact);

            if (_breakCount > 0)
            {
                Punch();
                return;
            }

            Breake();
        }

        public void StartFire()
        {
            Breake();
        }

        public IceBlockType IceBlockType => _type;

        public Transform Transform => transform;

        public bool IsActive => _breakCount > 0;

        public int IceBreakeCount
        {
            get => _breakCount;
            set => _breakCount = Mathf.Max(value, 1);
        }

        public void ForceDestroy()
        {
            Breake();
        }

        public void SetSize(Vector3 size)
        {
            _iceBlockScale.SetSize(size);
        }

        public void Initialize()
        {
            var result = new List<HookController>();

            var overlapTransfrom = _collider.transform;

            var size = _collider.size / 2f;
            size.x *= overlapTransfrom.localScale.x;
            size.y *= overlapTransfrom.localScale.y;
            size.z *= overlapTransfrom.localScale.z;

            var colliders = Physics.OverlapBox(overlapTransfrom.TransformPoint(_collider.center), size,
                overlapTransfrom.rotation);

            foreach (var collider in colliders)
            {
                var part = collider.GetComponent<HookPart>();

                if (part == null || part.IsIce || part.Controller == null)
                {
                    continue;
                }


                if (part.Controller == this || result.Contains(part.Controller))
                {
                    continue;
                }

                result.Add(part.Controller);
                part.Controller.AddFrozenBlock(this);
            }
        }

        public Vector3 GetSize()
        {
            return _iceBlockScale.GetSize();
        }

        public void SetType(IceBlockType type)
        {
            _type = type;
        }

        private void Punch()
        {
            _iceBlockVisual.transform.DOShakePosition(0.3f, 0.3f);
            for (var i = 0; i < _stages.Count; i++)
            {
                _stages[i].SetActive(i == 3 - _breakCount);
            }

            _brakeEffect.Play();
        }

        public void Breake()
        {
            SaveData.sIsDestroyed = 1;
            _breakCount = 0;
            _brakeEffect.Play();
            _iceBlockVisual.gameObject.SetActive(false);
            _collider.enabled = false;
        }
    }
}

public interface IIceBlock
{
    public int IceBreakeCount { get; set; }

    public IceBlockType IceBlockType { get; }

    public bool IsActive { get; }

    public Transform Transform { get; }

    public void SetSize(Vector3 size);

    public Vector3 GetSize();

    public void Initialize();

    public void ForceDestroy();
}

public enum IceBlockType
{
    none = 0,
    loop = 1,
    loopSliced = 2
}
