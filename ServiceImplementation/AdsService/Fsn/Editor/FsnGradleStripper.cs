#if UNITY_EDITOR && UNITY_ANDROID && !FSN
namespace ThirdPartyService.ServiceImplementation.AdsService.Fsn.Editor
{
    using System.IO;
    using UnityEditor.Android;

    /// <summary>
    /// Keeps the FSN Android library out of Android builds of games that do not define FSN: no FSN
    /// code, resources or FsnActivity, and no compile-only reference to the Google Mobile Ads SDK
    /// for R8 to trip on in games without it (<see cref="FsnGradleModule"/>).
    /// </summary>
    public sealed class FsnGradleStripper : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string unityLibraryPath)
        {
            Strip(Path.Combine(unityLibraryPath, "..", "settings.gradle"));
            Strip(Path.Combine(unityLibraryPath, "build.gradle"));
            var module = Path.Combine(unityLibraryPath, FsnGradleModule.Folder);
            if (Directory.Exists(module)) Directory.Delete(module, true);
        }

        private static void Strip(string gradlePath)
        {
            if (!File.Exists(gradlePath)) return;
            var gradle   = File.ReadAllText(gradlePath);
            var stripped = FsnGradleModule.Strip(gradle);
            if (stripped != gradle) File.WriteAllText(gradlePath, stripped);
        }
    }
}
#endif
