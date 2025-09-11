namespace ThirdParty.ServiceImplementation.AdsService.AppLovin.MREC {
    using System;
    using GameFoundation.Scripts.Addressable;
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.ServiceImplementation.DI.MRECAds;

    public class MAXMRECAdsService : IMRECAdsService {
        private string adUnitId;

        public MAXMRECAdsService(IAssetsManager assetsManager) {
            adUnitId = assetsManager.LoadAsset<APPLOVINSetting>("APPLOVINSetting").mrecAdUnitId;
        }

        private bool isShown;

        public void Initialize() {
            MaxSdk.CreateMRec(adUnitId, MaxSdkBase.AdViewPosition.Centered);

            MaxSdkCallbacks.MRec.OnAdLoadedEvent      += OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent  += OnMRecAdLoadFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent     += OnMRecAdClickedEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;
            MaxSdkCallbacks.MRec.OnAdExpandedEvent    += OnMRecAdExpandedEvent;
            MaxSdkCallbacks.MRec.OnAdCollapsedEvent   += OnMRecAdCollapsedEvent;
        }

        public void ShowMREC(MRECAdsPosition position) {
            MaxSdk.UpdateBannerPosition(adUnitId,ConvertPosition(position));
            MaxSdk.ShowBanner(adUnitId);
            isShown = true;
        }

        public void HideMREC() {
            MaxSdk.HideBanner(adUnitId);
            isShown = false;
        }

        public bool IsShown()       => isShown;
        public bool IsInitialized() {
            return MaxSdk.IsInitialized();
        }

        private MaxSdkBase.AdViewPosition ConvertPosition(MRECAdsPosition position) {
            return position switch {
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