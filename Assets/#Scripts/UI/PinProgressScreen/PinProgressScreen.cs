using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Unpuzzle
{
    public class PinProgressScreen : MonoBehaviour
    {
        private const int levelSoftReward = 10;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Animator _prize;
        [SerializeField] private Transform _coins;
        [SerializeField] private TMP_Text _coinsValue;
        [SerializeField] private Transform _tittle;

        [SerializeField] private TextButtonController _claimButton;
        [SerializeField] private RectTransform _flyingPrize;
        [SerializeField] private RectTransform _flyingPrizePos0;
        [SerializeField] private RectTransform _flyingPrizePos1;

        private Sequence _showCollectSeq;

        public bool Showed { get; private set; }

        private void Awake()
        {
            _claimButton.onButtonClicked += Claim;
        }

        public void Show(RectTransform prizeRectTransform)
        {
            Showed = true;

            _tittle.localScale = Vector3.zero;
            _claimButton.transform.localScale = Vector3.zero;

            _prize.Rebind();
            _prize.Update(0f);

            var softCount = levelSoftReward;

            _coinsValue.text = softCount.ToString();

            GlobalData.Instance.GetGameData().AddSoftCcy(softCount);

            _canvasGroup.alpha = 0f;

            gameObject.SetActive(true);
            StartCoroutine(ShowingAnimation(prizeRectTransform));
        }

        private IEnumerator ShowingAnimation(RectTransform prizeRectTransform)
        {
            _flyingPrize.gameObject.SetActive(false);

            yield return new WaitForSeconds(0.5f);

            prizeRectTransform.gameObject.SetActive(false);
            _flyingPrize.gameObject.SetActive(true);

            var pos0 = _flyingPrizePos0.localPosition;
            var pos1 = _flyingPrizePos1.localPosition;
            var scale0 = _flyingPrizePos0.localScale;
            var scale1 = new Vector3(1f, 0.81238f, 1f);
            _flyingPrize.localPosition = pos0;
            _flyingPrize.localScale = scale0;

            var flew = false;

            _prize.gameObject.SetActive(false);
            DOTween.Sequence().SetLink(gameObject)
                .Append(DOTween.To(() => 0f, t =>
                {
                    var x = Mathf.LerpUnclamped(pos0.x, pos1.x, DOVirtual.EasedValue(0f, 1f, t, Ease.InSine));
                    var y = Mathf.LerpUnclamped(pos0.y, pos1.y, DOVirtual.EasedValue(0f, 1f, t, Ease.OutSine));
                    var pos = new Vector3(x, y, pos1.z);
                    _flyingPrize.localPosition = pos;
                    _flyingPrize.localScale =
                        Vector3.Lerp(scale0, scale1, DOVirtual.EasedValue(0f, 1f, t, Ease.Linear));
                }, 1f, 0.5f).SetEase(Ease.InSine))
                .Append(_flyingPrize.DOPunchScale(Vector3.one * 0.05f, 0.2f).SetEase(Ease.Linear))
                .OnComplete(() =>
                {
                    _flyingPrize.gameObject.SetActive(false);
                    _prize.gameObject.SetActive(true);
                    flew = true;
                    prizeRectTransform.gameObject.SetActive(true);
                    _prize.SetBool("Open", true);
                });

            _canvasGroup.DOFade(1f, 0.5f);

            yield return new WaitWhile(() => !flew);


            _tittle.transform.DOScale(1f, 0.5f);
            _claimButton.transform.DOScale(1f, 0.5f).SetDelay(0.2f);
        }

        private void Claim()
        {
            HyperKit.Ads.ShowInterstitial("ad_inter_in_gameplay",
                _ =>
                {
                    var softCount = levelSoftReward;

                    var fromSoftCcy = GlobalData.Instance.GetGameData().SoftCcyAmount;
                    GlobalData.Instance.GetGameData().AddSoftCcy(softCount);


                    _claimButton.transform.localScale = Vector3.zero;

                    var showCollectSeq = DOTween.Sequence()
                        .SetLink(gameObject);

                    showCollectSeq.Append(_canvasGroup.DOFade(0f, 0.5f));
                    showCollectSeq.OnComplete(() =>
                    {
                        Showed = false;
                        gameObject.SetActive(false);
                    });

                    LevelsController.Instance.CurrentGameController.PinProgressNextStage();
                });
        }
    }
}
