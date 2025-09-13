namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin
{
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.AOA;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Banner;
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

        public Setup(
            MAXAOAAdsService           aoaAdService,
            MAXBannerAdsService        bannerAdsService,
            MAXInterstitialsAdsService interstitialsAdsService,
            MAXMRECAdsService          mrecAdsService,
            MAXRewardedAdsService      rewardedAdsService
        )
        {
            this.aoaAdService            = aoaAdService;
            this.bannerAdsService        = bannerAdsService;
            this.interstitialsAdsService = interstitialsAdsService;
            this.mrecAdsService          = mrecAdsService;
            this.rewardedAdsService      = rewardedAdsService;
        }

        public void Initialize()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
            {
                this.aoaAdService.Initialize();
                this.bannerAdsService.Initialize();
                this.interstitialsAdsService.Initialize();
                this.mrecAdsService.Initialize();
                this.rewardedAdsService.Initialize();
            };

            //MaxSdk.SetSdkKey(SDK_KEY);
            MaxSdk.SetUserId("USER_ID");
            MaxSdk.InitializeSdk();
        }
    }
}