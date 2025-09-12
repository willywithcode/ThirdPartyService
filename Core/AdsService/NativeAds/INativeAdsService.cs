namespace ThirdPartyService.Core.AdsService.NativeAds {
    public interface INativeAdsService {
        public void Initialize();
        public void Show();
        public void Hide();
        public bool IsReady();

    }
}