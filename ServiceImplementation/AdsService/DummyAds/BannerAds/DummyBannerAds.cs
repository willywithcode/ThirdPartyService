namespace ThirdPartyService.ServiceImplementation.AdsService.DummyAds.BannerAds
{
    using GameFoundation.Scripts.Patterns.MVP.Screen;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.Signals;

    public class DummyBannerAds : IBannerAdsService
    {
        private readonly IScreenManager screenManager;
        private readonly SignalBus      signalBus;

        public DummyBannerAds(IScreenManager screenManager, SignalBus signalBus)
        {
            this.screenManager = screenManager;
            this.signalBus     = signalBus;

        }
        public int GetPriority() => 1;
        public void Initialize() {
        }

        public void ShowBanner(BannerPosition position)
        {
            this.screenManager.ShowScreen<FakeBannerSplashPresenter, FakeBannerSplashModel>(new(position));
            // The fake banner is always there at once. Its height is in canvas units, not pixels, so
            // it reports 0 and a listener falls back to the standard banner height.
            this.signalBus?.Fire(new OnBannerVisibilityChangedSignal(true, 0f));
        }

        public void HideBanner()
        {
            this.screenManager.HideScreen<FakeBannerSplashPresenter>();
            this.signalBus?.Fire(new OnBannerVisibilityChangedSignal(false, 0f));
        }

        public float GetBannerHeight()
        {
            if (this.screenManager.GetScreen<FakeBannerSplashPresenter>() is null) return 0f;
            return this.screenManager.GetScreen<FakeBannerSplashPresenter>().GetHeight();
        }

        public bool IsInitialized() => true;
        public bool IsShown()       => this.screenManager.IsScreenOpen<FakeBannerSplashPresenter>();
    }
}