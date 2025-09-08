namespace ThirdPartyService.ServiceImplementation.DI.InterstitialsAds {
    public interface IInterstitialAdsService {
        public void Initialize();
        public bool IsInitialized();
        public void ShowInterstitial();
        public bool IsInterstitialReady();
    }
}