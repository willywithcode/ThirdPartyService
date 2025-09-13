namespace ThirdParty.ServiceImplementation.AdsService.IronSource.DI
{
    using ThirdParty.ServiceImplementation.AdsService.IronSource.Banner;
    using ThirdParty.ServiceImplementation.AdsService.IronSource.InterstitialsAds;
    using ThirdParty.ServiceImplementation.AdsService.IronSource.MRECAds;
    using ThirdParty.ServiceImplementation.AdsService.IronSource.RewardedAds;
    using VContainer.Unity;

    public static class IronSourceAdsVContainer
    {
        public static void RegisterIronSourceAds(this VContainer.IContainerBuilder builder)
        {
            builder.Register<IronSourceBannerAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<IronSourceInterstitialsAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<IronSourceRewardedAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<IronSourceMRECAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<Setup>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}