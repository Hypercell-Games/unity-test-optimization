using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Unpuzzle
{
    public class LobbyButtonBoosterAnimation : MonoBehaviour
    {
        [SerializeField] private Transform iconTarget;
        [SerializeField] private Transform scalePivot;
        private Sequence _scaleSeq;

        public Sequence SequenceCollectFrom(Transform sourceTransform, Image iconPrefab)
        {
            var target = iconTarget;

            var seq = DOTween.Sequence().SetLink(gameObject);

            for (var i = 0; i < 3; i++)
            {
                var delay = i * 0.01f + Random.Range(0, 0.015f);

                var img = Instantiate(iconPrefab, sourceTransform);
                img.transform.localPosition = Vector3.zero;
                img.transform.localScale = sourceTransform.localScale;

                img.transform.SetParent(target.transform, true);

                var scale = 0.85f;

                var randomDirection = new Vector2(0.5f, 0);
                randomDirection = randomDirection.SetAngle(Random.Range(0, 360));

                img.gameObject.SetActive(false);
                var seq1 = DOTween.Sequence()
                    .SetLink(gameObject)
                    .AppendInterval(delay)
                    .AppendCallback(() => img.gameObject.SetActive(true))
                    .Append(DOTween.Sequence()
                        .Append(img.transform.DOScale(1.3f, 0.3f).SetEase(Ease.OutSine))
                        .Join(img.transform.DOLocalRotate(new Vector3(0, 0, 20 * Random.Range(-1, 1)), 0.3f)
                            .SetEase(Ease.OutSine))
                        .Append(img.transform.DOScale(scale, 0.8f).SetEase(Ease.InSine))
                        .Join(img.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.7f).SetEase(Ease.InSine))
                    )
                    .Join(img.DOColor(Color.white, 0.2f).SetEase(Ease.InOutSine))
                    .Join(DOTween.Sequence()
                        .Append(img.transform.DOMove(new Vector3(randomDirection.x, randomDirection.y, 0), 0.5f)
                            .SetEase(Ease.OutSine).SetRelative(transform))
                        .Append(img.transform.DOMove(target.position, 0.5f).SetEase(Ease.InSine).OnComplete(() =>
                        {
                            if (scalePivot)
                            {
                                _scaleSeq?.Kill();
                                _scaleSeq = DOTween.Sequence().SetLink(scalePivot.gameObject)
                                    .Append(scalePivot.DOScale(0.9f, 0.07f))
                                    .Append(scalePivot.DOScale(1f, 0.2f));
                            }

                            if (img && img.gameObject)
                            {
                                img.gameObject.SetActive(false);
                            }
                        }))
                        .Join(DOTween.To(x =>
                        {
                        }, 0f, 1f, 0.5f).SetDelay(0.5f))
                    ).OnComplete(() => Destroy(img.gameObject));
                seq.Join(seq1);
            }

            return seq;
        }
    }
}
