namespace ThirdPartyService.ServiceImplementation.AdsService.DummyAds.NativeAds
{
    using ThirdPartyService.Core.AdsService.NativeAds;

    public class DummyNativeAds : INativeAdsService
    {
        public int GetPriority() => 1;
        public void Initialize()
        {
            //Do nothing
        }
        public void Show()
        {
            //Do nothing
        }
        public void Hide()
        {
            //Do nothing
        }
        public bool IsReady() => true;
    }
}