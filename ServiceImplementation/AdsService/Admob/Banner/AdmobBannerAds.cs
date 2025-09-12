namespace ThirdParty.ServiceImplementation.AdsService.Admob.Banner
{
    using System;
    using GameFoundation.Scripts.Addressable;
    using GoogleMobileAds.Api;
    using ThirdParty.ServiceImplementation.AdsService.Admob.Blueprints;
    using ThirdPartyService.ServiceImplementation.DI.BannerAds;

    public class AdmobBannerAds : IBannerAdsService
    {
        private readonly string adUnitId;
        private          bool   isCollapseBanner;

        public AdmobBannerAds(IAssetsManager assetsManager)
        {
            this.adUnitId         = assetsManager.LoadAsset<AdmobSetting>("AdmobSetting").bannerAdUnitId;
            this.isCollapseBanner = assetsManager.LoadAsset<AdmobSetting>("AdmobSetting").isCollapseBanner;
        }

        private BannerView bannerView;

        public void Initialize()
        {
            // Create a 320x50 banner at top of the screen.
            if (this.bannerView != null) this.bannerView.Destroy();

            var adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            this.bannerView = new(this.adUnitId, adaptiveSize, AdPosition.Bottom);
            // Send a request to load an ad into the banner view.
            var adRequest = new AdRequest();
            if (this.isCollapseBanner) adRequest.Extras.Add("collapsible", "bottom");
            this.bannerView.LoadAd(adRequest);
            this.bannerView.OnBannerAdLoaded += () =>
            {
                // Raised when an ad is loaded into the banner view.
            };
            this.bannerView.OnBannerAdLoadFailed += error =>
            {
                // Raised when an ad fails to load into the banner view.
            };
            this.bannerView.OnAdPaid += adValue =>
            {
                // Raised when the ad is estimated to have earned money.
            };
            this.bannerView.OnAdImpressionRecorded += () =>
            {
                // Raised when an impression is recorded for an ad.
            };
            this.bannerView.OnAdClicked += () =>
            {
                // Raised when a click is recorded for an ad.
            };
            this.bannerView.OnAdFullScreenContentOpened += () =>
            {
                // Raised when an ad opened full screen content.
            };
            this.bannerView.OnAdFullScreenContentClosed += () =>
            {
                // Raised when the ad closed full screen content.
            };
        }

        public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter)
        {
            if (position != BannerPosition.BottomCenter)
            {
                this.bannerView.SetPosition(this.Convert(position));
            }
            this.bannerView.Show();
        }

        public void HideBanner()
        {
            if (this.bannerView != null) this.bannerView.Hide();
        }

        public float GetBannerHeight()
        {
            return this.bannerView.GetHeightInPixels();
        }

        public bool IsInitialized()
        {
            throw new NotImplementedException();
        }

        public bool IsShown()
        {
            throw new NotImplementedException();
        }

        private AdPosition Convert(BannerPosition position)
        {
            return position switch
            {
                BannerPosition.BottomCenter => AdPosition.Bottom,
                BannerPosition.BottomRight  => AdPosition.BottomRight,
                BannerPosition.BottomLeft   => AdPosition.BottomLeft,
                BannerPosition.TopCenter    => AdPosition.Top,
                BannerPosition.TopRight     => AdPosition.TopRight,
                BannerPosition.TopLeft      => AdPosition.TopLeft,
                _                           => AdPosition.Bottom,
            };
        }
    }
}