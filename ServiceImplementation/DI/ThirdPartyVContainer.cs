using ThirdPartyService.ServiceImplementation.Analytics.DI;

namespace ThirdPartyService.ServiceImplementation.DI
{
    using ThirdPartyService.ServiceImplementation.AdsService.DI;
    using ThirdPartyService.ServiceImplementation.IAPService.DI;
    using ThirdPartyService.ServiceImplementation.RemoteConfig.DI;
    using VContainer;

    public static class ThirdPartyVContainer
    {
        public static void RegisterThirdPartyServices(this IContainerBuilder builder)
        {
            builder.RegisterAds();
            builder.RegisterIAP();
            builder.RegisterRemoteConfig();
            builder.RegisterAnalytics();
        }
    }
}