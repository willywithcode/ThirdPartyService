namespace ThirdPartyService.ServiceImplementation.DI
{
    using ThirdParty.ServiceImplementation.AdsService.DI;
    using VContainer;

    public static class ThirdPartyVContainer
    {
        public static void RegisterThirdPartyServices(this IContainerBuilder builder)
        {
            builder.RegisterAds();
        }
    }
}