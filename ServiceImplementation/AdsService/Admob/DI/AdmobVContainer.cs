namespace ThirdParty.ServiceImplementation.AdsService.Admob.DI
{
    using ThirdParty.ServiceImplementation.AdsService.Admob.AOA;
    using ThirdParty.ServiceImplementation.AdsService.Admob.Banner;
    using ThirdParty.ServiceImplementation.AdsService.Admob.InterstitialsAds;
    using ThirdParty.ServiceImplementation.AdsService.Admob.NativeAds;
    using ThirdParty.ServiceImplementation.AdsService.Admob.RewardedAds;
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
}