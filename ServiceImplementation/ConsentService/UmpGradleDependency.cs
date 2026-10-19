namespace ThirdPartyService.ServiceImplementation.ConsentService
{
    /// <summary>
    /// The Google User Messaging Platform (UMP) SDK the consent flow needs. It goes into an Android
    /// build only when the game defines <c>UMP</c>: <c>UmpGradlePostprocessor</c> adds it to the
    /// generated Unity library's build.gradle. Version from Google's Android setup guide:
    /// https://developers.google.com/admob/android/privacy
    /// </summary>
    public static class UmpGradleDependency
    {
        public const string Artifact    = "com.google.android.ump:user-messaging-platform";
        public const string Coordinates = Artifact + ":4.0.0";

        /// <summary>
        /// The Gradle script with the UMP dependency in its first <c>dependencies {</c> block. Returns
        /// it unchanged when it already depends on UMP, and null when it has no dependencies block.
        /// </summary>
        public static string AddTo(string gradle)
        {
            if (gradle.Contains(Artifact)) return gradle;
            const string block = "dependencies {";
            var start = gradle.IndexOf(block, System.StringComparison.Ordinal);
            if (start < 0) return null;
            var insertAt = start + block.Length;
            return gradle.Insert(insertAt, "\n    implementation '" + Coordinates + "' // ThirdPartyService consent, define UMP");
        }
    }
}
