namespace ThirdPartyService.ServiceImplementation.DI
{
    using ThirdPartyService.ServiceImplementation.AdsService.DI;
    using ThirdPartyService.ServiceImplementation.IAPService.DI;
    using VContainer;

    public static class ThirdPartyVContainer
    {
        public static void RegisterThirdPartyServices(this IContainerBuilder builder)
        {
            builder.RegisterAds();
            builder.RegisterIAP();
        }
    }
}