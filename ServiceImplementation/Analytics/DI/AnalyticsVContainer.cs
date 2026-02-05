using ThirdPartyService.ServiceImplementation.Analytics.Appsflyer;
using ThirdPartyService.ServiceImplementation.Analytics.Firebase;
using VContainer;

namespace ThirdPartyService.ServiceImplementation.Analytics.DI {
    public static class AnalyticsVContainer {
        public static void RegisterAnalytics(this IContainerBuilder builder) {
            #if FIREBASE_ANALYTICS
            builder.Register<FirebaseAnalytics>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            #endif
            #if APPSFLYER_ANALYTICS
            builder.Register<AppsflyerAnalytics>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            #endif
        }
    }
}