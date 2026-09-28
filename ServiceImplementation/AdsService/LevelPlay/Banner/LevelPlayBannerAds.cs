namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Banner
{
    #if LevelPlay
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;

    public class LevelPlayBannerAds : LevelPlayBannerSlot, IBannerAdsService
    {
        public LevelPlayBannerAds(LevelPlaySdkSession session, IAdsSdk sdk, ILevelPlaySettingsProvider settings, IAdsScheduler scheduler, AdEventLog log)
            : base(BannerAdSize.Banner, session, sdk, settings, scheduler, log) { }

        protected override AdFormat Format => AdFormat.Banner;

        protected override string AdUnitId(LevelPlayPlatformSettings current) => current.bannerAdUnitId;

        public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter) => this.ShowAt(position);

        public void HideBanner() => this.HideSlot();

        public float GetBannerHeight() => this.ShownHeightPixels;
    }
    #endif
}
