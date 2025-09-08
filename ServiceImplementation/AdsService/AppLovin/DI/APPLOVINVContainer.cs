namespace ThirdParty.ServiceImplementation.AdsService.AppLovin.DI
{
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.AOA;
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.Banner;
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.Interstitials;
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.MREC;
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.Rewarded;
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
        }
    }
}