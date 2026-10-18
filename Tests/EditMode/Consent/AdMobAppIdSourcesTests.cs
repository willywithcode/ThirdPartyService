namespace ThirdPartyService.Tests.EditMode.Consent
{
    using System.Linq;
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.ConsentService;

    public sealed class AdMobAppIdSourcesTests
    {
        private const string GameA = "ca-app-pub-1111111111111111~2222222222";
        private const string GameB = "ca-app-pub-3333333333333333~4444444444";

        [Test]
        public void GmaSettingsId_IsUsed_AndLeftToThePluginToWrite()
        {
            var result = AdMobAppIdSources.Resolve(GameA, null, true);

            Assert.IsNull(result.Error);
            Assert.AreEqual(GameA, result.AppId);
            Assert.IsFalse(result.InjectIntoManifest, "The Google Mobile Ads plugin writes it; a second entry would duplicate it.");
        }

        [Test]
        public void ConsentSettingsId_WithoutThePlugin_IsWrittenIntoTheManifest()
        {
            var result = AdMobAppIdSources.Resolve(null, $"  {GameA} ", true);

            Assert.IsNull(result.Error);
            Assert.AreEqual(GameA, result.AppId);
            Assert.IsTrue(result.InjectIntoManifest);
        }

        [Test]
        public void BothSources_WithTheSameId_AreFine()
        {
            var result = AdMobAppIdSources.Resolve(GameA, GameA, true);

            Assert.IsNull(result.Error);
            Assert.AreEqual(GameA, result.AppId);
            Assert.IsFalse(result.InjectIntoManifest);
        }

        [Test]
        public void TwoSources_WithDifferentIds_FailWithBothValuesAndPlaces()
        {
            var result = AdMobAppIdSources.Resolve(GameA, GameB, true);

            Assert.IsNull(result.AppId);
            StringAssert.Contains(GameA, result.Error);
            StringAssert.Contains(GameB, result.Error);
            StringAssert.Contains("Google Mobile Ads > Settings", result.Error);
            StringAssert.Contains(AdMobAppIdSources.ConsentSettingsAssetPath, result.Error);
        }

        [Test]
        public void PluginWithAnEmptyId_FailsAndSaysWhereToSetIt()
        {
            var result = AdMobAppIdSources.Resolve("  ", null, true);

            Assert.IsNull(result.AppId);
            StringAssert.Contains("Google Mobile Ads > Settings", result.Error);
        }

        [Test]
        public void ConsentSettingsWithAnEmptyId_FailsAndSaysWhereToSetIt()
        {
            var result = AdMobAppIdSources.Resolve(null, string.Empty, false);

            Assert.IsNull(result.AppId);
            StringAssert.Contains(AdMobAppIdSources.ConsentSettingsAssetPath, result.Error);
        }

        [Test]
        public void AdMobSdkInTheBuild_WithNoIdDeclared_Fails()
        {
            var result = AdMobAppIdSources.Resolve(null, null, true);

            Assert.IsNull(result.AppId);
            StringAssert.Contains("Google Mobile Ads > Settings", result.Error);
            StringAssert.Contains(AdMobAppIdSources.ConsentSettingsAssetPath, result.Error);
        }

        [Test]
        public void NoAdMobAtAll_IsSkippedSilently()
        {
            var result = AdMobAppIdSources.Resolve(null, null, false);

            Assert.IsNull(result.Error);
            Assert.IsNull(result.AppId);
            Assert.IsFalse(result.InjectIntoManifest);
        }

        [Test]
        public void ThePluginIdWins_OverAnEmptyConsentSettings()
        {
            var result = AdMobAppIdSources.Resolve(GameA, string.Empty, true);

            Assert.IsNull(result.Error);
            Assert.AreEqual(GameA, result.AppId);
        }

        [Test]
        public void TheLibraryShipsNoConsentSettingsOfItsOwn()
        {
            // The library root is wherever the game mounted it: the folder holding ServiceImplementation.
            var script = UnityEditor.AssetDatabase.FindAssets("ConsentSettings t:MonoScript")
                .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .Single(path => path.EndsWith("/ServiceImplementation/ConsentService/ConsentSettings.cs"));
            var root = script.Substring(0, script.LastIndexOf("/ServiceImplementation/", System.StringComparison.Ordinal));

            var assets = UnityEditor.AssetDatabase.FindAssets("t:ConsentSettings", new[] { root }).Select(UnityEditor.AssetDatabase.GUIDToAssetPath);
            CollectionAssert.IsEmpty(assets, "An AdMob app ID belongs to a game; ConsentSettings lives in the game's repository.");
        }
    }
}
