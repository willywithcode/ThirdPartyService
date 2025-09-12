namespace ThirdParty.ServiceImplementation.AdsService.Admob.NativeAds {
    using System;
    using GameFoundation.Scripts.Addressable;
    using GoogleMobileAds.Api;
    using ThirdParty.ServiceImplementation.AdsService.Admob.Blueprints;
    using ThirdPartyService.Core.AdsService.NativeAds;
    using UnityEngine;

    public class AdmobNativeAds : INativeAdsService {
        private string adUnitId;

        public AdmobNativeAds(IAssetsManager assetsManager) {
            this.adUnitId = assetsManager.LoadAsset<AdmobSetting>("AdmobSetting").nativeAdUnitId;
        }
        private NativeOverlayAd nativeOverlayAd;
        public void Initialize() {
            // Clean up the old ad before loading a new one.
            if (nativeOverlayAd != null)
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
            NativeOverlayAd.Load(this.adUnitId, adRequest, options,
                                 (NativeOverlayAd ad, LoadAdError error) =>
                                 {
                                     if (error != null)
                                     {
                                         Debug.LogError("Native Overlay ad failed to load an ad " +
                                                        " with error: " + error);
                                         return;
                                     }

                                     // The ad should always be non-null if the error is null, but
                                     // double-check to avoid a crash.
                                     if (ad == null)
                                     {
                                         Debug.LogError("Unexpected error: Native Overlay ad load event " +
                                                        " fired with null ad and null error.");
                                         return;
                                     }

                                     // The operation completed successfully.
                                     Debug.Log("Native Overlay ad loaded with response : " +
                                               ad.GetResponseInfo());
                                     this.nativeOverlayAd = ad;

                                     // Register to ad events to extend functionality.
                                     RegisterEventHandlers(ad);
                                 });
        }
        public void Show() {
            if (this.nativeOverlayAd != null)
            {
                Debug.Log("Showing native overlay ad.");
                this.nativeOverlayAd.Show();
            }
            else
            {
                Debug.LogError("Native Overlay ad is not ready yet.");
            }
        }
        public void Hide() {
            if (this.nativeOverlayAd != null) {
                this.nativeOverlayAd.Hide();
            }
        }
        public bool IsReady() => this.nativeOverlayAd != null;
        private void RegisterEventHandlers(NativeOverlayAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
                           {
                               Debug.Log(String.Format("Native Overlay ad paid {0} {1}.",
                                                       adValue.Value,
                                                       adValue.CurrencyCode));
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
                              };
            // Raised when the ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
                                              {
                                                  Debug.Log("Native Overlay ad full screen content opened.");
                                              };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
                                              {
                                                  Debug.Log("Native Overlay ad full screen content closed.");
                                              };
        }
    }
}