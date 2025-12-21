namespace ThirdPartyService.ServiceImplementation.AdsService.DummyAds.BannerAds
{
    using GameFoundation.Scripts.Patterns.MVP.Screen;
    using ThirdPartyService.Core.AdsService.BannerAds;

    public class DummyBannerAds : IBannerAdsService
    {
        private readonly IScreenManager screenManager;

        public DummyBannerAds(IScreenManager screenManager)
        {
            this.screenManager = screenManager;

        }
        public int GetPriority() => 1;
        public void Initialize() {
        }

        public void ShowBanner(BannerPosition position)
        {
            this.screenManager.ShowScreen<FakeBannerSplashPresenter, FakeBannerSplashModel>(new(position));
        }

        public void HideBanner()
        {
            this.screenManager.HideScreen<FakeBannerSplashPresenter>();
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