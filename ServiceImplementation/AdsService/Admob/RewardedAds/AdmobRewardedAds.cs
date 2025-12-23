namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.RewardedAds
{
    #if Admob
    using GameFoundation.Scripts.Patterns.SignalBus;
    using GoogleMobileAds.Api;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Blueprints;
    using UnityEngine.Events;

    public class AdmobRewardedAds : IRewardedAdsService
    {
        private readonly AdmobSettingBlueprintService admobSettingBlueprintService;
        private readonly SignalBus                    signalBus;

        public AdmobRewardedAds(
            AdmobSettingBlueprintService admobSettingBlueprintService,
            SignalBus                    signalBus
        )
        {
            this.admobSettingBlueprintService = admobSettingBlueprintService;
            this.signalBus                    = signalBus;
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
                    this.signalBus.Fire<OnRewardedAdLoadFailedEventSignal>(new(this.AD_FLATFORM, error.GetMessage()));
                    return;
                }
                // The ad loaded successfully.
                this.rewardedAd = ad;
                this.signalBus.Fire<OnRewardedAdLoadedEventSignal>(new(this.AD_FLATFORM, ""));
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
                    this.signalBus.Fire<OnRewardedAdReceivedRewardEventSignal>(new(this.AD_FLATFORM, "", reward.Type, reward.Amount));
                });
                this.signalBus.Fire<OnRewardedShowSignal>(new(this.AD_FLATFORM, where));
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
                this.signalBus.Fire<OnRewardedAdRevenuePaidEventSignal>(new(this.AD_FLATFORM, "", adValue.Value, adValue.CurrencyCode));
            };
            this.rewardedAd.OnAdImpressionRecorded += () =>
            {
                // Raised when an impression is recorded for an ad.
            };
            this.rewardedAd.OnAdClicked += () =>
            {
                // Raised when a click is recorded for an ad.
                this.signalBus.Fire<OnRewardedAdClickedEventSignal>(new(this.AD_FLATFORM, ""));
            };
            this.rewardedAd.OnAdFullScreenContentOpened += () =>
            {
                // Raised when the ad opened full screen content.
                this.signalBus.Fire<OnRewardedAdDisplayedEventSignal>(new(this.AD_FLATFORM, ""));
            };
            this.rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                this.signalBus.Fire<OnRewardedAdHiddenEventSignal>(new(this.AD_FLATFORM, ""));
                this.Initialize();
            };
            this.rewardedAd.OnAdFullScreenContentFailed += error =>
            {
                // Raised when the ad failed to open full screen content.
                this.signalBus.Fire<OnRewardedAdDisplayFailedEventSignal>(new(this.AD_FLATFORM, "", error.GetMessage()));
            };
        }
    }
    #endif
}