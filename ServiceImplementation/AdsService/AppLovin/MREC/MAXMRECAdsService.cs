#if MAX
namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.MREC
{
    using System;
    using GameFoundation.Scripts.Addressable;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.Core.AdsService.MRECAds;

    public class MAXMRECAdsService : IMRECAdsService
    {
        private readonly APPLOVINBlueprintService applovinBlueprintService;

        public MAXMRECAdsService(APPLOVINBlueprintService applovinBlueprintService )
        {
            this.applovinBlueprintService = applovinBlueprintService;
        }

        private bool isShown;

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
        }

        public void HideMREC()
        {
            MaxSdk.HideBanner(this.applovinBlueprintService.GetBlueprint().mrecAdUnitId);
            this.isShown = false;
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
        }

        private void OnMRecAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
        }

        private void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
        }

        private void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
        }

        private void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adInfo) {
        }

        private void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
        }

        #endregion
    }
}
#endif