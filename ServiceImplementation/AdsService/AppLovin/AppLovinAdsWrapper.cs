namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin
{
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using ThirdPartyService.Core.AdsService;
    using ThirdPartyService.Core.AdsService.AOA;
    using ThirdPartyService.Core.AdsService.MREC;
    using ThirdPartyService.ServiceImplementation.Configs;
    using ThirdPartyService.ServiceImplementation.Signals;
    using VContainer.Unity;

    public class AppLovinAdsWrapper : IInitializable, IAdsService, IMRECAdsService, IAOAAdService
    {
        #region Inject

        private readonly AppLovinSetting appLovinSetting;
        private readonly SignalBus       signalBus;

        public AppLovinAdsWrapper(
            IAssetsManager assetsManager,
            SignalBus      signalBus
        )
        {
            this.signalBus       = signalBus;
            this.appLovinSetting = assetsManager.LoadAsset<ThirdPartyConfig>("ThirdPartyConfig").appLovinSetting;
        }

        #endregion

        private bool isInit;

        public virtual async void Initialize()
        {
            #if ADS_DEBUG
            MaxSdk.SetCreativeDebuggerEnabled(true);
            #else
            MaxSdk.SetCreativeDebuggerEnabled(this.appLovinSetting.CreativeDebugger);
            #endif
            MaxSdk.SetSdkKey(this.appLovinSetting.SDKKey);
            MaxSdk.InitializeSdk();
            MaxSdkCallbacks.OnSdkInitializedEvent += this.OnSDKInitializedHandler;
            await UniTask.WaitUntil(MaxSdk.IsInitialized);
            this.InitBannerAds();
            this.InitMRECAds();
            this.InitInterstitialAds();
            this.InitRewardedAds();
            this.InitAOAAds();
            if (this.appLovinSetting.MediationDebugger) MaxSdk.ShowMediationDebugger();
            this.isInit = true;
        }

        private void OnSDKInitializedHandler(MaxSdkBase.SdkConfiguration obj)
        {
            #if ADS_DEBUG
            MaxSdk.ShowMediationDebugger();
            #endif
        }

        private void InitAOAAds()
        {
        }

        private void InitRewardedAds()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent         += this.OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent     += this.OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent      += this.OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent        += this.OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent    += this.OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent         += this.OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent  += this.OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += this.OnRewardedAdReceivedRewardEvent;
            this.LoadRewardedAd();
        }

        private void InitInterstitialAds()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent        += this.OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent    += this.OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent     += this.OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent       += this.OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent        += this.OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += this.OnInterstitialAdFailedToDisplayEvent;
            this.LoadInterstitial();
        }

        private void InitMRECAds()
        {
        }

        private void InitBannerAds()
        {
            var adViewConfiguration = new MaxSdk.AdViewConfiguration(MaxSdk.AdViewPosition.BottomCenter);
            MaxSdk.CreateBanner(this.appLovinSetting.BannerAdUnitId, adViewConfiguration);

            // Set background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(this.appLovinSetting.BannerAdUnitId, this.appLovinSetting.BannerBackgroundColor);
            MaxSdkCallbacks.Banner.OnAdLoadedEvent     += this.OnBannerAdLoadedHandler;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += this.OnBannerAdLoadFailedHandler;
            MaxSdkCallbacks.Banner.OnAdClickedEvent    += this.OnBannerAdClickedHandler;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent   += this.OnBannerAdExpandedHandler;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent  += this.OnBannerAdCollapsedHandler;
        }

        public void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50)
        {
            MaxSdk.ShowBanner(this.appLovinSetting.BannerAdUnitId);
        }

        public void HideBannedAd()
        {
            MaxSdk.HideBanner(this.appLovinSetting.BannerAdUnitId);
        }

        public void DestroyBannerAd()
        {
            MaxSdk.DestroyBanner(this.appLovinSetting.BannerAdUnitId);
        }

        public bool IsInterstitialAdReady(string place)
        {
            throw new NotImplementedException();
        }

        public void ShowInterstitialAd(string place)
        {
            if (MaxSdk.IsInterstitialReady(this.appLovinSetting.InterAdUnitId)) MaxSdk.ShowInterstitial(this.appLovinSetting.InterAdUnitId);
        }

        public bool IsRewardedAdReady(string place)
        {
            throw new NotImplementedException();
        }

        public void ShowRewardedAd(string place, Action onCompleted, Action onFailed = null)
        {
            if (MaxSdk.IsRewardedAdReady(this.appLovinSetting.RewardedAdUnitId))
            {
                MaxSdk.ShowRewardedAd(this.appLovinSetting.RewardedAdUnitId);
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (_, _, _) =>
                {
                    onCompleted?.Invoke();
                };
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += (_, _, _) =>
                {
                    onFailed?.Invoke();
                };
            }
        }

        public void RemoveAds(bool revokeConsent = false)
        {
            throw new NotImplementedException();
        }

        public bool IsAdsInitialized()
        {
            return this.isInit;
        }

        public bool IsRemoveAds()
        {
            throw new NotImplementedException();
        }

        public void ShowMREC(AdViewPosition adViewPosition)
        {
            throw new NotImplementedException();
        }

        public void HideMREC(AdViewPosition adViewPosition)
        {
            throw new NotImplementedException();
        }

        public void StopMRECAutoRefresh(AdViewPosition adViewPosition)
        {
            throw new NotImplementedException();
        }

        public void StartMRECAutoRefresh(AdViewPosition adViewPosition)
        {
            throw new NotImplementedException();
        }

        public void LoadMREC(AdViewPosition adViewPosition)
        {
            throw new NotImplementedException();
        }

        public bool IsMRECReady(AdViewPosition adViewPosition)
        {
            throw new NotImplementedException();
        }

        public void HideAllMREC()
        {
            throw new NotImplementedException();
        }

        public bool IsAOAReady()
        {
            throw new NotImplementedException();
        }

        public void ShowAOAAds()
        {
            throw new NotImplementedException();
        }

        //.............
        // Banner
        //.............

        private void OnBannerAdCollapsedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new BannerAdDismissedSignal(arg2.Placement));
        }

        private void OnBannerAdExpandedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new BannerAdPresentedSignal(arg2.Placement));
        }

        private void OnBannerAdClickedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new BannerAdClickedSignal(arg2.Placement));
        }

        private void OnBannerAdLoadFailedHandler(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
            this.signalBus.Fire(new BannerAdLoadFailedSignal("empty", arg2.Message));
        }

        private void OnBannerAdLoadedHandler(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.signalBus.Fire(new BannerAdLoadedSignal(arg2.Placement));
        }

        private int retryAttempt;

        private void LoadInterstitial()
        {
            MaxSdk.LoadInterstitial(this.appLovinSetting.InterAdUnitId);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.retryAttempt = 0;
        }

        private async void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            this.retryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, this.retryAttempt));
            await UniTask.Delay(TimeSpan.FromSeconds(retryDelay));
            this.LoadInterstitial();
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
            this.LoadInterstitial();
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is hidden. Pre-load the next ad.
            this.LoadInterstitial();
        }

        private void LoadRewardedAd()
        {
            MaxSdk.LoadRewardedAd(this.appLovinSetting.RewardedAdUnitId);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.

            // Reset retry attempt
            this.retryAttempt = 0;
        }

        private async void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).

            this.retryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, this.retryAttempt));
            await UniTask.Delay(TimeSpan.FromSeconds(retryDelay));
            this.LoadRewardedAd();
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            this.LoadRewardedAd();
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            this.LoadRewardedAd();
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            // The rewarded ad displayed and the user should receive the reward.
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Ad revenue paid. Use this callback to track user revenue.
        }
    }
}