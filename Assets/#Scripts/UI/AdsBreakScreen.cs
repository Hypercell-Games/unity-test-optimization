using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Unpuzzle
{
    public class AdsBreakScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _coinsParent;
        [SerializeField] private TextMeshProUGUI _coins;

        public void ShowAd()
        {
            if (!GameConfig.RemoteConfig.adsBreakeEnabled)
            {
                return;
            }

            if (!HyperKit.Ads.IsInterstitialReady())
            {
                return;
            }


            Debug.Log("Can show interstitial");

            gameObject.SetActive(true);

            StartCoroutine(AdShowing());
        }

        private IEnumerator AdShowing()
        {
            _canvasGroup.alpha = 1;

            var isCointEnabled = GameConfig.RemoteConfig.adsBreakeCoins > 0;

            _coinsParent.gameObject.SetActive(isCointEnabled);

            _coins.text = "+" + GameConfig.RemoteConfig.adsBreakeCoins;

            if (isCointEnabled)
            {
                GlobalData.Instance.GetGameData().AddSoftCcy(GameConfig.RemoteConfig.adsBreakeCoins, "ad_break");
            }

            yield return new WaitForSeconds(GameConfig.RemoteConfig.adsBreakDelayShowing);

            HyperKit.Ads.ShowInterstitial("ad_inter_between_stages");

            yield return new WaitForSeconds(GameConfig.RemoteConfig.adsBreakDelayHide);

            _canvasGroup.DOFade(0f, 0.5f)
                .OnComplete(() => { gameObject.SetActive(false); })
                .SetLink(_canvasGroup.gameObject);
        }
    }
}
