namespace ThirdPartyService.ServiceImplementation.AdsService.Admob
{
    #if Admob
    using Cysharp.Threading.Tasks;
    using GoogleMobileAds.Api;
    using ThirdPartyService.Core.ConsentService;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.AOA;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.NativeAds;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.RewardedAds;
    using UnityEngine;
    using VContainer.Unity;

    // Starts the Google Mobile Ads SDK and then the formats the blueprint turns on.
    //
    // IStartable, not IInitializable, for two reasons: it runs after the blueprint services'
    // Initialize has loaded AdmobSetting, and it must not start the SDK until the consent step has
    // finished - the same order LevelPlay's Setup follows, and what docs/product/ads.md requires of
    // every ad SDK. IConsentService.GatherConsentAsync is idempotent, so both Setups awaiting it
    // shows the player one form.
    public class Setup : IStartable
    {
        private readonly AdmobAOAAds                  aoaAds;
        private readonly AdmobBannerAds               bannerAds;
        private readonly AdmobInterstitialsAds        interstitialsAds;
        private readonly AdmobRewardedAds             rewardedAds;
        private readonly AdmobNativeAds               nativeAds;
        private readonly AdmobSettingBlueprintService admobSettingBlueprintService;
        private readonly IConsentService              consent;

        public Setup(
            AdmobAOAAds                  aoaAds,
            AdmobBannerAds               bannerAds,
            AdmobInterstitialsAds        interstitialsAds,
            AdmobRewardedAds             rewardedAds,
            AdmobNativeAds               nativeAds,
            AdmobSettingBlueprintService admobSettingBlueprintService,
            IConsentService              consent
        )
        {
            this.aoaAds                       = aoaAds;
            this.bannerAds                    = bannerAds;
            this.interstitialsAds             = interstitialsAds;
            this.rewardedAds                  = rewardedAds;
            this.nativeAds                    = nativeAds;
            this.admobSettingBlueprintService = admobSettingBlueprintService;
            this.consent                      = consent;
        }

        public void Start() => this.StartAfterConsentAsync().Forget();

        private async UniTaskVoid StartAfterConsentAsync()
        {
            await this.consent.GatherConsentAsync();

            var admobSetting = this.admobSettingBlueprintService.GetBlueprint();
            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.Initialize(initstatus =>
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
