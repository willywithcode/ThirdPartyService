namespace ThirdPartyService.ServiceImplementation.AdsService.Fsn
{
    using System;
    using System.Linq;

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
        public static string Strip(string gradle)
        {
            var newline = gradle.Contains("\r\n") ? "\r\n" : "\n";
            var lines   = gradle.Split(new[] { newline }, StringSplitOptions.None);
            return string.Join(newline, lines.Where(line => !line.Contains(":" + Folder + "'")));
        }
    }
}
