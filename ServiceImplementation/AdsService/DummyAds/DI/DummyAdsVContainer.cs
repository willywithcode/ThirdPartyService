namespace ThirdPartyService.ServiceImplementation.AdsService.DummyAds.DI
{
    using ThirdPartyService.ServiceImplementation.AdsService.DummyAds.NativeAds;
    using ThirdPartyService.ServiceImplementation.AdsService.DummyAds.AOA;
    using ThirdPartyService.ServiceImplementation.AdsService.DummyAds.BannerAds;
    using ThirdPartyService.ServiceImplementation.AdsService.DummyAds.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.DummyAds.MRECAds;
    using ThirdPartyService.ServiceImplementation.AdsService.DummyAds.RewardedAds;
    using VContainer;

    public static class DummyAdsVContainer
    {
        public static void RegisterDummyAds(this IContainerBuilder builder)
        {
            builder.Register<DummyAOAAdsService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<DummyBannerAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<DummyInterstitialAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<DummyMRECAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<DummyRewardedAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<DummyNativeAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}