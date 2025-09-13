namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.DI
{
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.AOA;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Interstitials;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.MREC;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Rewarded;
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin;
    using VContainer;

    public static class APPLOVINVContainer
    {
        public static void RegisterAPPLOVINAds(this IContainerBuilder builder)
        {
            builder.Register<MAXAOAAdsService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<MAXBannerAdsService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<MAXInterstitialsAdsService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<MAXMRECAdsService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<MAXRewardedAdsService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<Setup>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}