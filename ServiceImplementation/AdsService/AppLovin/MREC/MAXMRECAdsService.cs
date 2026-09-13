#if MAX
namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.MREC
{
    using System;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.Core.AdsService.MRECAds;
    using ThirdPartyService.Core.AdsService.Signals;

    public class MAXMRECAdsService : IMRECAdsService
    {
        private readonly APPLOVINBlueprintService applovinBlueprintService;
        private readonly SignalBus                signalBus;

        public MAXMRECAdsService(
            APPLOVINBlueprintService applovinBlueprintService,
            SignalBus                signalBus
        )
        {
            this.applovinBlueprintService = applovinBlueprintService;
            this.signalBus                = signalBus;
        }

        private          bool   isShown;
        private readonly string AD_FLATFORM = "MAX";

        public int GetPriority() => this.applovinBlueprintService.GetBlueprint().priorityMRECAds;
        public void Initialize()
        {
            MaxSdk.CreateMRec(this.applovinBlueprintService.GetBlueprint().mrecAdUnitId, MaxSdkBase.AdViewPosition.Centered);

            MaxSdkCallbacks.MRec.OnAdLoadedEvent      += this.OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent  += this.OnMRecAdLoadFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent     += this.OnMRecAdClickedEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += this.OnMRecAdRevenuePaidEvent;
            MaxSdkCallbacks.MRec.OnAdExpandedEvent    += this.OnMRecAdExpandedEvent;
            MaxSdkCallbacks.MRec.OnAdCollapsedEvent   += this.OnMRecAdCollapsedEvent;
        }

        public void ShowMREC(MRECAdsPosition position)
        {
            MaxSdk.UpdateBannerPosition(this.applovinBlueprintService.GetBlueprint().mrecAdUnitId, this.ConvertPosition(position));
            MaxSdk.ShowBanner(this.applovinBlueprintService.GetBlueprint().mrecAdUnitId);
            this.isShown = true;
            this.signalBus.Fire<OnShowMRECSignal>(new(this.AD_FLATFORM, ""));
        }

        public void HideMREC()
        {
            MaxSdk.HideBanner(this.applovinBlueprintService.GetBlueprint().mrecAdUnitId);
            this.isShown = false;
            this.signalBus.Fire<OnHideMRECSignal>(new(this.AD_FLATFORM, ""));
        }

        public bool IsShown()
        {
            return this.isShown;
        }

        public bool IsInitialized()
        {
            return MaxSdk.IsInitialized();
        }

        private MaxSdkBase.AdViewPosition ConvertPosition(MRECAdsPosition position)
        {
            return position switch
            {
                MRECAdsPosition.TopLeft      => MaxSdkBase.AdViewPosition.TopLeft,
                MRECAdsPosition.TopCenter    => MaxSdkBase.AdViewPosition.TopCenter,
                MRECAdsPosition.TopRight     => MaxSdkBase.AdViewPosition.TopRight,
                MRECAdsPosition.Centered     => MaxSdkBase.AdViewPosition.Centered,
                MRECAdsPosition.CenterLeft   => MaxSdkBase.AdViewPosition.CenterLeft,
                MRECAdsPosition.CenterRight  => MaxSdkBase.AdViewPosition.CenterRight,
                MRECAdsPosition.BottomLeft   => MaxSdkBase.AdViewPosition.BottomLeft,
                MRECAdsPosition.BottomCenter => MaxSdkBase.AdViewPosition.BottomCenter,
                MRECAdsPosition.BottomRight  => MaxSdkBase.AdViewPosition.BottomRight,
                _                            => throw new ArgumentOutOfRangeException(nameof(position), position, null)
            };
        }

        #region Callbacks

        private void OnMRecAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            this.signalBus.Fire<OnMRECAdCollapsedEventSignal>(new(this.AD_FLATFORM, adInfo.Placement));
        }

        private void OnMRecAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            this.signalBus.Fire<OnMRECAdExpandedEventSignal>(new(this.AD_FLATFORM, adInfo.Placement));
        }

        private void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            this.signalBus.Fire<OnMRECAdRevenuePaidEventSignal>(new(this.AD_FLATFORM, adInfo.Placement, adInfo.Revenue, adInfo.RevenuePrecision));
        }

        private void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            this.signalBus.Fire<OnMRECAdClickedEventSignal>(new(this.AD_FLATFORM, adInfo.Placement));
        }

        private void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adInfo) {
            this.signalBus.Fire<OnMRECAdLoadFailedEventSignal>(new(this.AD_FLATFORM, adInfo.Message));
        }

        private void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            this.signalBus.Fire<OnMRECAdLoadedEventSignal>(new(this.AD_FLATFORM, adInfo.Placement));
        }

        #endregion
    }
}
#endif