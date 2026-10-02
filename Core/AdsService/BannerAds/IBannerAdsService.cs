namespace ThirdPartyService.Core.AdsService.BannerAds {
    public interface IBannerAdsService {
        public int GetPriority();
        public void  Initialize();
        public void  ShowBanner(BannerPosition position = BannerPosition.BottomCenter);
        public void  HideBanner();
        public float GetBannerHeight();
        public bool  IsInitialized();
        public bool  IsShown();
    }

    // Optional beside IBannerAdsService: a banner that can say whether it has an ad to show. The
    // AdsService aggregator then shows the highest-priority banner that is loaded and switches when
    // that changes. A banner without it counts as always loaded, so it is picked by priority alone.
    // Loaded is meant to stay true after the first successful load: SDKs keep the last ad on screen
    // when a refresh fails.
    public interface IBannerLoadState {
        public bool IsBannerLoaded();
        public event System.Action BannerLoadStateChanged;
    }

    public enum BannerPosition {
        TopLeft,
        TopCenter,
        TopRight,
        Centered,
        CenterLeft,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}