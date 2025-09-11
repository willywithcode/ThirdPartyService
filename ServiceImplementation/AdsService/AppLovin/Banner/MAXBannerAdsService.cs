namespace ThirdParty.ServiceImplementation.AdsService.AppLovin.Banner {
    using System;
    using GameFoundation.Scripts.Addressable;
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.ServiceImplementation.DI.BannerAds;
    using UnityEngine;

    public class MAXBannerAdsService : IBannerAdsService {
        private string adUnitId;

        public MAXBannerAdsService(IAssetsManager assetsManager) {
            this.adUnitId = assetsManager.LoadAsset<APPLOVINSetting>("APPLOVINSetting").bannerAdUnitId;
        }

        private bool isShown;

        public void Initialize() {
            var adViewConfiguration = new MaxSdkBase.AdViewConfiguration(MaxSdkBase.AdViewPosition.Centered);

            MaxSdk.SetBannerBackgroundColor(adUnitId, Color.white);

            MaxSdkCallbacks.Banner.OnAdLoadedEvent      += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent     += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent    += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent   += OnBannerAdCollapsedEvent;
        }

        public void ShowBanner(BannerPosition bannerPosition = BannerPosition.BottomCenter) {
            var adViewPosition = ConvertPosition(bannerPosition);
            MaxSdk.UpdateBannerPosition(adUnitId, adViewPosition);
            MaxSdk.ShowBanner(adUnitId);
            isShown = true;
        }

        public void HideBanner() {
            MaxSdk.HideBanner(adUnitId);
            isShown = false;
        }

        public float GetBannerHeight() {
            throw new NotImplementedException();
        }

        public bool IsInitialized() {
            return MaxSdk.IsInitialized();
        }

        public bool IsShown() => isShown;

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

        private MaxSdkBase.AdViewPosition ConvertPosition(BannerPosition position) {
            return position switch {
                BannerPosition.TopLeft      => MaxSdkBase.AdViewPosition.TopLeft,
                BannerPosition.TopCenter    => MaxSdkBase.AdViewPosition.TopCenter,
                BannerPosition.TopRight     => MaxSdkBase.AdViewPosition.TopRight,
                BannerPosition.Centered     => MaxSdkBase.AdViewPosition.Centered,
                BannerPosition.CenterLeft   => MaxSdkBase.AdViewPosition.CenterLeft,
                BannerPosition.CenterRight  => MaxSdkBase.AdViewPosition.CenterRight,
                BannerPosition.BottomLeft   => MaxSdkBase.AdViewPosition.BottomLeft,
                BannerPosition.BottomCenter => MaxSdkBase.AdViewPosition.BottomCenter,
                BannerPosition.BottomRight  => MaxSdkBase.AdViewPosition.BottomRight,
                _                           => MaxSdkBase.AdViewPosition.BottomCenter,
            };
        }
    }
}