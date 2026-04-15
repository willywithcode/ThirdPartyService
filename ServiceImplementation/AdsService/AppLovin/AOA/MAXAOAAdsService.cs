#if MAX
namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.AOA
{
    using GameFoundation.Scripts.Addressable;
    using MessagePipe;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.Core.AdsService.AOA;
    using ThirdPartyService.Core.AdsService.Signals;

    public class MAXAOAAdsService : IAOAAdsService
    {
        private readonly APPLOVINBlueprintService applovinBlueprintService;
        private readonly string                   AD_FLATFORM = "MAX";

        public MAXAOAAdsService(APPLOVINBlueprintService applovinBlueprintService)
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
            GlobalMessagePipe.GetPublisher<OnAOAAdDisplayedEventSignal>().Publish(new OnAOAAdDisplayedEventSignal(this.AD_FLATFORM, arg2.Placement));
        }

        private bool isShown;

        private void OnAppDissmissedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            MaxSdk.LoadAppOpenAd(this.applovinBlueprintService.GetBlueprint().aoaAdUnitId);
            this.isShown = false;
            GlobalMessagePipe.GetPublisher<OnAOAAdHiddenEventSignal>().Publish(new OnAOAAdHiddenEventSignal(this.AD_FLATFORM, arg2.Placement));
        }

        public void ShowAd()
        {
            if (MaxSdk.IsAppOpenAdReady(this.applovinBlueprintService.GetBlueprint().aoaAdUnitId))
            {
                MaxSdk.ShowAppOpenAd(this.applovinBlueprintService.GetBlueprint().aoaAdUnitId);
                GlobalMessagePipe.GetPublisher<OnAOAShowSignal>().Publish(new OnAOAShowSignal(this.AD_FLATFORM, ""));
            }
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
