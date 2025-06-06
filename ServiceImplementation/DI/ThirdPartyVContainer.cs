namespace ThirdPartyService.ServiceImplementation.DI
{
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin;
    using VContainer;

    public static class ThirdPartyVContainer
    {
        public static void RegisterThirdPartyServices(this IContainerBuilder builder)
        {
            builder.Register<AppLovinAdsWrapper>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}