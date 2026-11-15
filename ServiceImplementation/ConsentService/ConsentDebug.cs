namespace ThirdPartyService.ServiceImplementation.ConsentService
{
    using System;
    using System.Collections.Generic;

    /// <summary>UMP's debug geographies (ConsentDebugSettings.DebugGeography).</summary>
    public enum ConsentDebugGeography
    {
        Disabled          = 0,
        Eea               = 1,
        NotEea            = 2,
        RegulatedUsState  = 3,
        Other             = 4,
    }

    /// <summary>
    /// Makes chosen test devices see the consent form as if they were in a region, so QA can check
    /// the GDPR or US-states flow without a VPN. UMP applies the geography only to the devices whose
    /// hashed id is listed; every other device keeps its real region. A device's id is in logcat,
    /// tag UserMessagingPlatform: "Use new ConsentDebugSettings.Builder().addTestDeviceHashedId(...)".
    ///
    /// A game configures it before GatherConsentAsync, and should do so only in its test builds.
    /// </summary>
    public static class ConsentDebug
    {
        public static ConsentDebugGeography Geography { get; private set; } = ConsentDebugGeography.Disabled;

        public static IReadOnlyList<string> TestDeviceHashedIds { get; private set; } = Array.Empty<string>();

        /// <summary>Whether a debug geography is set for at least one test device.</summary>
        public static bool IsActive => Geography != ConsentDebugGeography.Disabled && TestDeviceHashedIds.Count > 0;

        public static void Configure(ConsentDebugGeography geography, IReadOnlyList<string> testDeviceHashedIds)
        {
            Geography           = geography;
            TestDeviceHashedIds = testDeviceHashedIds ?? Array.Empty<string>();
        }
    }
}
