namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.RewardedAds
{
    #if Admob
    using GoogleMobileAds.Api;
    using MessagePipe;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Blueprints;
    using UnityEngine.Events;

    public class AdmobRewardedAds : IRewardedAdsService
    {
        private readonly AdmobSettingBlueprintService admobSettingBlueprintService;

        public AdmobRewardedAds(AdmobSettingBlueprintService admobSettingBlueprintService)
        {
            this.admobSettingBlueprintService = admobSettingBlueprintService;
        }

        private          RewardedAd rewardedAd;
        private readonly string     AD_FLATFORM = "Admob";

        public int GetPriority() => this.admobSettingBlueprintService.GetBlueprint().priorityRewarded;
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
            RewardedAd.Load(this.admobSettingBlueprintService.GetBlueprint().rewardedAdUnitId, adRequest, (ad, error) =>
            {
                if (error != null)
                {
                    // The ad failed to load.
                    GlobalMessagePipe.GetPublisher<OnRewardedAdLoadFailedEventSignal>().Publish(new OnRewardedAdLoadFailedEventSignal(this.AD_FLATFORM, error.GetMessage()));
                    return;
                }
                // The ad loaded successfully.
                this.rewardedAd = ad;
                GlobalMessagePipe.GetPublisher<OnRewardedAdLoadedEventSignal>().Publish(new OnRewardedAdLoadedEventSignal(this.AD_FLATFORM, ""));
                this.RegisterEventHandlers(ad);
            });
        }

        public void ShowAd(UnityAction<bool> onAdComplete, string where)
        {
            if (this.rewardedAd != null && this.rewardedAd.CanShowAd())
            {
                this.rewardedAd.Show((reward) =>
                {
                    onAdComplete(true);
                    GlobalMessagePipe.GetPublisher<OnRewardedAdReceivedRewardEventSignal>().Publish(new OnRewardedAdReceivedRewardEventSignal(this.AD_FLATFORM, "", reward.Type, reward.Amount));
                });
                GlobalMessagePipe.GetPublisher<OnRewardedShowSignal>().Publish(new OnRewardedShowSignal(this.AD_FLATFORM, where));
            }
            else onAdComplete?.Invoke(false);
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
                GlobalMessagePipe.GetPublisher<OnRewardedAdRevenuePaidEventSignal>().Publish(new OnRewardedAdRevenuePaidEventSignal(this.AD_FLATFORM, "", adValue.Value, adValue.CurrencyCode));
            };
            this.rewardedAd.OnAdImpressionRecorded += () =>
            {
                // Raised when an impression is recorded for an ad.
            };
            this.rewardedAd.OnAdClicked += () =>
            {
                // Raised when a click is recorded for an ad.
                GlobalMessagePipe.GetPublisher<OnRewardedAdClickedEventSignal>().Publish(new OnRewardedAdClickedEventSignal(this.AD_FLATFORM, ""));
            };
            this.rewardedAd.OnAdFullScreenContentOpened += () =>
            {
                // Raised when the ad opened full screen content.
                GlobalMessagePipe.GetPublisher<OnRewardedAdDisplayedEventSignal>().Publish(new OnRewardedAdDisplayedEventSignal(this.AD_FLATFORM, ""));
            };
            this.rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                GlobalMessagePipe.GetPublisher<OnRewardedAdHiddenEventSignal>().Publish(new OnRewardedAdHiddenEventSignal(this.AD_FLATFORM, ""));
                this.Initialize();
            };
            this.rewardedAd.OnAdFullScreenContentFailed += error =>
            {
                // Raised when the ad failed to open full screen content.
                GlobalMessagePipe.GetPublisher<OnRewardedAdDisplayFailedEventSignal>().Publish(new OnRewardedAdDisplayFailedEventSignal(this.AD_FLATFORM, "", error.GetMessage()));
            };
        }
    }
    #endif
}
