namespace ThirdPartyService.ServiceImplementation.AdsService.IronSource.Banner
{
    using com.unity3d.mediation;
    using GameFoundation.Scripts.Addressable;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.ServiceImplementation.AdsService.IronSource.Blueprints;

    public class IronSourceBannerAds : IBannerAdsService
    {
        private string adUnitId;

        public IronSourceBannerAds(IAssetsManager assetsManager)
        {
            this.adUnitId = assetsManager.LoadAsset<IronSourceSetting>("IronSourceSetting").bannerAdKey;
        }

        private LevelPlayBannerAd bannerAd;
        private bool              isShown;

        public void Initialize()
        {
            // Create banner instance
            this.bannerAd = new LevelPlayBannerAd(this.adUnitId,
                size: LevelPlayAdSize.BANNER,
                position: LevelPlayBannerPosition.BottomCenter,
                respectSafeArea: true);

            // Subscribe BannerAd events
            this.bannerAd.OnAdLoaded          += this.BannerOnAdLoadedEvent;
            this.bannerAd.OnAdLoadFailed      += this.BannerOnAdLoadFailedEvent;
            this.bannerAd.OnAdDisplayed       += this.BannerOnAdDisplayedEvent;
            this.bannerAd.OnAdDisplayFailed   += this.BannerOnAdDisplayFailedEvent;
            this.bannerAd.OnAdClicked         += this.BannerOnAdClickedEvent;
            this.bannerAd.OnAdCollapsed       += this.BannerOnAdCollapsedEvent;
            this.bannerAd.OnAdLeftApplication += this.BannerOnAdLeftApplicationEvent;
            this.bannerAd.OnAdExpanded        += this.BannerOnAdExpandedEvent;
            this.Load();
        }

        public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter)
        {
            this.bannerAd.ShowAd();
            this.isShown = true;
        }

        public void HideBanner()
        {
            this.bannerAd.HideAd();
            this.isShown = false;
        }

        public float GetBannerHeight() => this.bannerAd.GetAdSize().Height;

        public bool IsInitialized() => this.bannerAd != null;

        public bool IsShown() => this.bannerAd != null && this.isShown;

        private void Load()
        {
            this.bannerAd.LoadAd();
        }

        private void Destroy()
        {
            if (this.bannerAd != null)
            {
                this.bannerAd.DestroyAd();
                this.bannerAd = null;
            }
        }

        //Implement BannAd Events
        public void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
        {
            // Ad loaded successfully
        }

        public void BannerOnAdLoadFailedEvent(LevelPlayAdError ironSourceError)
        {
            // Ad failed to load. Inspect error for more details.
        }

        public void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            // Ad was clicked.
        }

        public void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            // Ad displayed.
        }

        public void BannerOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError adInfoError)
        {
            // Ad failed to display. Inspect error for more details.
        }

        public void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo)
        {
            // Ad collapsed.
        }

        public void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo)
        {
            // Ad left application.
        }

        public void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo)
        {
            // Ad expanded.
        }
    }
}