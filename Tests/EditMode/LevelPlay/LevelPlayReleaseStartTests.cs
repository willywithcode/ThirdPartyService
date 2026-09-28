namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.Analytics.Null;
    using ThirdPartyService.ServiceImplementation.ConsentService;
    using UnityEngine;

    // Whether the SDK starts: the Editor and development builds always start it; a release build
    // starts it only when LevelPlaySettings.startSdkInReleaseBuilds is on, which is the class default.
    // EditMode runs in the Editor, so the release branch is proved through Setup.ShouldStartSdk.
    public class LevelPlayReleaseStartTests
    {
        [TestCase(true, true, ExpectedResult = true)]
        [TestCase(true, false, ExpectedResult = true)]
        [TestCase(false, true, ExpectedResult = true)]
        [TestCase(false, false, ExpectedResult = false)]
        public bool ShouldStartSdk_AlwaysInTheEditorOrADevelopmentBuild_OtherwiseOnlyWithReleaseStartOn(bool editorOrDevelopmentBuild, bool startSdkInReleaseBuilds) =>
            Setup.ShouldStartSdk(editorOrDevelopmentBuild, startSdkInReleaseBuilds);

        [Test]
        public void TheEditor_CountsAsAnEditorOrDevelopmentBuild()
        {
            Assert.That(Setup.IsEditorOrDevelopmentBuild, Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SetupInTheEditor_StartsTheSdk_WhateverTheReleaseStartSetting(bool startSdkInReleaseBuilds)
        {
            var rig = new AdsTestRig(initialized: false);
            rig.Settings.StartSdkInReleaseBuilds = startSdkInReleaseBuilds;
            var dummy = new DummyConsentService();
            var setup = new Setup(rig.Session, rig.Settings, rig.NewBanner(), rig.NewMrec(), rig.NewInterstitial(), rig.NewRewarded(), dummy, dummy, rig.Sdk, new NullAnalyticsService());

            setup.Start();

            Assert.That(rig.Sdk.InitAppKeys, Is.EqualTo(new[] { "app-key" }));
        }

        [Test]
        public void SettingsClassDefault_StartsTheSdkInReleaseBuilds()
        {
            var settings = ScriptableObject.CreateInstance<LevelPlaySettings>();
            try
            {
                Assert.That(settings.startSdkInReleaseBuilds, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SettingsService_ServesTheAssetsReleaseStartSetting(bool startSdkInReleaseBuilds)
        {
            var assets = new CountingAssetsManager { Settings = ScriptableObject.CreateInstance<LevelPlaySettings>() };
            assets.Settings.startSdkInReleaseBuilds = startSdkInReleaseBuilds;
            var service = new LevelPlaySettingsBlueprintService(assets);
            try
            {
                service.Initialize();

                Assert.That(service.StartSdkInReleaseBuilds, Is.EqualTo(startSdkInReleaseBuilds));
            }
            finally
            {
                Object.DestroyImmediate(assets.Settings);
            }
        }

        [Test]
        public void SettingsService_WithoutTheAsset_ServesNoKeys_AndTheClassDefault()
        {
            var service = new LevelPlaySettingsBlueprintService(new CountingAssetsManager());

            service.Initialize();

            Assert.That(service.Current, Is.Null);
            Assert.That(service.StartSdkInReleaseBuilds, Is.True);
        }
    }
    #endif
}
