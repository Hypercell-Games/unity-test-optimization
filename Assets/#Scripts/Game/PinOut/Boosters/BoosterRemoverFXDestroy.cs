using UnityEngine;

namespace Unpuzzle
{
    public class BoosterRemoverFXDestroy : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _effect;

        public void SetAndPlay(Vector3 bounds)
        {
            var main = _effect.shape;
            main.scale = bounds * 2f;
            _effect.Play();
        }
    }
}
