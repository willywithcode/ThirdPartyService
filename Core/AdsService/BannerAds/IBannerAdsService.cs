namespace ThirdPartyService.ServiceImplementation.DI.BannerAds {
    public interface IBannerAdsService {
        public void  Initialize(bool isAdaptive, BannerPosition position = BannerPosition.BottomCenter);
        public void  ShowBanner(BannerPosition position = BannerPosition.BottomCenter);
        public void  HideBanner();
        public float GetBannerHeight();
        public bool  IsInitialized();
        public bool  IsShown();
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