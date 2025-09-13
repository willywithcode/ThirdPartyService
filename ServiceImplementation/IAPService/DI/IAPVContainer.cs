namespace ThirdPartyService.ServiceImplementation.IAPService.DI
{
    using ThirdPartyService.ServiceImplementation.IAPService.DummyIAP;
    using VContainer;
    #if UNITY_PURCHASING
    using ThirdPartyService.ServiceImplementation.UnityIAP.IAPService;
    #endif

    public static class IAPVContainer
    {
        public static void RegisterIAP(this IContainerBuilder builder)
        {
            #if UNITY_EDITOR
            builder.Register<DummyIAPService>(Lifetime.Singleton).AsImplementedInterfaces();
            #else
            #if UNITY_PURCHASING
            builder.Register<UnityIAPService>(Lifetime.Singleton).AsImplementedInterfaces();
            #endif
            #endif
        }
    }
}