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