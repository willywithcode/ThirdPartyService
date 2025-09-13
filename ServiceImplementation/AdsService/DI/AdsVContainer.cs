namespace ThirdPartyService.ServiceImplementation.AdsService.DI
{
    using ThirdPartyService.ServiceImplementation.AdsService.DummyAds.DI;
    using VContainer;
    #if MAX
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.DI;
    #endif

    #if Admob
    using ThirdParty.ServiceImplementation.AdsService.Admob.DI;
    #endif
    #if IronSource
    using ThirdParty.ServiceImplementation.AdsService.IronSource.DI;
    #endif

    public static class AdsVContainer
    {
        public static void RegisterAds(this IContainerBuilder builder)
        {
            builder.RegisterDummyAds();
            #if MAX
            builder.RegisterAPPLOVINAds();
            #endif
            #if Admob
            builder.RegisterAdmobAds();
            #endif
            #if IronSource
            builder.RegisterIronSourceAds();
            #endif
        }
    }
}