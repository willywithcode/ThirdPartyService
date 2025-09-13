namespace ThirdParty.ServiceImplementation.AdsService.Admob.InterstitialsAds
{
    using GameFoundation.Scripts.Addressable;
    using GoogleMobileAds.Api;
    using ThirdParty.ServiceImplementation.AdsService.Admob.Blueprints;
    using ThirdPartyService.ServiceImplementation.DI.InterstitialsAds;
    using UnityEngine;
    using UnityEngine.Events;

    public class AdmobInterstitialsAds : IInterstitialAdsService
    {
        private readonly string adUnitId;

        public AdmobInterstitialsAds(IAssetsManager assetsManager)
        {
            this.adUnitId = assetsManager.LoadAsset<AdmobSetting>("AdmobSetting").interstitialAdUnitId;
        }

        private InterstitialAd interstitialAd;
        private UnityAction    onSuccess;
        private UnityAction    onFailure;

        public void Initialize()
        {
            if (this.interstitialAd != null)
            {
                this.interstitialAd.Destroy();
                this.interstitialAd = null;
            }

            Debug.Log("Loading interstitial ad.");
            this.Load();
        }

        public bool IsInitialized()
        {
            return this.interstitialAd != null;
        }

        public void ShowInterstitial(string where, UnityAction onAdClosed = null, UnityAction onAdFailedToShow = null)
        {
            if (this.interstitialAd != null && this.interstitialAd.CanShowAd())
                this.interstitialAd.Show();
            else
                onAdFailedToShow?.Invoke();
        }

        public bool IsInterstitialReady()
        {
            return this.interstitialAd != null && this.interstitialAd.CanShowAd();
        }

        private void Load()
        {
            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // Send the request to load the ad.
            InterstitialAd.Load(this.adUnitId, adRequest, (ad, error) =>
            {
                // If the operation failed with a reason.
                if (error != null)
                {
                    Debug.LogError("Interstitial ad failed to load an ad with error : " + error);
                    return;
                }
                // If the operation failed for unknown reasons.
                // This is an unexpected error, please report this bug if it happens.
                if (ad == null)
                {
                    Debug.LogError("Unexpected error: Interstitial load event fired with null ad and null error.");
                    return;
                }

                // The operation completed successfully.
                Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
                this.interstitialAd = ad;

                // Register to ad events to extend functionality.
                this.RegisterEventHandlers(ad);
            });
        }

        private void RegisterEventHandlers(InterstitialAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += adValue =>
            {
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                this.onSuccess?.Invoke();
                this.Load();
                Debug.Log("Interstitial ad full screen content closed.");
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content with error : "
                    + error);
            };
        }
    }
}