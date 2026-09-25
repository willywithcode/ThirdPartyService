namespace ThirdPartyService.ServiceImplementation.IAPService.DI
{
    using ThirdPartyService.ServiceImplementation.IAPService.DummyIAP;
    using VContainer;
    #if UNITY_PURCHASING || UNITY_PURCHASING_V5
    using ThirdPartyService.ServiceImplementation.UnityIAP.IAPService;
    #endif

    public static class IAPVContainer
    {
        public static void RegisterIAP(this IContainerBuilder builder)
        {
            #if UNITY_EDITOR
            builder.Register<DummyIAPService>(Lifetime.Singleton).AsImplementedInterfaces();
            #elif UNITY_PURCHASING
            builder.Register<UnityIAPService>(Lifetime.Singleton).AsImplementedInterfaces();
            #elif UNITY_PURCHASING_V5
            builder.Register<UnityIAPV5Service>(Lifetime.Singleton).AsImplementedInterfaces();
            #endif
        }
    }
}
