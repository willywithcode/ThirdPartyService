#if UNITY_EDITOR && UNITY_ANDROID
namespace ThirdPartyService.ServiceImplementation.ConsentService.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using UnityEditor;
    using UnityEditor.Android;
    using UnityEditor.Build;
    using UnityEngine;

    /// <summary>
    /// Checks that the game declares its AdMob app ID in its own repository, once
    /// (<see cref="AdMobAppIdSources"/>), and fails the build with where to put it when it does not.
    /// Games with the Google Mobile Ads plugin have the ID written into the manifest by the plugin;
    /// for games with a <see cref="ConsentSettings"/> asset instead, this writes it. A game with
    /// neither, whose build does not carry the AdMob SDK, does not use AdMob and is left alone.
    /// </summary>
    public sealed class ConsentManifestPostprocessor : IPostGenerateGradleAndroidProject
    {
        private const string AndroidNamespace = "http://schemas.android.com/apk/res/android";

        public int callbackOrder => 100;

        public void OnPostGenerateGradleAndroidProject(string unityLibraryPath)
        {
            var consent    = Resources.Load<ConsentSettings>(ConsentSettings.ResourcesPath);
            var resolution = AdMobAppIdSources.Resolve(
                ReadGmaAppId(),
                consent != null ? consent.androidAdMobAppId ?? string.Empty : null,
                AdMobSdkInBuild(unityLibraryPath));
            if (resolution.Error != null) throw new BuildFailedException(resolution.Error);
            if (resolution.InjectIntoManifest) WriteAppId(unityLibraryPath, resolution.AppId);
        }

        /// <summary>
        /// Whether the generated Android project pulls in play-services-ads: as a Gradle dependency
        /// (External Dependency Manager writes those into the Unity library's build.gradle) or as a
        /// copied library in its libs folder.
        /// </summary>
        private static bool AdMobSdkInBuild(string unityLibraryPath)
        {
            const string artifact = "play-services-ads";
            var gradle = Path.Combine(unityLibraryPath, "build.gradle");
            if (File.Exists(gradle) && File.ReadAllText(gradle).Contains("com.google.android.gms:" + artifact)) return true;
            var libs = Path.Combine(unityLibraryPath, "libs");
            return Directory.Exists(libs) && Directory.EnumerateFiles(libs).Any(f => Path.GetFileName(f).StartsWith(artifact, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// The Android app ID in the game's Google Mobile Ads plugin settings, read by field name so
        /// this library needs no reference to the plugin; null when the game has no such settings.
        /// </summary>
        private static string ReadGmaAppId()
        {
            var values = new Dictionary<string, string>();
            foreach (var guid in AssetDatabase.FindAssets("t:GoogleMobileAdsSettings"))
            {
                var path  = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null) continue;
                var field = new SerializedObject(asset).FindProperty("adMobAndroidAppId");
                if (field == null) throw new BuildFailedException($"[ThirdPartyService] {path} has no adMobAndroidAppId field; this Google Mobile Ads plugin version is not supported by ConsentManifestPostprocessor.");
                values[path] = field.stringValue ?? string.Empty;
            }
            if (values.Count == 0) return null;
            if (values.Values.Select(v => v.Trim()).Distinct().Count() > 1)
            {
                throw new BuildFailedException("[ThirdPartyService] Several Google Mobile Ads settings assets with different Android App IDs: "
                    + string.Join(", ", values.Select(v => $"{v.Key} = '{v.Value}'")) + ". Keep one.");
            }
            return values.Values.First();
        }

        private static void WriteAppId(string unityLibraryPath, string appId)
        {
            var manifestPath = Path.Combine(unityLibraryPath, "src", "main", "AndroidManifest.xml");
            if (!File.Exists(manifestPath)) throw new FileNotFoundException("Unity library AndroidManifest.xml is required for the AdMob app ID", manifestPath);

            var document = new XmlDocument();
            document.Load(manifestPath);
            if (document.DocumentElement?.SelectSingleNode("application") is not XmlElement application)
                throw new InvalidOperationException("Unity library AndroidManifest.xml has no application element");

            XmlElement metadata = null;
            foreach (XmlElement child in application.GetElementsByTagName("meta-data"))
            {
                if (child.GetAttribute("name", AndroidNamespace) == ConsentAppIdProvider.ManifestKey)
                {
                    metadata = child;
                    break;
                }
            }
            metadata ??= document.CreateElement("meta-data");
            metadata.SetAttribute("name", AndroidNamespace, ConsentAppIdProvider.ManifestKey);
            metadata.SetAttribute("value", AndroidNamespace, appId);
            if (metadata.ParentNode == null) application.AppendChild(metadata);
            document.Save(manifestPath);
        }
    }
}
#endif
