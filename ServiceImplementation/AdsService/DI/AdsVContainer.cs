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
            // The Editor gets the Dummies and nothing else, so every ad in the Editor is a fake that
            // is always ready. No mediation SDK serves an ad in the Editor anyway, and a registered
            // real provider would outrank the Dummies and answer "not ready" forever.
            //
            // A build gets the real providers and no Dummy, so the waterfall can actually run out of
            // ads: the AdsService aggregator picks the highest-priority provider that is READY, and
            // an always-ready Dummy at the end of that list would make "no fill" unreachable.
            #if UNITY_EDITOR
            builder.RegisterDummyAds();
            #else
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
            #endif
        }
    }
}
