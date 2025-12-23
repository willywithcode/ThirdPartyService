namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.Banner
{
    #if Admob
    using System;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using GoogleMobileAds.Api;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Blueprints;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.Signals;

    public class AdmobBannerAds : IBannerAdsService
    {
        private readonly AdmobSettingBlueprintService admobSettingBlueprintService;
        private readonly SignalBus                    signalBus;

        public AdmobBannerAds(
            AdmobSettingBlueprintService admobSettingBlueprintService,
            SignalBus                    signalBus
        )
        {
            this.admobSettingBlueprintService = admobSettingBlueprintService;
            this.signalBus                    = signalBus;
        }

        private          BannerView bannerView;
        private          bool       isShown;
        private readonly string     AD_FLATFORM = "Admob";

        public int GetPriority() => this.admobSettingBlueprintService.GetBlueprint().priorityBanner;
        public void Initialize()
        {
            // Create a 320x50 banner at top of the screen.
            if (this.bannerView != null) this.bannerView.Destroy();

            var adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            this.bannerView = new(this.admobSettingBlueprintService.GetBlueprint().bannerAdUnitId, adaptiveSize, AdPosition.Bottom);
            // Send a request to load an ad into the banner view.
            var adRequest = new AdRequest();
            if (this.admobSettingBlueprintService.GetBlueprint().isCollapseBanner) adRequest.Extras.Add("collapsible", "bottom");
            this.bannerView.LoadAd(adRequest);
            this.bannerView.OnBannerAdLoaded += () =>
            {
                // Raised when an ad is loaded into the banner view.
                this.signalBus.Fire<OnBannerAdLoadedEventSignal>(new(this.AD_FLATFORM, ""));
            };
            this.bannerView.OnBannerAdLoadFailed += error =>
            {
                // Raised when an ad fails to load into the banner view.
                this.signalBus.Fire<OnBannerAdLoadFailedEventSignal>(new(this.AD_FLATFORM, error.GetMessage()));
            };
            this.bannerView.OnAdPaid += adValue =>
            {
                // Raised when the ad is estimated to have earned money.
                this.signalBus.Fire<OnBannerAdRevenuePaidEventSignal>(new(this.AD_FLATFORM, "", adValue.Value, adValue.CurrencyCode));
            };
            this.bannerView.OnAdImpressionRecorded += () =>
            {
                // Raised when an impression is recorded for an ad.
            };
            this.bannerView.OnAdClicked += () =>
            {
                // Raised when a click is recorded for an ad.
                this.signalBus.Fire<OnBannerAdClickedEventSignal>(new(this.AD_FLATFORM, ""));
            };
            this.bannerView.OnAdFullScreenContentOpened += () =>
            {
                // Raised when an ad opened full screen content.
                this.signalBus.Fire<OnBannerAdExpandedEventSignal>(new(this.AD_FLATFORM, ""));
            };
            this.bannerView.OnAdFullScreenContentClosed += () =>
            {
                // Raised when the ad closed full screen content.
                this.signalBus.Fire<OnBannerAdCollapsedEventSignal>(new(this.AD_FLATFORM, ""));
            };
        }

        public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter)
        {
            if (position != BannerPosition.BottomCenter) this.bannerView.SetPosition(this.Convert(position));
            this.bannerView.Show();
            this.isShown = true;
            this.signalBus.Fire<OnShowBannerSignal>(new(this.AD_FLATFORM, ""));
        }

        public void HideBanner()
        {
            if (this.bannerView != null)
            {
                this.bannerView.Hide();
                this.isShown = false;
                this.signalBus.Fire<OnHideBannerSignal>(new(this.AD_FLATFORM, ""));
            }
        }

        public float GetBannerHeight()
        {
            return this.bannerView.GetHeightInPixels();
        }

        public bool IsInitialized()
        {
            return this.bannerView != null;
        }

        public bool IsShown()
        {
            return this.isShown;
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
                _                           => AdPosition.Bottom
            };
        }
    }
    #endif
}