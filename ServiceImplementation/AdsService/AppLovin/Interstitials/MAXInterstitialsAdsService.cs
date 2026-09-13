#if MAX
namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Interstitials
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using UnityEngine.Events;

    public class MAXInterstitialsAdsService : IInterstitialAdsService
    {
        #region Inject

        private readonly APPLOVINBlueprintService applovinBlueprintService;
        private readonly SignalBus                signalBus;

        public MAXInterstitialsAdsService(
            APPLOVINBlueprintService applovinBlueprintService,
            SignalBus                signalBus
        )
        {
            this.applovinBlueprintService = applovinBlueprintService;
            this.signalBus                = signalBus;
        }

        #endregion

        private int         retryAttempt;
        private UnityAction onAdClosed;
        private UnityAction onAdFailedToShow;
        private bool        isInitialized;

        public int GetPriority()
        {
            return this.applovinBlueprintService.GetBlueprint().priorityInterstitialsAds;
        }
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
        private readonly string AD_FLATFORM = "MAX";

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
                MaxSdk.ShowInterstitial(this.applovinBlueprintService.GetBlueprint().interstitialAdUnitId, where);
                this.signalBus.Fire<OnInterstitialShowSignal>(new(this.AD_FLATFORM, where));
            } else
            {
                this.onAdFailedToShow?.Invoke();
                this.onAdFailedToShow = null;
                this.Load();
            }
        }

        public bool IsInterstitialReady()
        {
            return MaxSdk.IsInterstitialReady(this.applovinBlueprintService.GetBlueprint().interstitialAdUnitId);
        }

        #region Callbacks

        private void Load()
        {
            MaxSdk.LoadInterstitial(this.applovinBlueprintService.GetBlueprint().interstitialAdUnitId);
        }

        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.signalBus.Fire<OnInterstitialAdRevenuePaidEventSignal>(new(this.AD_FLATFORM, adInfo.Placement, adInfo.Revenue, adInfo.RevenuePrecision, adUnitId));
        }

        private void OnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.onAdClosed?.Invoke();
            this.onAdClosed = null;
            this.Load();
            this.signalBus.Fire<OnInterstitialAdHiddenEventSignal>(new(this.AD_FLATFORM, adInfo.Placement));
        }

        private void OnAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.signalBus.Fire<OnInterstitialAdClickedEventSignal>(new(this.AD_FLATFORM, adInfo.Placement));
        }

        private void OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. We recommend loading the next ad
            this.Load();
            this.onAdFailedToShow?.Invoke();
            this.onAdFailedToShow = null;
            this.signalBus.Fire<OnInterstitialAdDisplayFailedEventSignal>(new(this.AD_FLATFORM, adInfo.Placement, error.Message));
        }

        private void OnAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.signalBus.Fire<OnInterstitialAdDisplayedEventSignal>(new(this.AD_FLATFORM, adInfo.Placement, adInfo.AdUnitIdentifier));
        }

        private void OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adInfo)
        {
            // Interstitial ad failed to load. We recommend trying to load the ad again
            // with some delay to avoid a tight loop.
            this.retryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, this.retryAttempt));
            UniTask.Delay(TimeSpan.FromSeconds(retryDelay)).ContinueWith(this.Load);
            this.signalBus.Fire<OnInterstitialAdLoadFailedEventSignal>(new(this.AD_FLATFORM, adInfo.Message));
        }

        private void OnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.retryAttempt = 0;
            this.signalBus.Fire<OnInterstitialAdLoadedEventSignal>(new(this.AD_FLATFORM, adInfo.Placement));
        }

        #endregion
    }
}
#endif