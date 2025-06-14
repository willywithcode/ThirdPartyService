namespace ThirdPartyService.Core.AdsService
{
    #if ADMOB
    using GoogleMobileAds.Api;
    #endif

    public enum BannerAdsPosition
    {
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    public static class BannerAdsPositionExtensions
    {
        #if ADMOB
        public static AdPosition ToAdMobAdPosition(this BannerAdsPosition bannerAdsPosition)
        {
            return bannerAdsPosition switch
            {
                BannerAdsPosition.Top         => AdPosition.Top,
                BannerAdsPosition.Bottom      => AdPosition.Bottom,
                BannerAdsPosition.TopLeft     => AdPosition.TopLeft,
                BannerAdsPosition.TopRight    => AdPosition.TopRight,
                BannerAdsPosition.BottomLeft  => AdPosition.BottomLeft,
                BannerAdsPosition.BottomRight => AdPosition.BottomRight,
                _                             => AdPosition.Bottom,
            };
        }
        #endif
    }
}