namespace ThirdPartyService.ServiceImplementation.AdsService.DummyAds.BannerAds
{
    using ThirdPartyService.ServiceImplementation.DI.BannerAds;

    public class DummyBannerAds : IBannerAdsService
    {
        public void Initialize() {
        }

        public void ShowBanner(BannerPosition position) {
        }

        public void HideBanner() {
        }

        public float GetBannerHeight() => 0;

        public bool IsInitialized() => true;
        public bool IsShown()       => false;
    }
}