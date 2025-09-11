namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin {
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.AOA;
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.Banner;
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.Interstitials;
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.MREC;
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.Rewarded;
    using VContainer.Unity;

    public class Setup : IInitializable {
        private readonly MAXAOAAdsService           aoaAdService;
        private readonly MAXBannerAdsService        bannerAdsService;
        private readonly MAXInterstitialsAdsService interstitialsAdsService;
        private readonly MAXMRECAdsService          mrecAdsService;
        private readonly MAXRewardedAdsService      rewardedAdsService;

        public Setup(
            MAXAOAAdsService           aoaAdService,
            MAXBannerAdsService        bannerAdsService,
            MAXInterstitialsAdsService interstitialsAdsService,
            MAXMRECAdsService          mrecAdsService,
            MAXRewardedAdsService      rewardedAdsService
        ) {
            this.aoaAdService            = aoaAdService;
            this.bannerAdsService        = bannerAdsService;
            this.interstitialsAdsService = interstitialsAdsService;
            this.mrecAdsService          = mrecAdsService;
            this.rewardedAdsService      = rewardedAdsService;
        }

        public void Initialize() {
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => {
                                                         aoaAdService.Initialize();
                                                         bannerAdsService.Initialize();
                                                         interstitialsAdsService.Initialize();
                                                         mrecAdsService.Initialize();
                                                         rewardedAdsService.Initialize();
                                                     };

            //MaxSdk.SetSdkKey(SDK_KEY);
            MaxSdk.SetUserId("USER_ID");
            MaxSdk.InitializeSdk();
        }
    }
}