namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using System.Collections.Generic;
    using System.Linq;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.DI;
    using NUnit.Framework;
    using ThirdPartyService.Core.AdsService;
    using ThirdPartyService.Core.AdsService.MRECAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.DI;
    using UnityEngine;
    using VContainer;
    using VContainer.Unity;
    using Object = UnityEngine.Object;

    // The game scope's blueprint registration (GDK RegisterSOBlueprint) already registers every
    // BaseSOBlueprintService, LevelPlaySettingsBlueprintService included. With RegisterLevelPlayAds
    // on top there must still be one settings service, loading the asset once. Building the
    // container dispatches the entry points, which runs the blueprint services' Initialize.
    public class LevelPlayAdsRegistrationTests
    {
        private sealed class NullAdsService : IAdsService
        {
            public void  RemoveAds() { }
            public bool  IsRemovedAds() => false;
            public void  ShowBannerAd() { }
            public void  HideBannerAd() { }
            public float GetBannerAdHeight() => 0f;
            public bool  IsShowingBannerAd() => false;
            public void  ShowInterstitialAd(string where, UnityEngine.Events.UnityAction onShowFail = null, UnityEngine.Events.UnityAction onShowSuccess = null) { }
            public void  ShowRewardedAd(UnityEngine.Events.UnityAction<bool> onComplete, string where) { }
            public void  ShowMRECAd(MRECAdsPosition position) { }
            public void  HideMRECAd() { }
            public bool  IsShowingMRECAd() => false;
        }

        [Test]
        public void SettingsService_IsRegisteredOnce_LoadsOnce_AndServesTheSettings()
        {
            var assets = new CountingAssetsManager { Settings = ScriptableObject.CreateInstance<LevelPlaySettings>() };
            assets.Settings.android.appKey = "android-key";
            assets.Settings.ios.appKey     = "ios-key";
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IAssetsManager>(assets);
            builder.RegisterInstance<IAdsService>(new NullAdsService());
            builder.RegisterSOBlueprint();
            builder.RegisterLevelPlayAds();

            try
            {
                using var container = builder.Build();
                var settingsServices = container.Resolve<IEnumerable<IInitializable>>().OfType<LevelPlaySettingsBlueprintService>().ToList();
                Assert.That(settingsServices.Count, Is.EqualTo(1));

                Assert.That(assets.LoadedKeys.Count(k => k == "LevelPlaySettings"), Is.EqualTo(1));
                var provider = container.Resolve<ILevelPlaySettingsProvider>();
                Assert.That(provider, Is.SameAs(settingsServices[0]));
                Assert.That(provider.Current, Is.SameAs(assets.Settings.ForCurrentPlatform()));
            }
            finally
            {
                Object.DestroyImmediate(assets.Settings);
            }
        }
    }
    #endif
}
