namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common
{
    #if LevelPlay
    public static class LevelPlayAds
    {
        // Above the ThirdPartyService Dummies (1), so the AdsService aggregator ranks LevelPlay first.
        public const int Priority = 10;
    }
    #endif
}
