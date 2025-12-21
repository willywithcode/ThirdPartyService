#if MAX
namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.AOA
{
    using GameFoundation.Scripts.Addressable;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.Core.AdsService.AOA;

    public class MAXAOAAdsService : IAOAAdsService
    {
        private readonly APPLOVINBlueprintService applovinBlueprintService;
        public MAXAOAAdsService(APPLOVINBlueprintService applovinBlueprintService )
        {
            this.applovinBlueprintService = applovinBlueprintService;
        }

        public int GetPriority() => this.applovinBlueprintService.GetBlueprint().priorityAoaAds;
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
            MaxSdk.LoadAppOpenAd(this.applovinBlueprintService.GetBlueprint().aoaAdUnitId);
            this.isShown = false;
        }

        public void ShowAd()
        {
            if (MaxSdk.IsAppOpenAdReady(this.applovinBlueprintService.GetBlueprint().aoaAdUnitId))
                MaxSdk.ShowAppOpenAd(this.applovinBlueprintService.GetBlueprint().aoaAdUnitId);
            else
                MaxSdk.LoadAppOpenAd(this.applovinBlueprintService.GetBlueprint().aoaAdUnitId);
        }

        public void HideAd() {
        }

        public bool IsShown()
        {
            return this.isShown;
        }

        public bool IsReady()
        {
            return MaxSdk.IsAppOpenAdReady(this.applovinBlueprintService.GetBlueprint().aoaAdUnitId);
        }
    }
}
#endif