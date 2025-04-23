using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    public class IceBlock : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _brakeEffect;
        [SerializeField] private GameObject _iceBlockVisual;
        [SerializeField] private HookPart _hookPart;
        [SerializeField] private List<GameObject> _stages;
        [SerializeField] private BoxCollider _collider;

        public BoxCollider FrozenCollider => _collider;

        public void Punch(int currentStage)
        {
            _iceBlockVisual.transform.DOShakePosition(0.3f, 0.3f);
            for (var i = 0; i < _stages.Count; i++)
            {
                _stages[i].SetActive(i == currentStage);
            }

            _brakeEffect.Play();
        }

        public void Breake()
        {
            _brakeEffect.Play();
            _iceBlockVisual.gameObject.SetActive(false);
            _hookPart.gameObject.SetActive(false);
        }
    }
}
