namespace ThirdPartyService.ServiceImplementation.RemoteConfig.DI
{
    using ThirdPartyService.Core.RemoteConfig;
    using ThirdPartyService.ServiceImplementation.RemoteConfig.Firebase;
    using VContainer;

    public static class RemoteConfigVContainer
    {
        public static void RegisterRemoteConfig(this IContainerBuilder builder)
        {
            #if FIREBASE_REMOTE_CONFIG
            builder.Register<FirebaseRemoteConfig>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            #endif
        }
    }
}