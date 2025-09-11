namespace ThirdPartyService.ServiceImplementation.DI.AOA {
    public interface IAOAAdsService {
        public void Initialize();
        public void ShowAd();
        public void HideAd();
        public bool IsShown();
        public bool IsReady();
    }
}