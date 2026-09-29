#if UNITY_EDITOR && UNITY_ANDROID
namespace ThirdPartyService.ServiceImplementation.ConsentService.Editor
{
    using System;
    using System.IO;
    using System.Xml;
    using UnityEditor.Android;
    using UnityEngine;

    // Uses the same Resources asset as the runtime provider. Empty App ID leaves the manifest
    // untouched, and the runtime service skips UMP entirely.
    public sealed class ConsentManifestPostprocessor : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 100;

        public void OnPostGenerateGradleAndroidProject(string unityLibraryPath)
        {
            var appId = Resources.Load<ConsentSettings>("ThirdPartyService/ConsentSettings")?.androidAdMobAppId;
            if (string.IsNullOrWhiteSpace(appId)) return;

            var launcherManifest = Path.Combine(unityLibraryPath, "src", "main", "AndroidManifest.xml");
            if (!File.Exists(launcherManifest)) throw new FileNotFoundException("Unity library AndroidManifest.xml is required for UMP App ID injection", launcherManifest);

            var document = new XmlDocument();
            document.Load(launcherManifest);
            var application = document.DocumentElement?.SelectSingleNode("application") as XmlElement;
            if (application == null) throw new InvalidOperationException("Launcher AndroidManifest.xml has no application element");

            const string androidNamespace = "http://schemas.android.com/apk/res/android";
            const string key = "com.google.android.gms.ads.APPLICATION_ID";
            XmlElement metadata = null;
            foreach (XmlElement child in application.GetElementsByTagName("meta-data"))
            {
                if (child.GetAttribute("name", androidNamespace) == key) { metadata = child; break; }
            }
            metadata ??= document.CreateElement("meta-data");
            metadata.SetAttribute("name", androidNamespace, key);
            metadata.SetAttribute("value", androidNamespace, appId.Trim());
            if (metadata.ParentNode == null) application.AppendChild(metadata);
            document.Save(launcherManifest);
        }
    }
}
#endif
