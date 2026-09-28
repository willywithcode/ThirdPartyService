#if LevelPlay && IronSource
#error The LevelPlay and IronSource scripting defines are both set. ServiceImplementation/AdsService/IronSource is written against the LevelPlay 8.x API and cannot sit beside the LevelPlay 9.5.1 provider: remove one of the two defines.
#endif
namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.DI
{
    #if LevelPlay
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.MRECAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.RewardedAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;
    using VContainer;
    using VContainer.Unity;

    public static class LevelPlayAdsVContainer
    {
        // Called by AdsVContainer.RegisterAds() after the Dummies. The wrappers join the Dummies in the
        // aggregator's lists and outrank them (LevelPlayAds.Priority). Native and app-open ads have no
        // LevelPlay 9.5.1 C# API, so those two keep only their Dummy. LevelPlaySettingsBlueprintService
        // is not registered here: GDK's RegisterSOBlueprint registers every BaseSOBlueprintService in
        // the loaded assemblies, this one included, and a second registration makes the container
        // build fail.
        public static void RegisterLevelPlayAds(this IContainerBuilder builder)
        {
            builder.Register<LevelPlaySdkAdapter>(Lifetime.Singleton).As<IAdsSdk>();
            builder.Register<RealtimeAdsScheduler>(Lifetime.Singleton).As<IAdsScheduler>();
            builder.Register<AdEventLog>(Lifetime.Singleton);
            builder.Register<LevelPlaySdkSession>(Lifetime.Singleton);

            builder.Register<LevelPlayBannerAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LevelPlayMRECAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LevelPlayInterstitialAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LevelPlayRewardedAds>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            builder.RegisterEntryPoint<Setup>();
        }
    }
    #endif
}
