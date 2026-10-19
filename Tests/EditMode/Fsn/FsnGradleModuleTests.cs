namespace ThirdPartyService.Tests.EditMode.Fsn
{
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.AdsService.Fsn;

    public sealed class FsnGradleModuleTests
    {
        [Test]
        public void Strip_RemovesTheModuleInclude_FromSettings()
        {
            var settings = "include ':launcher', ':unityLibrary'\ninclude 'unityLibrary:FsnForUnity.androidlib'\ninclude 'unityLibrary:NativeTemplates.androidlib'\n";

            var stripped = FsnGradleModule.Strip(settings);

            StringAssert.DoesNotContain("FsnForUnity", stripped);
            StringAssert.Contains("include 'unityLibrary:NativeTemplates.androidlib'", stripped);
            StringAssert.Contains("include ':launcher', ':unityLibrary'", stripped);
        }

        [Test]
        public void Strip_RemovesTheProjectDependency_AndKeepsTheRest_WithWindowsNewlines()
        {
            var gradle = "dependencies {\r\n    implementation project(':unityLibrary:FsnForUnity.androidlib')\r\n    implementation project(':unityLibrary:GoogleMobileAdsPlugin.androidlib')\r\n}\r\n";

            var stripped = FsnGradleModule.Strip(gradle);

            Assert.AreEqual("dependencies {\r\n    implementation project(':unityLibrary:GoogleMobileAdsPlugin.androidlib')\r\n}\r\n", stripped);
        }

        [Test]
        public void Strip_LeavesAScriptWithoutTheModule_Unchanged()
        {
            const string gradle = "dependencies {\n    implementation 'androidx.core:core:1.9.0'\n}\n";
            Assert.AreEqual(gradle, FsnGradleModule.Strip(gradle));
        }
    }
}
