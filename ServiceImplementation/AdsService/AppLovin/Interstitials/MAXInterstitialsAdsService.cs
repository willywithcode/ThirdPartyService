#if MAX
namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Interstitials
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using UnityEngine.Events;

    public class MAXInterstitialsAdsService : IInterstitialAdsService
    {
        #region Inject

        public MAXInterstitialsAdsService(IAssetsManager assetsManager)
        {
            this.adUnitId = assetsManager.LoadAsset<APPLOVINSetting>("APPLOVINSetting").interstitialAdUnitId;
        }

        #endregion

        private          int         retryAttempt;
        private          UnityAction onAdClosed;
        private          UnityAction onAdFailedToShow;
        private readonly string      adUnitId;
        private          bool        isInitialized;

        public void Initialize()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent        += this.OnAdLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent    += this.OnAdLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent     += this.OnAdDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += this.OnAdDisplayFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent       += this.OnAdClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent        += this.OnAdHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent   += this.OnAdRevenuePaidEvent;
            this.Load();
            this.isInitialized = true;
        }

        public bool IsInitialized()
        {
            return this.isInitialized;
        }

        public void ShowInterstitial(string where, UnityAction onAdClosed = null, UnityAction onAdFailedToShow = null)
        {
            this.onAdClosed       = onAdClosed;
            this.onAdFailedToShow = onAdFailedToShow;
            if (this.IsInterstitialReady())
            {
                MaxSdk.ShowInterstitial(this.adUnitId, where);
            }
            else
            {
                this.onAdFailedToShow?.Invoke();
                this.onAdFailedToShow = null;
                this.Load();
            }
        }

        public bool IsInterstitialReady()
        {
            return MaxSdk.IsInterstitialReady(this.adUnitId);
        }

        #region Callbacks

        private void Load()
        {
            MaxSdk.LoadInterstitial(this.adUnitId);
        }

        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.onAdClosed?.Invoke();
            this.onAdClosed = null;
            this.Load();
        }

        private void OnAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adInfo, MaxSdkBase.AdInfo arg3)
        {
            // Interstitial ad failed to display. We recommend loading the next ad
            this.Load();
            this.onAdFailedToShow?.Invoke();
            this.onAdFailedToShow = null;
        }

        private void OnAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adInfo)
        {
            // Interstitial ad failed to load. We recommend trying to load the ad again
            // with some delay to avoid a tight loop.
            this.retryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, this.retryAttempt));
            UniTask.Delay(TimeSpan.FromSeconds(retryDelay)).ContinueWith(this.Load);
        }

        private void OnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.retryAttempt = 0;
        }

        #endregion
    }
}
#endif