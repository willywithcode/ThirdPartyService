#if UNITY_EDITOR && UNITY_ANDROID && UMP
namespace ThirdPartyService.ServiceImplementation.ConsentService.Editor
{
    using System.IO;
    using UnityEditor.Android;
    using UnityEditor.Build;

    /// <summary>
    /// Puts the UMP SDK into Android builds of games that define <c>UMP</c>, and only those
    /// (<see cref="UmpGradleDependency"/>). A game without the define gets no UMP SDK and the
    /// consent flow's Dummy.
    /// </summary>
    public sealed class UmpGradlePostprocessor : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 100;

        public void OnPostGenerateGradleAndroidProject(string unityLibraryPath)
        {
            var gradlePath = Path.Combine(unityLibraryPath, "build.gradle");
            var gradle     = File.ReadAllText(gradlePath);
            var withUmp    = UmpGradleDependency.AddTo(gradle);
            if (withUmp == null) throw new BuildFailedException($"[ThirdPartyService] {gradlePath} has no dependencies block to add {UmpGradleDependency.Coordinates} to (define UMP).");
            if (withUmp != gradle) File.WriteAllText(gradlePath, withUmp);
        }
    }
}
#endif
