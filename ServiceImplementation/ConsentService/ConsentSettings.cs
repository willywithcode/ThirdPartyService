namespace ThirdPartyService.ServiceImplementation.ConsentService
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The AdMob app ID of a game that does not use the Google Mobile Ads Unity plugin. Games with
    /// the plugin declare the ID in its settings instead (see <see cref="AdMobAppIdSources"/>).
    /// The asset belongs to the game: create it at <see cref="AdMobAppIdSources.ConsentSettingsAssetPath"/>
    /// in the game's repository, never inside this library.
    /// </summary>
    [CreateAssetMenu(fileName = "ConsentSettings", menuName = "ThirdParty/Consent Settings")]
    public sealed class ConsentSettings : ScriptableObject
    {
        /// <summary>The Resources path the build step loads it from.</summary>
        public const string ResourcesPath = "ThirdPartyService/ConsentSettings";

        [Tooltip("This game's AdMob Android App ID (ca-app-pub-...~...). Only for games without the Google Mobile Ads Unity plugin.")]
        public string androidAdMobAppId;
    }

    /// <summary>
    /// Reads the AdMob app ID the build wrote into the app's manifest
    /// (<c>com.google.android.gms.ads.APPLICATION_ID</c>), whichever game declaration it came from.
    /// Empty off an Android device or when the manifest has none, which makes the consent flow skip UMP.
    /// </summary>
    public sealed class ConsentAppIdProvider : IConsentAppIdProvider
    {
        public const string ManifestKey = "com.google.android.gms.ads.APPLICATION_ID";

        private string appId;
        private bool   read;

        public string AppId
        {
            get
            {
                if (this.read) return this.appId;
                this.read  = true;
                this.appId = ReadFromManifest();
                return this.appId;
            }
        }

        private static string ReadFromManifest()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                const int getMetaData = 128; // PackageManager.GET_META_DATA
                using var player         = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity       = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var packageManager = activity.Call<AndroidJavaObject>("getPackageManager");
                var packageName = activity.Call<string>("getPackageName");
                using var info     = packageManager.Call<AndroidJavaObject>("getApplicationInfo", packageName, getMetaData);
                using var metaData = info.Get<AndroidJavaObject>("metaData");
                return metaData?.Call<string>("getString", ManifestKey);
            }
            catch (Exception error)
            {
                Debug.LogWarning($"[Consent] Could not read {ManifestKey} from the manifest: {error.Message}");
                return null;
            }
#else
            return null;
#endif
        }
    }
}
