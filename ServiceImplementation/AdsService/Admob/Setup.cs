namespace ThirdPartyService.ServiceImplementation.AdsService.Admob
{
    #if Admob
    using GameFoundation.Scripts.Addressable;
    using GoogleMobileAds.Api;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.AOA;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.NativeAds;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.RewardedAds;
    using UnityEngine;
    using VContainer.Unity;

    public class Setup : IInitializable
    {
        private readonly AdmobAOAAds                  aoaAds;
        private readonly AdmobBannerAds               bannerAds;
        private readonly AdmobInterstitialsAds        interstitialsAds;
        private readonly AdmobRewardedAds             rewardedAds;
        private readonly AdmobNativeAds               nativeAds;
        private readonly AdmobSettingBlueprintService admobSettingBlueprintService;

        public Setup(
            AdmobAOAAds                  aoaAds,
            AdmobBannerAds               bannerAds,
            AdmobInterstitialsAds        interstitialsAds,
            AdmobRewardedAds             rewardedAds,
            AdmobNativeAds               nativeAds,
            AdmobSettingBlueprintService admobSettingBlueprintService
        )
        {
            this.aoaAds                       = aoaAds;
            this.bannerAds                    = bannerAds;
            this.interstitialsAds             = interstitialsAds;
            this.rewardedAds                  = rewardedAds;
            this.nativeAds                    = nativeAds;
            this.admobSettingBlueprintService = admobSettingBlueprintService;
        }

        public void Initialize()
        {
            var admobSetting = this.admobSettingBlueprintService.GetBlueprint();
            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.Initialize((InitializationStatus initstatus) =>
            {
                if (initstatus == null)
                {
                    Debug.LogError("Google Mobile Ads initialization failed.");
                    return;
                }

                Debug.Log("Google Mobile Ads initialization complete.");

                // Google Mobile Ads events are raised off the Unity Main thread. If you need to
                // access UnityEngine objects after initialization,
                // use MobileAdsEventExecutor.ExecuteInUpdate(). For more information, see:
                // https://developers.google.com/admob/unity/global-settings#raise_ad_events_on_the_unity_main_thread
                if (admobSetting.useAOA) this.aoaAds.Initialize();
                if (admobSetting.useBanner) this.bannerAds.Initialize();
                if (admobSetting.useInterstitial) this.interstitialsAds.Initialize();
                if (admobSetting.useRewarded) this.rewardedAds.Initialize();
                if (admobSetting.useNative) this.nativeAds.Initialize();
            });
        }
    }
    #endif
}