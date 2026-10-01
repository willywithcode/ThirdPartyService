namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.Common
{
    #if Admob
    using System;
    using GoogleMobileAds.Common;

    // Google raises Mobile Ads events off the Unity main thread (the plugin's own note, quoted in
    // Admob/Setup.cs). A callback that reaches the game - the signal bus, a UniTask the gameplay flow
    // is awaiting, anything touching a UnityEngine object - has to be back on it first, so every
    // handler that leaves this provider is wrapped here. MobileAdsEventExecutor is the plugin's
    // documented way across; its executor exists once MobileAds.Initialize has run, which is always
    // true by the time an ad raises an event.
    internal static class AdmobMainThread
    {
        public static void Run(Action action) => MobileAdsEventExecutor.ExecuteInUpdate(action);
    }
    #endif
}
