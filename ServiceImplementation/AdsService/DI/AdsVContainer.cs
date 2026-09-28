namespace ThirdPartyService.ServiceImplementation.AdsService.DI
{
    using ThirdPartyService.ServiceImplementation.AdsService.DummyAds.DI;
    using ThirdPartyService.ServiceImplementation.ConsentService.DI;
    using VContainer;
    #if MAX
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin.DI;
    #endif

    #if Admob
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.DI;
    #endif
    #if IronSource
    using ThirdParty.ServiceImplementation.AdsService.IronSource.DI;
    #endif
    #if LevelPlay
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.DI;
    #endif

    public static class AdsVContainer
    {
        public static void RegisterAds(this IContainerBuilder builder)
        {
            builder.RegisterConsent();
            builder.Register<AdsService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            // #if UNITY_EDITOR
            builder.RegisterDummyAds();
            // #else
            #if MAX
            builder.RegisterAPPLOVINAds();
            #endif
            #if Admob
            builder.RegisterAdmobAds();
            #endif
            #if IronSource
            builder.RegisterIronSourceAds();
            #endif
            #if LevelPlay
            builder.RegisterLevelPlayAds();
            #endif
            // #endif
        }
    }
}
