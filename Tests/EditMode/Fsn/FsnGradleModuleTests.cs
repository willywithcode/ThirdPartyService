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

        // What broke the 0.0.7 build: the template came out of git with CRLF and Unity joined its
        // include lines with LF, so the old strip saw one line and took the other modules with FSN.
        [Test]
        public void Strip_RemovesOnlyTheModuleInclude_WhenUnityJoinsIncludesWithLfInACrlfTemplate()
        {
            var settings = "include ':launcher', ':unityLibrary'\r\n" +
                           "include 'unityLibrary:GoogleMobileAdsPlugin.androidlib'\ninclude 'unityLibrary:FsnForUnity.androidlib'\ninclude 'unityLibrary:FirebaseApp.androidlib'\r\n" +
                           "\r\ndependencyResolutionManagement {\r\n}\r\n";

            var stripped = FsnGradleModule.Strip(settings);

            Assert.AreEqual(
                "include ':launcher', ':unityLibrary'\r\n" +
                "include 'unityLibrary:GoogleMobileAdsPlugin.androidlib'\ninclude 'unityLibrary:FirebaseApp.androidlib'\r\n" +
                "\r\ndependencyResolutionManagement {\r\n}\r\n",
                stripped);
        }

        [Test]
        public void Strip_LeavesAScriptWithoutTheModule_Unchanged()
        {
            const string gradle = "dependencies {\n    implementation 'androidx.core:core:1.9.0'\n}\n";
            Assert.AreEqual(gradle, FsnGradleModule.Strip(gradle));
        }
    }
}
