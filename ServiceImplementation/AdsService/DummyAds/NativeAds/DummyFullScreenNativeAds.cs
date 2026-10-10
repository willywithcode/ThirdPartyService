namespace ThirdPartyService.ServiceImplementation.AdsService.DummyAds.NativeAds
{
    using ThirdPartyService.Core.AdsService.NativeAds;
    using UnityEngine;
    using UnityEngine.Events;

    public class DummyFullScreenNativeAds : IFullScreenNativeAdsService
    {
        public void Load(string placement)
        {
            // Do nothing
        }

        public bool IsReady(string placement) => true;

        public void Show(string placement, UnityAction<bool> onClosed)
        {
            Debug.Log($"DummyFullScreenNativeAds: Show {placement}");
            onClosed?.Invoke(true);
        }
    }
}
