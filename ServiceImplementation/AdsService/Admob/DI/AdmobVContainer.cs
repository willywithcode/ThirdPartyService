namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.DI
{
    #if Admob
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.AOA;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.NativeAds;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.RewardedAds;
    using VContainer;
    using VContainer.Unity;

    // Called by AdsVContainer.RegisterAds() for a build, never in the Editor. The wrappers join the
    // AdsService aggregator's per-format lists; AdmobSetting.priority* decides where they sit against
    // the other providers. AdmobSettingBlueprintService is
    // not registered here: GDK's RegisterSOBlueprint already registers every BaseSOBlueprintService in
    // the loaded assemblies, and a second registration makes the container build fail.
    public static class AdmobVContainer
    {
        public static void RegisterAdmobAds(this IContainerBuilder builder)
        {
            builder.Register<AdmobAOAAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<AdmobBannerAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<AdmobInterstitialsAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<AdmobRewardedAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<AdmobNativeAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            // RegisterEntryPoint, as LevelPlay's container does: Setup is an IStartable and needs the
            // entry point dispatcher to be registered for its Start() to ever run.
            builder.RegisterEntryPoint<Setup>();
        }
    }
    #endif
}