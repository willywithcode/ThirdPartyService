namespace ThirdPartyService.ServiceImplementation.AdsService.DummyAds.InterstitialsAds
{
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using UnityEngine;
    using UnityEngine.Events;

    public class DummyInterstitialAds : IInterstitialAdsService
    {
        public int GetPriority() => 1;
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