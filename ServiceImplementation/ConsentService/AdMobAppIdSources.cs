namespace ThirdPartyService.ServiceImplementation.ConsentService
{
    /// <summary>
    /// Which AdMob app ID the game being built declares. The ID belongs to each game, never to this
    /// library. A game declares it in one of two places in its own repository:
    /// <list type="number">
    /// <item>The Google Mobile Ads Unity plugin settings (Assets &gt; Google Mobile Ads &gt; Settings),
    /// which games with the plugin already fill in. The plugin writes the ID into the manifest itself.</item>
    /// <item>A <see cref="ConsentSettings"/> asset at <see cref="ConsentSettingsAssetPath"/>, for games
    /// without the plugin. <c>ConsentManifestPostprocessor</c> writes that one into the manifest.</item>
    /// </list>
    /// Both may exist only with the same value.
    /// </summary>
    public static class AdMobAppIdSources
    {
        public const string GmaSettingsLocation      = "Assets > Google Mobile Ads > Settings > Android App ID (Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset)";
        public const string ConsentSettingsAssetPath = "Assets/Resources/" + ConsentSettings.ResourcesPath + ".asset";

        public readonly struct Resolution
        {
            /// <summary>The game's app ID; null when the game does not use AdMob.</summary>
            public readonly string AppId;

            /// <summary>True when the build must write the ID into the manifest (no Google Mobile Ads plugin to do it).</summary>
            public readonly bool InjectIntoManifest;

            /// <summary>Why the build must stop; null when it may go on.</summary>
            public readonly string Error;

            private Resolution(string appId, bool inject, string error)
            {
                this.AppId              = appId;
                this.InjectIntoManifest = inject;
                this.Error              = error;
            }

            public static Resolution NoAdMob                       => new(null, false, null);
            public static Resolution Use(string appId, bool inject) => new(appId, inject, null);
            public static Resolution Fail(string error)             => new(null, false, error);
        }

        /// <param name="gmaAppId">The Android app ID in the Google Mobile Ads plugin settings; null when the game has no such settings.</param>
        /// <param name="consentAppId">The ID in the game's <see cref="ConsentSettings"/>; null when the game has none.</param>
        /// <param name="adMobSdkInBuild">Whether the Android build carries the Google Mobile Ads SDK (play-services-ads).</param>
        public static Resolution Resolve(string gmaAppId, string consentAppId, bool adMobSdkInBuild)
        {
            var gma     = gmaAppId?.Trim();
            var consent = consentAppId?.Trim();

            if (gma == null && consent == null)
            {
                if (!adMobSdkInBuild) return Resolution.NoAdMob;
                return Resolution.Fail("[ThirdPartyService] This build includes the Google Mobile Ads SDK (play-services-ads) but the game declares no AdMob app ID. "
                    + $"Set it in {GmaSettingsLocation}, or, without that plugin, create a ConsentSettings asset at {ConsentSettingsAssetPath} "
                    + "(Create > ThirdParty > Consent Settings) and fill androidAdMobAppId.");
            }

            if (gma != null)
            {
                if (gma.Length == 0)
                {
                    return Resolution.Fail("[ThirdPartyService] The Google Mobile Ads plugin is in the project but its Android App ID is empty. "
                        + $"Set this game's AdMob app ID (ca-app-pub-...~...) in {GmaSettingsLocation}.");
                }
                if (!string.IsNullOrEmpty(consent) && consent != gma)
                {
                    return Resolution.Fail($"[ThirdPartyService] Two different AdMob app IDs: '{gma}' in {GmaSettingsLocation} "
                        + $"and '{consent}' in {ConsentSettingsAssetPath}. Keep the ID only in the Google Mobile Ads settings "
                        + "and delete the ConsentSettings asset (or make both equal).");
                }
                // The plugin's own build step writes it into the manifest; writing it again would duplicate it.
                return Resolution.Use(gma, false);
            }

            if (consent.Length == 0)
            {
                return Resolution.Fail($"[ThirdPartyService] {ConsentSettingsAssetPath} exists but its androidAdMobAppId is empty. "
                    + "Set this game's AdMob app ID (ca-app-pub-...~...) there, or delete the asset if the game does not use AdMob.");
            }
            return Resolution.Use(consent, true);
        }
    }
}
