namespace ThirdParty.ServiceImplementation.AdsService.Admob.AOA
{
    using System;
    using GameFoundation.Scripts.Addressable;
    using GoogleMobileAds.Api;
    using GoogleMobileAds.Common;
    using ThirdParty.ServiceImplementation.AdsService.Admob.Blueprints;
    using ThirdPartyService.ServiceImplementation.DI.AOA;
    using UnityEngine;

    public class AdmobAOAAds : IAOAAdsService
    {
        private readonly string adUnitId;

        public AdmobAOAAds(IAssetsManager assetsManager)
        {
            this.adUnitId = assetsManager.LoadAsset<AdmobSetting>("AdmobSetting").aoaAdUnitId;
        }

        private readonly TimeSpan  TIMEOUT = TimeSpan.FromHours(4);
        private          DateTime  expireTime;
        private          AppOpenAd appOpenAd;
        private          bool      isShown;

        public void Initialize()
        {
            this.HideAd();
            AppStateEventNotifier.AppStateChanged += (state) =>
            {
                if (state == AppState.Foreground)
                {
                    if (!this.IsReady())
                    {
                        this.ShowAd();
                    }
                }
            };
            this.LoadAppOpenAd();
        }

        public void ShowAd()
        {
            if (this.appOpenAd != null && this.appOpenAd.CanShowAd())
            {
                Debug.Log("Showing app open ad.");
                this.appOpenAd.Show();
                this.isShown = true;
            }
            else
            {
                Debug.LogError("App open ad is not ready yet.");
            }
        }

        public void HideAd()
        {
            if (this.appOpenAd != null)
            {
                this.appOpenAd.Destroy();
                this.appOpenAd = null;
                this.isShown   = false;
            }
        }

        public bool IsShown() => this.isShown;

        public bool IsReady()
        {
            return this.appOpenAd != null && DateTime.Now < this.expireTime;
        }

        private void LoadAppOpenAd() {
            Debug.Log("Loading app open ad.");
            var adRequest = new AdRequest();

            AppOpenAd.Load(this.adUnitId, adRequest, (ad, error) =>
                                                     {
                                                         // If the operation failed with a reason.
                                                         if (error != null)
                                                         {
                                                             Debug.LogError("App open ad failed to load an ad with error : "
                                                                            + error);
                                                             return;
                                                         }

                                                         // If the operation failed for unknown reasons.
                                                         // This is an unexpected error, please report this bug if it happens.
                                                         if (ad == null)
                                                         {
                                                             Debug.LogError("Unexpected error: App open ad load event fired with " + " null ad and null error.");
                                                             return;
                                                         }

                                                         // The operation completed successfully.
                                                         Debug.Log("App open ad loaded with response : " + ad.GetResponseInfo());
                                                         this.appOpenAd = ad;

                                                         // App open ads can be preloaded for up to 4 hours.
                                                         this.expireTime = DateTime.Now + this.TIMEOUT;

                                                         // Register to ad events to extend functionality.
                                                         this.RegisterEventHandlers(ad);
                                                     });
        }

        #region Callbacks

        private void RegisterEventHandlers(AppOpenAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += adValue =>
            {
                Debug.Log(string.Format("App open ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("App open ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("App open ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("App open ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("App open ad full screen content closed.");
                this.isShown = false;
                this.LoadAppOpenAd();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.LogError("App open ad failed to open full screen content " + "with error : " + error);
                this.isShown = false;
                this.LoadAppOpenAd();
            };
        }

        #endregion
    }
}