namespace ThirdPartyService.ServiceImplementation.AdsService.Fsn
{
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The FSN Android library (Plugins/Android/FsnForUnity.androidlib) as Unity wires it into the
    /// generated Gradle project. Unity adds every .androidlib whatever the scripting defines (a define
    /// constraint on its importer is ignored), so <c>FsnGradleStripper</c> takes it out again in
    /// Android builds of games that do not define FSN.
    /// </summary>
    public static class FsnGradleModule
    {
        public const string Folder = "FsnForUnity.androidlib";

        /// <summary>The Gradle script without the lines that include or depend on the FSN module.</summary>
        /// <remarks>
        /// Lines end in either newline, each kept as it was: a settings template checked out with CRLF
        /// gets Unity's include lines joined with LF, and splitting on CRLF alone read that whole block
        /// as one line and dropped every other module's include with FSN's.
        /// </remarks>
        public static string Strip(string gradle)
        {
            // Alternates line, newline, line, newline ... ending in a line, possibly empty.
            var parts    = Regex.Split(gradle, "(\r\n|\n)");
            var stripped = new StringBuilder(gradle.Length);
            for (var i = 0; i < parts.Length; i += 2)
            {
                if (parts[i].Contains(":" + Folder + "'")) continue;
                stripped.Append(parts[i]);
                if (i + 1 < parts.Length) stripped.Append(parts[i + 1]);
            }
            return stripped.ToString();
        }
    }
}
