using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    [ExecuteInEditMode]
    public class PinBoltPlank : MonoBehaviour
    {
        private const float _meshOffset = 0.3f;
        [SerializeField] private BoxCollider _boxCollider;
        [SerializeField] private Transform _visual;
        [SerializeField] private Transform _plankMesh;
        [SerializeField] private Vector3 _scale = new(4, 4, 1);
        [SerializeField] private Transform _hole;
        [SerializeField] private MeshRenderer _mesh;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
        [SerializeField] private Material _material;
        private List<HookController> _bolts;
        private List<Transform> _holes = new();

        private bool _isDestoroyed;
#if UNITY_EDITOR
        private Vector3 _lastScale;
#endif
        private BoxCollider _physicsBoxCollider;
        private Rigidbody _rigidBody;

        [NonSerialized] public PinBoltPlankData SaveData;

        private void Update()
        {
#if UNITY_EDITOR
            if (Application.IsPlaying(gameObject))
            {
                return;
            }

            if (_lastScale != _scale)
            {
                SetScale(_scale);
            }

            if (transform.localScale != Vector3.one)
            {
                var scale = _scale + new Vector3(transform.localScale.x - 1, transform.localScale.y - 1,
                    transform.localScale.z - 1) * 0.3f;
                transform.localScale = Vector3.one;
                SetScale(scale);
            }
#endif
        }

        private void FixedUpdate()
        {
            if (_rigidBody && _rigidBody.isKinematic == false)
            {
                _rigidBody.AddForce(Vector3.down * 200f, ForceMode.Acceleration);
            }
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            SetScale(_scale);
#endif
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
            {
                return;
            }

            var max = 0f;
            for (var i = 0; i < collision.contacts.Length; i++)
            {
                var contact = collision.contacts[i];
                var impulse = contact.impulse;
                max = Mathf.Max(max, impulse.sqrMagnitude);
            }
        }

        public void Initialize()
        {
            _bolts = new List<HookController>();

            var overlapTransfrom = _boxCollider.transform;

            var size = _boxCollider.size / 2f;
            size.x *= overlapTransfrom.localScale.x;
            size.y *= overlapTransfrom.localScale.y;
            size.z *= overlapTransfrom.localScale.z;

            var colliders = Physics.OverlapBox(overlapTransfrom.TransformPoint(_boxCollider.center), size,
                overlapTransfrom.rotation);

            foreach (var collider in colliders)
            {
                var part = collider.GetComponent<HookPart>();

                if (part == null || part.IsIce || part.Controller == null)
                {
                    continue;
                }

                if (part.Controller.pinType != PinType.pinBolt)
                {
                    continue;
                }

                if (_bolts.Contains(part.Controller))
                {
                    continue;
                }

                _bolts.Add(part.Controller);
                var hole = Instantiate(_hole);
                var holeSize = hole.localScale;
                holeSize.y = 0.385f * _scale.z;
                hole.localScale = holeSize;
                hole.rotation = part.Controller.transform.rotation;

                hole.transform.SetParent(_visual);
                var position = _visual.InverseTransformPoint(part.Controller.transform.position);
                position.z = 0f;
                hole.transform.localPosition = position;
                part.Controller.OnPostMoveOut += OnBoltRemoved;
            }

            var scale = Vector3.Scale(_visual.transform.localScale, _meshFilter.transform.localScale);
            _skinnedMeshRenderer.SetBlendShapeWeight(2, scale.x - _meshOffset);
            _skinnedMeshRenderer.SetBlendShapeWeight(0, scale.y - _meshOffset);
            _skinnedMeshRenderer.SetBlendShapeWeight(1, scale.z - _meshOffset);
            CreateCube();

            void CreateCube()
            {
                var mesh = new Mesh { name = "ProceduralCube" };

                var vertices = new Vector3[24]
                {
                    new(-0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, 0.5f), new(0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f),
                    new(0.5f, -0.5f, -0.5f), new(-0.5f, -0.5f, -0.5f), new(-0.5f, 0.5f, -0.5f),
                    new(0.5f, 0.5f, -0.5f), new(-0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, -0.5f),
                    new(-0.5f, 0.5f, -0.5f), new(-0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, -0.5f),
                    new(0.5f, -0.5f, 0.5f), new(-0.5f, -0.5f, 0.5f), new(-0.5f, -0.5f, -0.5f),
                    new(-0.5f, -0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, -0.5f),
                    new(0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, -0.5f), new(0.5f, 0.5f, -0.5f), new(0.5f, 0.5f, 0.5f)
                };

                for (var i = 0; i < vertices.Length; i++)
                {
                    vertices[i].Scale(scale);
                }

                var triangles = new int[36]
                {
                    0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7, 8, 9, 10, 8, 10, 11, 12, 13, 14, 12, 14, 15, 16, 17, 18, 16,
                    18, 19, 20, 21, 22, 20, 22, 23
                };

                var uvs = new Vector2[24]
                {
                    new(0, 0), new(scale.x, 0), new(scale.x, scale.y), new(0, scale.y), new(0, 0), new(scale.x, 0),
                    new(scale.x, scale.y), new(0, scale.y), new(0, 0), new(scale.x, 0), new(scale.x, scale.z),
                    new(0, scale.z), new(0, 0), new(scale.x, 0), new(scale.x, scale.z), new(0, scale.z), new(0, 0),
                    new(scale.z, 0), new(scale.z, scale.y), new(0, scale.y), new(0, 0), new(scale.z, 0),
                    new(scale.z, scale.y), new(0, scale.y)
                };

                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = uvs;
                mesh.RecalculateNormals();

                _meshFilter.mesh = mesh;

                var targetLossyScale = Vector3.one;
                var parentLossyScale = _meshFilter.transform.parent != null
                    ? _meshFilter.transform.parent.lossyScale
                    : Vector3.one;
                var calculatedLocalScale = new Vector3(
                    targetLossyScale.x / parentLossyScale.x,
                    targetLossyScale.y / parentLossyScale.y,
                    targetLossyScale.z / parentLossyScale.z
                );
                _meshFilter.transform.localScale = calculatedLocalScale;
            }

            _mesh.material = _material;
            _skinnedMeshRenderer.material = _material;

            var objectMatrix = _skinnedMeshRenderer.transform.localToWorldMatrix;


            _physicsBoxCollider = Instantiate(_boxCollider, _boxCollider.transform.position,
                _boxCollider.transform.rotation, _boxCollider.transform.parent);
            _physicsBoxCollider.isTrigger = false;
            _physicsBoxCollider.gameObject.layer = LayersUtils.PhysicsLayer;
        }

        private void OnBoltRemoved(HookController hookController)
        {
            _bolts.Remove(hookController);
            hookController.OnPostMoveOut -= OnBoltRemoved;

            if (_bolts.Count == 0)
            {
                DestroyPlank(hookController);
            }
        }

        public void DestroyPlank(HookController hookController)
        {
            if (_isDestoroyed)
            {
                return;
            }

            _isDestoroyed = true;
            SaveData.sIsDestroyed = 1;


            _rigidBody = gameObject.AddComponent<Rigidbody>();
            _rigidBody.isKinematic = false;
            _rigidBody.useGravity = true;
            _rigidBody.mass = 1f;
            _boxCollider.enabled = false;

            if (hookController)
            {
                var handle = hookController.hookHandle.transform;

                _rigidBody.AddForceAtPosition(Vector3.up * 10f, handle.position, ForceMode.VelocityChange);
            }

            StartCoroutine(Co_Wait());

            IEnumerator Co_Wait()
            {
                yield return new WaitForSeconds(0.8f);

                var waiter = new WaitForSeconds(0.2f);
                while (true)
                {
                    var lastPos = _rigidBody.position;
                    yield return waiter;
                    if (Vector3.SqrMagnitude(lastPos - _rigidBody.position) < 0.01f)
                    {
                        gameObject.SetActive(false);
                        var particle = ParticlesSpawner.Instance.SpawnParticle(ParticleId.woodenParts, _visual.position,
                            _visual.rotation);
                        var shape = particle.ParticleSystem.shape;
                        shape.meshRenderer = _mesh;
                        particle.StartPlaying();

                        break;
                    }

                    if (Vector3.SqrMagnitude(_rigidBody.position) > 10000f * 10000f)
                    {
                        gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }

        public void PlankTouched()
        {
            _visual.DOKill(true);
            _visual.DOPunchPosition(Vector3.one * 0.2f, 0.1f);
        }

        public Vector3 GetScale()
        {
            return _scale;
        }

        public void SetScale(Vector3 scale)
        {
            if (scale.z == 0f)
            {
                scale.z = 1f;
            }

            _scale = scale;

#if UNITY_EDITOR

            _lastScale = scale;
#endif

            if (_plankMesh != null)
            {
                _plankMesh.localScale = new Vector3(scale.x, scale.y, scale.z);
            }

            if (_plankMesh != null)
            {
                _boxCollider.size = new Vector3(scale.x, scale.y, 0.38f * scale.z);
            }


            _skinnedMeshRenderer.SetBlendShapeWeight(2, scale.x - _meshOffset);
            _skinnedMeshRenderer.SetBlendShapeWeight(0, scale.y - _meshOffset);
            _skinnedMeshRenderer.SetBlendShapeWeight(1, 0.38f * (scale.z - _meshOffset));
        }
    }
}
