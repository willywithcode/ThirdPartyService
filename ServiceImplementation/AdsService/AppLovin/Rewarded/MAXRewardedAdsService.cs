#if MAX
namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Rewarded
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using UnityEngine.Events;

    public class MAXRewardedAdsService : IRewardedAdsService
    {
        private readonly APPLOVINBlueprintService applovinBlueprintService;
        private readonly SignalBus                signalBus;

        public MAXRewardedAdsService(
            APPLOVINBlueprintService applovinBlueprintService,
            SignalBus                signalBus
        )
        {
            this.applovinBlueprintService = applovinBlueprintService;
            this.signalBus                = signalBus;
        }

        private          int               retryAttempt;
        private          UnityAction<bool> onAdComplete;
        private          int               countReloadVideo;
        private readonly int[]             maxDelay      = { 2, 4, 8 };
        private          bool              isReloadingAd = false;
        private readonly string            AD_FLATFORM   = "MAX";

        public int GetPriority() => this.applovinBlueprintService.GetBlueprint().priorityRewardedAds;
        public void Initialize()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent         += this.OnAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent     += this.OnAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent      += this.OnAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent        += this.OnAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent         += this.OnAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent  += this.OnAdDisplayFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += this.OnAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent    += this.OnAdRevenuePaidEvent;

            this.LoadRewardedAd();
        }

        public void ShowAd(UnityAction<bool> onAdComplete, string where)
        {
            if (this.IsAdReady())
            {
                this.countReloadVideo = 0;
                this.onAdComplete     = onAdComplete;
                MaxSdk.ShowRewardedAd(this.applovinBlueprintService.GetBlueprint().rewardedAdUnitId, where);
                this.signalBus.Fire<OnRewardedShowSignal>(new(this.AD_FLATFORM, where));
                return;
            }
            onAdComplete?.Invoke(false);
        }

        public bool IsAdReady()
        {
            if (MaxSdk.IsRewardedAdReady(this.applovinBlueprintService.GetBlueprint().rewardedAdUnitId)) return true;
            if (this.countReloadVideo < 3 && !this.isReloadingAd)
            {
                this.LoadRewardedAd();
                this.isReloadingAd = true;
                UniTask.Delay(TimeSpan.FromSeconds(this.maxDelay[this.countReloadVideo])).ContinueWith(() =>
                {
                    this.isReloadingAd = false;
                });
                this.countReloadVideo++;
            }
            return false;
        }

        private void LoadRewardedAd()
        {
            MaxSdk.LoadRewardedAd(this.applovinBlueprintService.GetBlueprint().rewardedAdUnitId);
        }

        #region Callbacks

        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.signalBus.Fire<OnRewardedAdRevenuePaidEventSignal>(new(this.AD_FLATFORM, adInfo.Placement, adInfo.Revenue, adInfo.RevenuePrecision));
        }

        private void OnAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward adRewardInfo, MaxSdkBase.AdInfo adInfo)
        {
            this.onAdComplete?.Invoke(true);
            this.onAdComplete = null;
            this.signalBus.Fire<OnRewardedAdReceivedRewardEventSignal>(new(this.AD_FLATFORM, adInfo.Placement, adRewardInfo.Label, adRewardInfo.Amount));
        }

        private void OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adErrorInfo, MaxSdkBase.AdInfo adInfo)
        {
            this.onAdComplete?.Invoke(false);
            this.onAdComplete = null;
            this.LoadRewardedAd();
            this.signalBus.Fire<OnRewardedAdDisplayFailedEventSignal>(new(this.AD_FLATFORM, adInfo.Placement, adErrorInfo.Message));
        }

        private void OnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.onAdComplete?.Invoke(false);
            this.onAdComplete = null;
            this.LoadRewardedAd();
            this.signalBus.Fire<OnRewardedAdHiddenEventSignal>(new(this.AD_FLATFORM, adInfo.Placement));
        }

        private void OnAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.signalBus.Fire<OnRewardedAdClickedEventSignal>(new(this.AD_FLATFORM, adInfo.Placement));
        }

        private void OnAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.signalBus.Fire<OnRewardedAdDisplayedEventSignal>(new(this.AD_FLATFORM, adInfo.Placement));
        }

        private void OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adErrorInfo)
        {
            this.retryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, this.retryAttempt));
            UniTask.Delay(TimeSpan.FromSeconds(retryDelay)).ContinueWith(this.LoadRewardedAd);
            this.signalBus.Fire<OnRewardedAdLoadFailedEventSignal>(new(this.AD_FLATFORM, adErrorInfo.Message));
        }

        private void OnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.retryAttempt = 0;
            this.signalBus.Fire<OnRewardedAdLoadedEventSignal>(new(this.AD_FLATFORM, adInfo.Placement));
        }

        #endregion
    }
}
#endif