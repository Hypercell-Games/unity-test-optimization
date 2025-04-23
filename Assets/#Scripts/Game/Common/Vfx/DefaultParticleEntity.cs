using System.Collections;
using UnityEngine;

namespace Unpuzzle
{
    public class DefaultParticleEntity : MonoBehaviour
    {
        [SerializeField] private ParticleId _particleID;
        [SerializeField] private ParticleSystem _particleSystem;

        public ParticleId particleID => _particleID;

        public ParticleSystem ParticleSystem => _particleSystem;

        public void StartPlaying()
        {
            _particleSystem.Play();

            StartCoroutine(WaitingEnd());
        }

        private IEnumerator WaitingEnd()
        {
            yield return new WaitWhile(() => _particleSystem.isPlaying);
            Destroy(gameObject);
        }

        [ContextMenu("sdsd")]
        private void SetParticle()
        {
            _particleID = ParticleId.ignite;
        }
    }
}
