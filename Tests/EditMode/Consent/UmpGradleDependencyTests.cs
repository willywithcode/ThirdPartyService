namespace ThirdPartyService.Tests.EditMode.Consent
{
    using System.Linq;
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.ConsentService;

    public sealed class UmpGradleDependencyTests
    {
        private const string Gradle = "apply plugin: 'com.android.library'\n\ndependencies {\n    implementation fileTree(dir: 'libs', include: ['*.jar'])\n}\n\nandroid {\n}\n";

        [Test]
        public void AddsTheUmpSdkToTheDependenciesBlock_Once()
        {
            var once = UmpGradleDependency.AddTo(Gradle);

            StringAssert.Contains("dependencies {\n    implementation '" + UmpGradleDependency.Coordinates + "'", once);
            StringAssert.Contains("implementation fileTree", once);
            Assert.AreEqual(once, UmpGradleDependency.AddTo(once), "A second build step does not add it twice.");
        }

        [Test]
        public void LeavesAScriptThatAlreadyDependsOnUmp_Alone()
        {
            var withOther = Gradle.Replace("dependencies {\n", "dependencies {\n    implementation 'com.google.android.ump:user-messaging-platform:4.1.0'\n");
            Assert.AreEqual(withOther, UmpGradleDependency.AddTo(withOther));
        }

        [Test]
        public void ScriptWithoutDependencies_IsReportedAsNull()
        {
            Assert.IsNull(UmpGradleDependency.AddTo("android {\n}\n"));
        }

        [Test]
        public void TheLibraryNoLongerShipsAnEdmDependencyFileForUmp()
        {
            // An EDM *Dependencies.xml in the library would put UMP into every game's build, define UMP
            // or not. (The Google Mobile Ads plugin ships its own; that one belongs to the game.)
            var script = UnityEditor.AssetDatabase.FindAssets("UmpGradleDependency t:MonoScript")
                .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .Single(path => path.EndsWith("/ServiceImplementation/ConsentService/UmpGradleDependency.cs"));
            var root  = script.Substring(0, script.LastIndexOf("/ServiceImplementation/", System.StringComparison.Ordinal));
            var files = UnityEditor.AssetDatabase.FindAssets("Dependencies", new[] { root })
                .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith("Dependencies.xml"));
            CollectionAssert.IsEmpty(files);
        }
    }
}
