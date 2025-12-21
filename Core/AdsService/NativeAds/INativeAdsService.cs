namespace ThirdPartyService.Core.AdsService.NativeAds {
    public interface INativeAdsService {
        public int  GetPriority();
        public void Initialize();
        public void Show();
        public void Hide();
        public bool IsReady();

    }
}