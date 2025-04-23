using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    public class FireTrigger : MonoBehaviour
    {
        [SerializeField] private BoxCollider _boxCollider;
        private readonly List<ICollisionElement> _collisionElement = new();
        private readonly List<ParticleSystem> _firedElement = new();
        private readonly List<HookController> _triggeredParts = new();
        private HookController _parentHookController;

        private void OnTriggerEnter(Collider other)
        {
            var hookPart = other.GetComponent<HookPart>();

            var position = other.ClosestPoint(transform.position);

            if (hookPart != null)
            {
                if (!hookPart.Controller)
                {
                    return;
                }

                if (hookPart.Controller.Removed)
                {
                    return;
                }

                if (hookPart.Controller == _parentHookController || _triggeredParts.Contains(hookPart.Controller))
                {
                    return;
                }

                _triggeredParts.Add(hookPart.Controller);
                var fire = ParticlesSpawner.Instance.GetFireParticle(_parentHookController.transform.parent, position);
                ParticlesSpawner.Instance.SpawnParticle(ParticleId.ignite, position, Quaternion.identity, true);
                _firedElement.Add(fire);
                hookPart.Controller.Burn(position);
            }
            else
            {
                var collisionElement = other.GetComponent<ICollisionElement>();

                if (collisionElement != null && !_collisionElement.Contains(collisionElement))
                {
                    _collisionElement.Add(collisionElement);
                    var fire = ParticlesSpawner.Instance.GetFireParticle(_parentHookController.transform.parent,
                        position);
                    _firedElement.Add(fire);
                    ParticlesSpawner.Instance.SpawnParticle(ParticleId.ignite, position, Quaternion.identity, true);
                    collisionElement.StartFire();
                }
            }
        }

        public void SetScaleAndParent(HookController hookController, List<Transform> borderPosition)
        {
            _parentHookController = hookController;
            var minPosition = new Vector3();
            var maxPosition = new Vector3();
            foreach (var pivot in borderPosition)
            {
                var pivotPos = transform.InverseTransformPoint(pivot.position);
                minPosition.x = Mathf.Min(minPosition.x, pivotPos.x);
                minPosition.y = Mathf.Min(minPosition.y, pivotPos.y);
                minPosition.z = Mathf.Min(minPosition.z, pivotPos.z);

                maxPosition.x = Mathf.Max(maxPosition.x, pivotPos.x);
                maxPosition.y = Mathf.Max(maxPosition.y, pivotPos.y);
                maxPosition.z = Mathf.Max(maxPosition.z, pivotPos.z);
            }

            _boxCollider.center = Vector3.Lerp(minPosition, maxPosition, 0.5f);
            _boxCollider.size = maxPosition - minPosition + Vector3.one * GameConfig.RemoteConfig.fireTriggerSize;
        }

        public void DestroyFiredItems()
        {
            var levelParent = _parentHookController.transform.parent;
            var position = _parentHookController.transform.position;
            var rotation = _parentHookController.transform.rotation;

            var burnEffect = ParticlesSpawner.Instance.GetBurnEffect();
            burnEffect.transform.position = position;
            burnEffect.transform.rotation = rotation;
            burnEffect.transform.SetParent(levelParent);
            burnEffect.SetAndPlay(_parentHookController.GetBounds());
            var delay = 0.1f;

            _firedElement.ForEach(a =>
            {
                a.Stop();
                Destroy(a.gameObject, 2f);
            });

            _collisionElement.Clear();
            _triggeredParts.Clear();
            _boxCollider.enabled = false;
        }
    }
}
