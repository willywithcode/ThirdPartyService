namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.Common
{
    #if Admob
    using System;

    // Exponential backoff for repeated AdMob load failures: 2s, 4s, 8s ... capped at 64s, so an ad
    // unit that never fills keeps trying about once a minute instead of spinning. Without a retry the
    // first failed load - a cold start with no network, say - would leave AdMob unable to serve for
    // the rest of the session, which is exactly when the waterfall needs it.
    //
    // This mirrors LevelPlay/Common/RetryBackoff.cs value for value. The two cannot be shared:
    // that one lives behind #if LevelPlay, and no provider here may depend on another's define.
    // AdmobLoadBackoffTests pins them to the same numbers so the pair cannot drift apart silently.
    public static class AdmobLoadBackoff
    {
        public const float FirstDelaySeconds = 2f;
        public const float MaxDelaySeconds   = 64f;

        public static float DelaySeconds(int consecutiveFailures)
        {
            var exponent = Math.Max(0, Math.Min(consecutiveFailures - 1, 5));
            return Math.Min(MaxDelaySeconds, FirstDelaySeconds * (1 << exponent));
        }
    }
    #endif
}
