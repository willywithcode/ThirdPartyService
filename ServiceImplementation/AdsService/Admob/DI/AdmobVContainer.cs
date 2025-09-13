namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.DI
{
    #if Admob
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.AOA;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.NativeAds;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.RewardedAds;
    using VContainer;

    public static class AdmobVContainer
    {
        public static void RegisterAdmobAds(this IContainerBuilder builder)
        {
            builder.Register<AdmobAOAAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<AdmobBannerAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<AdmobInterstitialsAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<AdmobRewardedAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<AdmobNativeAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<Setup>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
    #endif
}