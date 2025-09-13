#if IronSource
namespace ThirdPartyService.ServiceImplementation.AdsService.IronSource.DI
{
    using ThirdParty.ServiceImplementation.AdsService.IronSource;
    using ThirdPartyService.ServiceImplementation.AdsService.IronSource.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.IronSource.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.IronSource.MRECAds;
    using ThirdPartyService.ServiceImplementation.AdsService.IronSource.RewardedAds;
    using VContainer;

    public static class IronSourceAdsVContainer
    {
        public static void RegisterIronSourceAds(this IContainerBuilder builder)
        {
            builder.Register<IronSourceBannerAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<IronSourceInterstitialsAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<IronSourceRewardedAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<IronSourceMRECAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<Setup>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
#endif