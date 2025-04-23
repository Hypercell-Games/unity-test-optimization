using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Unpuzzle
{
    public class BoosterRemoverFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private BoosterRemoverFXDestroy _destroyEffect;
        [SerializeField] private SpriteRenderer _rocketSprite;
        [SerializeField] private ParticleSystem _explosionEffect;
        private Transform _flyParent;

        private HookController _hookController;
        private IIceBlock _iceBLock;
        private Camera _mainCamera;
        private float _rayLenght;
        private Camera _uiCamera;

        public void RemovePin(HookController hookController, Transform flyParent, Camera mainCamera, Camera uiCamera,
            float delay)
        {
            _mainCamera = mainCamera;
            _uiCamera = uiCamera;
            _flyParent = flyParent;

            _rayLenght = _flyParent.localPosition.z;

            _hookController = hookController;

            _rocketSprite.gameObject.SetActive(false);

            StartCoroutine(FlyAnimation(transform.localPosition, hookController.transform, delay, false));
        }

        public void RemoveIceBlock(IIceBlock iceBlock, Transform flyParent, Camera mainCamera, Camera uiCamera,
            float delay)
        {
            _mainCamera = mainCamera;
            _uiCamera = uiCamera;
            _flyParent = flyParent;

            _rayLenght = _flyParent.localPosition.z;

            _iceBLock = iceBlock;

            _rocketSprite.gameObject.SetActive(false);

            StartCoroutine(FlyAnimation(transform.localPosition, _iceBLock.Transform, delay, true));
        }

        private Vector3 TransformFromMainToUI(Vector3 position)
        {
            position = _mainCamera.WorldToScreenPoint(position);
            position = _uiCamera.ScreenPointToRay(position).GetPoint(_rayLenght);

            return position;
        }

        private IEnumerator FlyAnimation(Vector3 startPos, Transform target, float delay, bool isIceBlock)
        {
            yield return new WaitForSeconds(delay);


            _rocketSprite.gameObject.SetActive(true);

            var endPosition = _flyParent.InverseTransformPoint(TransformFromMainToUI(target.position));
            var middlePos = Vector3.Lerp(startPos, endPosition, 0.5f);

            var direction = (endPosition - startPos).normalized;
            var controlLenght = (endPosition - startPos).magnitude;

            var perpendiculdar = Vector3.Cross(direction, Vector3.back);

            middlePos -= perpendiculdar * 3f * Random.Range(-1f, 1f);

            var controlOffset = direction * controlLenght * 0.25f;

            _particleSystem.Play();

            _rocketSprite.transform.localScale = Vector3.one * 0.5f;


            _rocketSprite.transform.DOScale(1f, 0.25f)
                .OnComplete(() => { _rocketSprite.transform.DOScale(0.5f, 0.25f); });

            DOTween.To(x =>
                {
                    var lastPositon = transform.localPosition;

                    var newEndPosition = _flyParent.InverseTransformPoint(TransformFromMainToUI(target.position));
                    middlePos += newEndPosition - endPosition;

                    endPosition = newEndPosition;

                    var position = DOCurve.CubicBezier.GetPointOnSegment(startPos, middlePos - controlOffset,
                        endPosition, middlePos + controlOffset, x);

                    transform.localPosition = position;

                    var directionUp = transform.parent.TransformDirection(position - lastPositon);

                    transform.LookAt(Vector3.forward, directionUp);
                }, 0f, 1f, 0.5f)
                .SetEase(Ease.Linear);
            yield return new WaitForSeconds(0.5f);
            _explosionEffect.Play();
            _rocketSprite.gameObject.SetActive(false);
            BoosterRemoverFXDestroy destroyEffect = null;
            if (!isIceBlock)
            {
                destroyEffect = Instantiate(_destroyEffect, _hookController.transform.position,
                    _hookController.transform.rotation);
                destroyEffect.SetAndPlay(_hookController.GetBounds());
            }

            _particleSystem.Stop();
            yield return new WaitForSeconds(0.2f);
            if (!isIceBlock)
            {
                _hookController.BoosterRemovePin();
            }

            if (isIceBlock)
            {
                _iceBLock.ForceDestroy();
            }

            yield return new WaitForSeconds(3f);
            if (destroyEffect != null)
            {
                Destroy(destroyEffect.gameObject);
            }

            Destroy(gameObject);
        }
    }
}
