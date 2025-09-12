namespace ThirdParty.ServiceImplementation.AdsService.Admob.RewardedAds
{
    using GoogleMobileAds.Api;
    using ThirdPartyService.ServiceImplementation.DI.RewardedAds;
    using UnityEngine.Events;

    public class AdmobRewardedAds : IRewardedAdsService
    {
        private RewardedAd rewardedAd;

        public void Initialize()
        {
            if (this.rewardedAd != null)
            {
                this.rewardedAd.Destroy();
                this.rewardedAd = null;
            }
            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // Send the request to load the ad.
            RewardedAd.Load("AD_UNIT_ID", adRequest, (ad, error) =>
            {
                if (error != null)
                    // The ad failed to load.
                    return;
                // The ad loaded successfully.
                this.rewardedAd = ad;
                this.RegisterEventHandlers(ad);
            });
        }

        public void ShowAd(UnityAction<bool> onAdComplete, string where)
        {
            if (this.rewardedAd != null && this.rewardedAd.CanShowAd())
                this.rewardedAd.Show((_) => onAdComplete(true));
            else
                onAdComplete?.Invoke(false);
        }

        public bool IsAdReady()
        {
            return this.rewardedAd != null && this.rewardedAd.CanShowAd();
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            this.rewardedAd.OnAdPaid += adValue =>
            {
                // Raised when the ad is estimated to have earned money.
            };
            this.rewardedAd.OnAdImpressionRecorded += () =>
            {
                // Raised when an impression is recorded for an ad.
            };
            this.rewardedAd.OnAdClicked += () =>
            {
                // Raised when a click is recorded for an ad.
            };
            this.rewardedAd.OnAdFullScreenContentOpened += () =>
            {
                // Raised when the ad opened full screen content.
            };
            this.rewardedAd.OnAdFullScreenContentClosed += this.Initialize;
            this.rewardedAd.OnAdFullScreenContentFailed += error =>
            {
                // Raised when the ad failed to open full screen content.
            };
        }
    }
}