namespace ThirdPartyService.ServiceImplementation.AdsService.DummyAds.InterstitialsAds
{
    using ThirdPartyService.ServiceImplementation.DI.InterstitialsAds;
    using UnityEngine;
    using UnityEngine.Events;

    public class DummyInterstitialAds : IInterstitialAdsService
    {
        public void Initialize()
        {
            // Do nothing
        }

        public bool IsInitialized()
        {
            return true;
        }

        public void ShowInterstitial(string where, UnityAction onAdClosed = null, UnityAction onAdFailedToShow = null)
        {
            Debug.Log("DummyInterstitialAds: ShowInterstitial called");
            onAdClosed?.Invoke();
        }

        public bool IsInterstitialReady()
        {
            return true;
        }
    }
}