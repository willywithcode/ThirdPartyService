using ThirdPartyService.ServiceImplementation.Analytics.Appsflyer;
using ThirdPartyService.ServiceImplementation.Analytics.Firebase;
using ThirdPartyService.ServiceImplementation.Analytics.Null;
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
            #if !FIREBASE_ANALYTICS && !APPSFLYER_ANALYTICS
            // Keeps IAnalyticsService resolvable with every provider compiled out, so a define
            // toggled off changes where events go rather than whether the container can be built.
            builder.Register<NullAnalyticsService>(Lifetime.Singleton).AsImplementedInterfaces();
            #endif
        }
    }
}