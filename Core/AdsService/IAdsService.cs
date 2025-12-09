namespace ThirdPartyService.Core.AdsService
{
    public interface IAdsService
    {
        public void RemoveAds();
        public bool IsRemovedAds();
        public void ShowBannerAd();
        public void HideBannerAd();
        public float GetBannerAdHeight();
    }
}