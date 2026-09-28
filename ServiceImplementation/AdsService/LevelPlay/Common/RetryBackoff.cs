namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common
{
    #if LevelPlay
    using System;

    // Exponential backoff for repeated failures: 2s, 4s, 8s ... capped at 64s, so a unit that never
    // fills keeps trying about once a minute instead of spinning.
    public static class RetryBackoff
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
