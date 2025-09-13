namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Rewarded
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using UnityEngine.Events;

    public class MAXRewardedAdsService : IRewardedAdsService
    {
        public MAXRewardedAdsService(IAssetsManager assetsManager)
        {
            this.adUnitId = assetsManager.LoadAsset<APPLOVINSetting>("APPLOVINSetting").rewardedAdUnitId;
        }

        private readonly string            adUnitId;
        private          int               retryAttempt;
        private          UnityAction<bool> onAdComplete;
        private          int               countReloadVideo;
        private readonly int[]             maxDelay      = { 2, 4, 8 };
        private          bool              isReloadingAd = false;

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
                MaxSdk.ShowRewardedAd(this.adUnitId, where);
                return;
            }
            onAdComplete?.Invoke(false);
        }

        public bool IsAdReady()
        {
            if (MaxSdk.IsRewardedAdReady(this.adUnitId)) return true;
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
            MaxSdk.LoadRewardedAd(this.adUnitId);
        }

        #region Callbacks

        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward adRewardInfo, MaxSdkBase.AdInfo adInfo)
        {
            this.onAdComplete?.Invoke(true);
            this.onAdComplete = null;
        }

        private void OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adErrorInfo, MaxSdkBase.AdInfo adInfo)
        {
            this.onAdComplete?.Invoke(false);
            this.onAdComplete = null;
            this.LoadRewardedAd();
        }

        private void OnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.onAdComplete?.Invoke(false);
            this.onAdComplete = null;
            this.LoadRewardedAd();
        }

        private void OnAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adErrorInfo)
        {
            this.retryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, this.retryAttempt));
            UniTask.Delay(TimeSpan.FromSeconds(retryDelay)).ContinueWith(this.LoadRewardedAd);
        }

        private void OnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.retryAttempt = 0;
        }

        #endregion
    }
}