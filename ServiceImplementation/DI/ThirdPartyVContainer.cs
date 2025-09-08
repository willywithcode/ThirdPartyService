namespace ThirdPartyService.ServiceImplementation.DI
{
    using ThirdParty.ServiceImplementation.AdsService.DummyAds.DI;
    using VContainer;
    #if MAX
    using ThirdParty.ServiceImplementation.AdsService.AppLovin.DI;
    #endif

    public static class ThirdPartyVContainer
    {
        public static void RegisterThirdPartyServices(this IContainerBuilder builder)
        {
            builder.RegisterDummyAds();
            #if MAX
            builder.RegisterAPPLOVINAds();
            #endif
        }
    }
}