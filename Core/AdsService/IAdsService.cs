namespace ThirdPartyService.Core.AdsService
{
    using ThirdPartyService.Core.AdsService.MRECAds;
    using UnityEngine.Events;
    public interface IAdsService
    {
        public void  RemoveAds();
        public bool  IsRemovedAds();
        public void  ShowBannerAd();
        public void  HideBannerAd();
        public float GetBannerAdHeight();
        public bool  IsShowingBannerAd();
        public void  ShowInterstitialAd(string        where,      UnityAction onShowFail = null, UnityAction onShowSuccess = null);
        public void  ShowRewardedAd(UnityAction<bool> onComplete, string      where);
        public void  ShowMRECAd(MRECAdsPosition       position);
        public void  HideMRECAd();
        public bool  IsShowingMRECAd();

    }
}