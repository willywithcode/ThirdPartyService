namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.NativeAds
{
    #if Admob
    using GameFoundation.Scripts.Addressable;
    using GoogleMobileAds.Api;
    using MessagePipe;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Blueprints;
    using ThirdPartyService.Core.AdsService.NativeAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using UnityEngine;

    public class AdmobNativeAds : INativeAdsService
    {
        private readonly AdmobSettingBlueprintService admobSettingBlueprintService;
        private readonly string                       AD_FLATFORM = "Admob";

        public AdmobNativeAds(AdmobSettingBlueprintService admobSettingBlueprintService)
        {
            this.admobSettingBlueprintService = admobSettingBlueprintService;
        }

        private NativeOverlayAd nativeOverlayAd;

        public int GetPriority() => this.admobSettingBlueprintService.GetBlueprint().priorityNative;
        public void Initialize()
        {
            // Clean up the old ad before loading a new one.
            if (this.nativeOverlayAd != null)
            {
                this.nativeOverlayAd.Destroy();
                this.nativeOverlayAd = null;
            }

            Debug.Log("Loading native overlay ad.");

            // Create a request used to load the ad.
            var adRequest = new AdRequest();

            // Optional: Define native ad options.
            var options = new NativeAdOptions
            {
                // AdChoicesPosition = AdChoicesPlacement.TopRightCorner,
                // MediaAspectRatio  = NativeMediaAspectRatio.Any,
            };

            // Send the request to load the ad.
            NativeOverlayAd.Load(this.admobSettingBlueprintService.GetBlueprint().nativeAdUnitId, adRequest, options,
                (ad, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError("Native Overlay ad failed to load an ad " + " with error: " + error);
                        GlobalMessagePipe.GetPublisher<OnNativeAdLoadFailedEventSignal>().Publish(new OnNativeAdLoadFailedEventSignal(this.AD_FLATFORM, error.GetMessage()));
                        return;
                    }

                    // The ad should always be non-null if the error is null, but
                    // double-check to avoid a crash.
                    if (ad == null)
                    {
                        Debug.LogError("Unexpected error: Native Overlay ad load event " + " fired with null ad and null error.");
                        return;
                    }

                    // The operation completed successfully.
                    Debug.Log("Native Overlay ad loaded with response : " + ad.GetResponseInfo());
                    this.nativeOverlayAd = ad;
                    GlobalMessagePipe.GetPublisher<OnNativeAdLoadedEventSignal>().Publish(new OnNativeAdLoadedEventSignal(this.AD_FLATFORM, ""));

                    // Register to ad events to extend functionality.
                    this.RegisterEventHandlers(ad);
                });
        }

        public void Show()
        {
            if (this.nativeOverlayAd != null)
            {
                Debug.Log("Showing native overlay ad.");
                this.nativeOverlayAd.Show();
                GlobalMessagePipe.GetPublisher<OnNativeShowSignal>().Publish(new OnNativeShowSignal(this.AD_FLATFORM, ""));
            }
            else
            {
                Debug.LogError("Native Overlay ad is not ready yet.");
            }
        }

        public void Hide()
        {
            if (this.nativeOverlayAd != null)
            {
                this.nativeOverlayAd.Hide();
                GlobalMessagePipe.GetPublisher<OnNativeHideSignal>().Publish(new OnNativeHideSignal(this.AD_FLATFORM, ""));
            }
        }

        public bool IsReady()
        {
            return this.nativeOverlayAd != null;
        }

        private void RegisterEventHandlers(NativeOverlayAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += adValue =>
            {
                Debug.Log(string.Format("Native Overlay ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
                GlobalMessagePipe.GetPublisher<OnNativeAdRevenuePaidEventSignal>().Publish(new OnNativeAdRevenuePaidEventSignal(this.AD_FLATFORM, "", adValue.Value, adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Native Overlay ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Native Overlay ad was clicked.");
                GlobalMessagePipe.GetPublisher<OnNativeAdClickedEventSignal>().Publish(new OnNativeAdClickedEventSignal(this.AD_FLATFORM, ""));
            };
            // Raised when the ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Native Overlay ad full screen content opened.");
                GlobalMessagePipe.GetPublisher<OnNativeAdDisplayedEventSignal>().Publish(new OnNativeAdDisplayedEventSignal(this.AD_FLATFORM, ""));
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Native Overlay ad full screen content closed.");
                GlobalMessagePipe.GetPublisher<OnNativeAdHiddenEventSignal>().Publish(new OnNativeAdHiddenEventSignal(this.AD_FLATFORM, ""));
            };
        }
    }
    #endif
}
