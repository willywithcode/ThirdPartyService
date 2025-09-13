namespace ThirdPartyService.ServiceImplementation.AdsService.Admob {
    using GoogleMobileAds.Api;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.AOA;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.NativeAds;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.RewardedAds;
    using UnityEngine;
    using VContainer.Unity;

    public class Setup : IInitializable {
        private readonly AdmobAOAAds           aoaAds;
        private readonly AdmobBannerAds        bannerAds;
        private readonly AdmobInterstitialsAds interstitialsAds;
        private readonly AdmobRewardedAds      rewardedAds;
        private readonly AdmobNativeAds        nativeAds;

        public Setup(
            AdmobAOAAds           aoaAds,
            AdmobBannerAds        bannerAds,
            AdmobInterstitialsAds interstitialsAds,
            AdmobRewardedAds      rewardedAds,
            AdmobNativeAds        nativeAds
        ) {
            this.aoaAds           = aoaAds;
            this.bannerAds        = bannerAds;
            this.interstitialsAds = interstitialsAds;
            this.rewardedAds      = rewardedAds;
            this.nativeAds        = nativeAds;
        }

        public void Initialize() {
            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.Initialize((InitializationStatus initstatus) => {
                                     if (initstatus == null) {
                                         Debug.LogError("Google Mobile Ads initialization failed.");
                                         return;
                                     }

                                     Debug.Log("Google Mobile Ads initialization complete.");

                                     // Google Mobile Ads events are raised off the Unity Main thread. If you need to
                                     // access UnityEngine objects after initialization,
                                     // use MobileAdsEventExecutor.ExecuteInUpdate(). For more information, see:
                                     // https://developers.google.com/admob/unity/global-settings#raise_ad_events_on_the_unity_main_thread
                                     this.aoaAds.Initialize();
                                     this.bannerAds.Initialize();
                                     this.interstitialsAds.Initialize();
                                     this.rewardedAds.Initialize();
                                     this.nativeAds.Initialize();
                                 });
        }
    }
}