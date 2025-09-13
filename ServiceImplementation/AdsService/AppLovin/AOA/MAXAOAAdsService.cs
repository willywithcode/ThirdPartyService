#if MAX
namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.AOA
{
    using GameFoundation.Scripts.Addressable;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.Core.AdsService.AOA;

    public class MAXAOAAdsService : IAOAAdsService
    {
        private readonly string adUnitId;

        public MAXAOAAdsService(IAssetsManager assetsManager)
        {
            this.adUnitId = assetsManager.LoadAsset<APPLOVINSetting>("APPLOVINSetting").aoaAdUnitId;
        }

        public void Initialize()
        {
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent    += this.OnAppDissmissedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += this.OnAppOpenDisplayedEvent;
        }

        private void OnAppOpenDisplayedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            this.isShown = true;
        }

        private bool isShown;

        private void OnAppDissmissedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            MaxSdk.LoadAppOpenAd(this.adUnitId);
            this.isShown = false;
        }

        public void ShowAd()
        {
            if (MaxSdk.IsAppOpenAdReady(this.adUnitId))
                MaxSdk.ShowAppOpenAd(this.adUnitId);
            else
                MaxSdk.LoadAppOpenAd(this.adUnitId);
        }

        public void HideAd() {
        }

        public bool IsShown()
        {
            return this.isShown;
        }

        public bool IsReady()
        {
            return MaxSdk.IsAppOpenAdReady(this.adUnitId);
        }
    }
}
#endif