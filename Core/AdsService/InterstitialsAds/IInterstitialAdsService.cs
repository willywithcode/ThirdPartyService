namespace ThirdPartyService.Core.AdsService.InterstitialsAds
{
    using UnityEngine.Events;

    public interface IInterstitialAdsService
    {
        public void Initialize();
        public bool IsInitialized();
        public void ShowInterstitial(string where, UnityAction onAdClosed = null, UnityAction onAdFailedToShow = null);
        public bool IsInterstitialReady();
    }
}