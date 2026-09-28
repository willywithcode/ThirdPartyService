namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.MRECAds
{
    #if LevelPlay
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.MRECAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;

    // LevelPlay 9.5.1 serves MREC through its banner API with the MEDIUM_RECTANGLE size, on the MREC
    // ad unit when LevelPlaySettings names one and on the banner unit otherwise.
    public class LevelPlayMRECAds : LevelPlayBannerSlot, IMRECAdsService
    {
        public LevelPlayMRECAds(LevelPlaySdkSession session, IAdsSdk sdk, ILevelPlaySettingsProvider settings, IAdsScheduler scheduler, AdEventLog log)
            : base(BannerAdSize.MediumRectangle, session, sdk, settings, scheduler, log) { }

        protected override AdFormat Format => AdFormat.MREC;

        protected override string AdUnitId(LevelPlayPlatformSettings current) => current.MrecAdUnitId;

        public void ShowMREC(MRECAdsPosition position) => this.ShowAt(ToBannerPosition(position));

        public void HideMREC() => this.HideSlot();

        public static BannerPosition ToBannerPosition(MRECAdsPosition position) => position switch
        {
            MRECAdsPosition.TopLeft      => BannerPosition.TopLeft,
            MRECAdsPosition.TopCenter    => BannerPosition.TopCenter,
            MRECAdsPosition.TopRight     => BannerPosition.TopRight,
            MRECAdsPosition.Centered     => BannerPosition.Centered,
            MRECAdsPosition.CenterLeft   => BannerPosition.CenterLeft,
            MRECAdsPosition.CenterRight  => BannerPosition.CenterRight,
            MRECAdsPosition.BottomLeft   => BannerPosition.BottomLeft,
            MRECAdsPosition.BottomCenter => BannerPosition.BottomCenter,
            MRECAdsPosition.BottomRight  => BannerPosition.BottomRight,
            _                            => BannerPosition.Centered,
        };
    }
    #endif
}
