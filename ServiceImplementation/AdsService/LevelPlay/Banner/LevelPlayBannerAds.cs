namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Banner
{
    #if LevelPlay
    using GameFoundation.Scripts.Patterns.SignalBus;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;

    public class LevelPlayBannerAds : LevelPlayBannerSlot, IBannerAdsService, IBannerLoadState
    {
        private readonly SignalBus signalBus;

        public event System.Action BannerLoadStateChanged;

        public bool IsBannerLoaded() => this.IsLoaded;

        protected override void OnLoadedChanged() => this.BannerLoadStateChanged?.Invoke();

        public LevelPlayBannerAds(LevelPlaySdkSession session, IAdsSdk sdk, ILevelPlaySettingsProvider settings, IAdsScheduler scheduler, AdEventLog log, SignalBus signalBus)
            : base(BannerAdSize.Banner, session, sdk, settings, scheduler, log)
        {
            this.signalBus = signalBus;
        }

        // Only the banner reports: the MREC shares the slot but is never laid out around.
        protected override void OnVisibilityChanged(bool visible, float heightPixels) =>
            this.signalBus?.Fire(new OnBannerVisibilityChangedSignal(visible, heightPixels));

        protected override AdFormat Format => AdFormat.Banner;

        protected override string AdUnitId(LevelPlayPlatformSettings current) => current.bannerAdUnitId;

        public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter) => this.ShowAt(position);

        public void HideBanner() => this.HideSlot();

        public float GetBannerHeight() => this.ShownHeightPixels;
    }
    #endif
}
