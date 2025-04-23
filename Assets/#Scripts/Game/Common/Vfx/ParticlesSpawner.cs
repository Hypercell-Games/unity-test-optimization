using System.Collections.Generic;
using UnityEngine;

namespace Unpuzzle
{
    public class ParticlesSpawner : MonoBehaviour
    {
        [SerializeField] private List<DefaultParticleEntity> _particles;
        [SerializeField] private FireFlyPinActivator _fireFlyPinActivator;
        [SerializeField] private ParticleSystem _fireParticle;
        [SerializeField] private FireTrigger _fireTrigger;
        [SerializeField] private BurnEffect _burnEffect;
        public static ParticlesSpawner Instance { private set; get; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public DefaultParticleEntity SpawnParticle(ParticleId particleId, Vector3 position, Quaternion rotation,
            bool autoPlay = false)
        {
            var particle = _particles.Find(a => a.particleID == particleId);

            if (particle == null)
            {
                return null;
            }

            particle = Instantiate(particle, position, rotation);

            if (autoPlay)
            {
                particle.StartPlaying();
            }

            return particle;
        }

        public FireFlyPinActivator GetActivatorFireFly()
        {
            var fireFly = Instantiate(_fireFlyPinActivator);
            return fireFly;
        }

        public ParticleSystem GetFireParticle(Transform parent, Vector3 position)
        {
            var particle = Instantiate(_fireParticle, position, Quaternion.identity, parent);
            return particle;
        }

        public FireTrigger GetFireTrigger(Transform parent, Vector3 position, Quaternion rotation)
        {
            var fireTrigger = Instantiate(_fireTrigger, position, rotation, parent);
            return fireTrigger;
        }

        public BurnEffect GetBurnEffect()
        {
            var burnEffect = Instantiate(_burnEffect);
            return burnEffect;
        }
    }
}
