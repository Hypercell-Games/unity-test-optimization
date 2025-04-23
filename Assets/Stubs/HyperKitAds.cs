using System;

// ReSharper disable CheckNamespace

namespace Services
{
    namespace Ads
    {
        public class AdsService
        {
            public void ShowBanner()
            {
            }

            public void HideBanner()
            {
            }

            public void ShowInterstitial(string interstitialId, Action<bool> action = null)
            {
                action?.Invoke(true);
            }

            public void ShowRewardedAd(string rewardedAdId, Action<bool> action)
            {
                action?.Invoke(true);
            }

            public bool IsRewardedAdReady()
            {
                return true;
            }

            public bool IsInterstitialReady()
            {
                return true;
            }
        }
    }
}
