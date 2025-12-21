#if MAX
namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin
{
    using GameFoundation.Scripts.Addressable;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.AOA;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Interstitials;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.MREC;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Rewarded;
    using VContainer.Unity;

    public class Setup : IInitializable
    {
        private readonly MAXAOAAdsService           aoaAdService;
        private readonly MAXBannerAdsService        bannerAdsService;
        private readonly MAXInterstitialsAdsService interstitialsAdsService;
        private readonly MAXMRECAdsService          mrecAdsService;
        private readonly MAXRewardedAdsService      rewardedAdsService;
        private readonly APPLOVINBlueprintService   applovinBlueprintService;

        public Setup(
            MAXAOAAdsService           aoaAdService,
            MAXBannerAdsService        bannerAdsService,
            MAXInterstitialsAdsService interstitialsAdsService,
            MAXMRECAdsService          mrecAdsService,
            MAXRewardedAdsService      rewardedAdsService,
            APPLOVINBlueprintService  applovinBlueprintService
        )
        {
            this.aoaAdService             = aoaAdService;
            this.bannerAdsService         = bannerAdsService;
            this.interstitialsAdsService  = interstitialsAdsService;
            this.mrecAdsService           = mrecAdsService;
            this.rewardedAdsService       = rewardedAdsService;
            this.applovinBlueprintService = applovinBlueprintService;
        }

        public void Initialize()
        {
            var appLovinSettings = this.applovinBlueprintService.GetBlueprint();
            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
            {

                if(appLovinSettings.useAoaAds)this.aoaAdService.Initialize();
                if(appLovinSettings.useBannerAds)this.bannerAdsService.Initialize();
                if(appLovinSettings.useInterstitialsAds)this.interstitialsAdsService.Initialize();
                if(appLovinSettings.useMRECAds) this.mrecAdsService.Initialize();
                if(appLovinSettings.useRewardedAds)this.rewardedAdsService.Initialize();
            };

            //MaxSdk.SetSdkKey(SDK_KEY);
            MaxSdk.SetUserId("USER_ID");
            MaxSdk.InitializeSdk();
        }
    }
}
#endif