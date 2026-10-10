namespace ThirdPartyService.ServiceImplementation.AdsService.Fsn
{
    using ThirdPartyService.Core.AdsService.NativeAds;
    using UnityEngine.Events;

    /// <summary>A build without FSN (define FSN, Android): never has an ad.</summary>
    public sealed class UnavailableFullScreenNativeAds : IFullScreenNativeAdsService
    {
        public void Load(string placement) { }

        public bool IsReady(string placement) => false;

        public void Show(string placement, UnityAction<bool> onClosed) => onClosed?.Invoke(false);
    }
}
