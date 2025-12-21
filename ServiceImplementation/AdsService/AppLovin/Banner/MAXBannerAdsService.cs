#if MAX
namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Banner
{
    using System;
    using GameFoundation.Scripts.Addressable;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using UnityEngine;

    public class MAXBannerAdsService : IBannerAdsService
    {
        private readonly APPLOVINBlueprintService applovinBlueprintService;

        public MAXBannerAdsService(APPLOVINBlueprintService applovinBlueprintService )
        {
            this.applovinBlueprintService = applovinBlueprintService;
        }

        private bool isShown;

        public int GetPriority() => this.applovinBlueprintService.GetBlueprint().priorityBannerAds;
        public void Initialize()
        {
            var adViewConfiguration = new MaxSdkBase.AdViewConfiguration(ConvertPosition(this.applovinBlueprintService.GetBlueprint().bannerPosition));
            MaxSdk.CreateBanner(this.applovinBlueprintService.GetBlueprint().bannerAdUnitId, adViewConfiguration);

            MaxSdk.SetBannerBackgroundColor(this.applovinBlueprintService.GetBlueprint().bannerAdUnitId, Color.white);

            MaxSdkCallbacks.Banner.OnAdLoadedEvent      += this.OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += this.OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent     += this.OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += this.OnBannerAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent    += this.OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent   += this.OnBannerAdCollapsedEvent;
        }

        public void ShowBanner(BannerPosition bannerPosition = BannerPosition.BottomCenter)
        {
            var adViewPosition = this.ConvertPosition(bannerPosition);
            MaxSdk.UpdateBannerPosition(this.applovinBlueprintService.GetBlueprint().bannerAdUnitId, adViewPosition);
            MaxSdk.ShowBanner(this.applovinBlueprintService.GetBlueprint().bannerAdUnitId);
            this.isShown = true;
        }

        public void HideBanner()
        {
            MaxSdk.HideBanner(this.applovinBlueprintService.GetBlueprint().bannerAdUnitId);
            this.isShown = false;
        }

        public float GetBannerHeight()
        {
            var   bannerHeight        = MaxSdk.GetBannerLayout(this.applovinBlueprintService.GetBlueprint().bannerAdUnitId).height;
            float bannerHeightInUnits = bannerHeight / Screen.dpi * 160f;
            return bannerHeightInUnits;
        }

        public bool IsInitialized()
        {
            return MaxSdk.IsInitialized();
        }

        public bool IsShown()
        {
            return this.isShown;
        }

        #region Callbacks

        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
        }

        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
        }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
        }

        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adInfo) {
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
        }

        #endregion

        private MaxSdkBase.AdViewPosition ConvertPosition(BannerPosition position)
        {
            return position switch
            {
                BannerPosition.TopLeft      => MaxSdkBase.AdViewPosition.TopLeft,
                BannerPosition.TopCenter    => MaxSdkBase.AdViewPosition.TopCenter,
                BannerPosition.TopRight     => MaxSdkBase.AdViewPosition.TopRight,
                BannerPosition.Centered     => MaxSdkBase.AdViewPosition.Centered,
                BannerPosition.CenterLeft   => MaxSdkBase.AdViewPosition.CenterLeft,
                BannerPosition.CenterRight  => MaxSdkBase.AdViewPosition.CenterRight,
                BannerPosition.BottomLeft   => MaxSdkBase.AdViewPosition.BottomLeft,
                BannerPosition.BottomCenter => MaxSdkBase.AdViewPosition.BottomCenter,
                BannerPosition.BottomRight  => MaxSdkBase.AdViewPosition.BottomRight,
                _                           => MaxSdkBase.AdViewPosition.BottomCenter
            };
        }
    }
}
#endif