namespace ThirdParty.ServiceImplementation.AdsService.AppLovin.AOA {
    using GameFoundation.Scripts.Addressable;
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.ServiceImplementation.DI.AOA;

    public class MAXAOAAdsService : IAOAAdsService {
        private string adUnitId;

        public MAXAOAAdsService(IAssetsManager assetsManager) {
            adUnitId = assetsManager.LoadAsset<APPLOVINSetting>("APPLOVINSetting").aoaAdUnitId;
        }

        public void Initialize() {
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent    += OnAppDissmissedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAppOpenDisplayedEvent;
        }

        private void OnAppOpenDisplayedEvent(string arg1, MaxSdkBase.AdInfo arg2) {
            isShown = true;
        }

        private bool isShown;

        private void OnAppDissmissedEvent(string arg1, MaxSdkBase.AdInfo arg2) {
            MaxSdk.LoadAppOpenAd(adUnitId);
            isShown = false;
        }

        public void ShowAd() {
            if (MaxSdk.IsAppOpenAdReady(adUnitId)) {
                MaxSdk.ShowAppOpenAd(adUnitId);
            }
            else {
                MaxSdk.LoadAppOpenAd(adUnitId);
            }
        }

        public void HideAd() {
        }

        public bool IsShown() {
            return isShown;
        }

        public bool IsReady() {
            return MaxSdk.IsAppOpenAdReady(adUnitId);
        }
    }
}