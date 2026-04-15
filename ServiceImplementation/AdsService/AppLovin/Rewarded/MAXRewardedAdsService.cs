#if MAX
namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Rewarded
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using MessagePipe;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using UnityEngine.Events;

    public class MAXRewardedAdsService : IRewardedAdsService
    {
        private readonly APPLOVINBlueprintService applovinBlueprintService;

        public MAXRewardedAdsService(APPLOVINBlueprintService applovinBlueprintService)
        {
            this.applovinBlueprintService = applovinBlueprintService;
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
                GlobalMessagePipe.GetPublisher<OnRewardedShowSignal>().Publish(new OnRewardedShowSignal(this.AD_FLATFORM, where));
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
            GlobalMessagePipe.GetPublisher<OnRewardedAdRevenuePaidEventSignal>().Publish(new OnRewardedAdRevenuePaidEventSignal(this.AD_FLATFORM, adInfo.Placement, adInfo.Revenue, adInfo.RevenuePrecision));
        }

        private void OnAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward adRewardInfo, MaxSdkBase.AdInfo adInfo)
        {
            this.onAdComplete?.Invoke(true);
            this.onAdComplete = null;
            GlobalMessagePipe.GetPublisher<OnRewardedAdReceivedRewardEventSignal>().Publish(new OnRewardedAdReceivedRewardEventSignal(this.AD_FLATFORM, adInfo.Placement, adRewardInfo.Label, adRewardInfo.Amount));
        }

        private void OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adErrorInfo, MaxSdkBase.AdInfo adInfo)
        {
            this.onAdComplete?.Invoke(false);
            this.onAdComplete = null;
            this.LoadRewardedAd();
            GlobalMessagePipe.GetPublisher<OnRewardedAdDisplayFailedEventSignal>().Publish(new OnRewardedAdDisplayFailedEventSignal(this.AD_FLATFORM, adInfo.Placement, adErrorInfo.Message));
        }

        private void OnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.onAdComplete?.Invoke(false);
            this.onAdComplete = null;
            this.LoadRewardedAd();
            GlobalMessagePipe.GetPublisher<OnRewardedAdHiddenEventSignal>().Publish(new OnRewardedAdHiddenEventSignal(this.AD_FLATFORM, adInfo.Placement));
        }

        private void OnAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            GlobalMessagePipe.GetPublisher<OnRewardedAdClickedEventSignal>().Publish(new OnRewardedAdClickedEventSignal(this.AD_FLATFORM, adInfo.Placement));
        }

        private void OnAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            GlobalMessagePipe.GetPublisher<OnRewardedAdDisplayedEventSignal>().Publish(new OnRewardedAdDisplayedEventSignal(this.AD_FLATFORM, adInfo.Placement));
        }

        private void OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adErrorInfo)
        {
            this.retryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, this.retryAttempt));
            UniTask.Delay(TimeSpan.FromSeconds(retryDelay)).ContinueWith(this.LoadRewardedAd);
            GlobalMessagePipe.GetPublisher<OnRewardedAdLoadFailedEventSignal>().Publish(new OnRewardedAdLoadFailedEventSignal(this.AD_FLATFORM, adErrorInfo.Message));
        }

        private void OnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.retryAttempt = 0;
            GlobalMessagePipe.GetPublisher<OnRewardedAdLoadedEventSignal>().Publish(new OnRewardedAdLoadedEventSignal(this.AD_FLATFORM, adInfo.Placement));
        }

        #endregion
    }
}
#endif
