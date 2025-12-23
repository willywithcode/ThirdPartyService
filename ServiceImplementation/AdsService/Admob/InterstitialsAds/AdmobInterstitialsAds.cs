namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.InterstitialsAds
{
    #if Admob
    using GameFoundation.Scripts.Patterns.SignalBus;
    using GoogleMobileAds.Api;
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Blueprints;
    using UnityEngine;
    using UnityEngine.Events;

    public class AdmobInterstitialsAds : IInterstitialAdsService
    {
        private readonly AdmobSettingBlueprintService admobSettingBlueprintService;
        private readonly SignalBus                    signalBus;

        public AdmobInterstitialsAds(
            AdmobSettingBlueprintService admobSettingBlueprintService,
            SignalBus                    signalBus
        )
        {
            this.admobSettingBlueprintService = admobSettingBlueprintService;
            this.signalBus                    = signalBus;
        }

        private InterstitialAd interstitialAd;
        private UnityAction    onSuccess;
        private UnityAction    onFailure;
        private readonly string   AD_FLATFORM = "Admob";

        public int GetPriority() => this.admobSettingBlueprintService.GetBlueprint().priorityInterstitial;
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
            {
                this.interstitialAd.Show();
                this.signalBus.Fire<OnInterstitialShowSignal>(new(this.AD_FLATFORM, where));
            }
            else onAdFailedToShow?.Invoke();
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
            InterstitialAd.Load(this.admobSettingBlueprintService.GetBlueprint().interstitialAdUnitId, adRequest, (ad, error) =>
            {
                // If the operation failed with a reason.
                if (error != null)
                {
                    Debug.LogError("Interstitial ad failed to load an ad with error : " + error);
                    this.signalBus.Fire<OnInterstitialAdLoadFailedEventSignal>(new(this.AD_FLATFORM, error.GetMessage()));
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
                this.signalBus.Fire<OnInterstitialAdLoadedEventSignal>(new(this.AD_FLATFORM, ""));

                // Register to ad events to extend functionality.
                this.RegisterEventHandlers(ad);
            });
        }

        private void RegisterEventHandlers(InterstitialAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += adValue =>
            {
                this.signalBus.Fire<OnInterstitialAdRevenuePaidEventSignal>(new(this.AD_FLATFORM,"", adValue.Value, adValue.CurrencyCode, this.admobSettingBlueprintService.GetBlueprint().interstitialAdUnitId));
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
                this.signalBus.Fire<OnInterstitialAdClickedEventSignal>(new(this.AD_FLATFORM, ""));
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
                this.signalBus.Fire<OnInterstitialAdDisplayedEventSignal>(new(this.AD_FLATFORM, "", this.admobSettingBlueprintService.GetBlueprint().interstitialAdUnitId));
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                this.onSuccess?.Invoke();
                this.Load();
                Debug.Log("Interstitial ad full screen content closed.");
                this.signalBus.Fire<OnInterstitialAdHiddenEventSignal>(new(this.AD_FLATFORM, ""));
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content with error : "
                    + error);
                this.signalBus.Fire<OnInterstitialAdDisplayFailedEventSignal>(new(this.AD_FLATFORM, "", error.GetMessage()));
            };
        }
    }
    #endif
}