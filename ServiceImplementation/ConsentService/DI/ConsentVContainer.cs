namespace ThirdPartyService.ServiceImplementation.ConsentService.DI
{
    using ThirdPartyService.Core.ConsentService;
    using VContainer;

    public static class ConsentVContainer
    {
        /// <summary>
        /// The real UMP consent flow on Android builds of games that define UMP (which also puts the
        /// UMP SDK in the build, see UmpGradleDependency); the Dummy everywhere else.
        /// </summary>
        public static void RegisterConsent(this IContainerBuilder builder)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR && UMP
            builder.Register<ConsentAppIdProvider>(Lifetime.Singleton).As<IConsentAppIdProvider>();
            builder.Register<AndroidUmpBridge>(Lifetime.Singleton).As<IConsentBridge>();
            builder.Register<AndroidGppCcpaConsentReader>(Lifetime.Singleton).As<ICcpaConsentReader>();
            builder.Register<ConsentService>(Lifetime.Singleton).AsSelf().As<IConsentService>();
            #else
            builder.Register<DummyConsentService>(Lifetime.Singleton).As<IConsentService>().As<ICcpaConsentReader>();
            #endif
        }
    }
}
