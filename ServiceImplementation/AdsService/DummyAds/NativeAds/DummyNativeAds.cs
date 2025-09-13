namespace ThirdParty.ServiceImplementation.AdsService.DummyAds.NativeAds
{
    using ThirdPartyService.Core.AdsService.NativeAds;

    public class DummyNativeAds : INativeAdsService
    {
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