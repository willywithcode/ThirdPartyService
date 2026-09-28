namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk
{
    #if LevelPlay
    using System;

    // Delayed work for load retries and the reward grace window. Disposing the handle cancels the
    // action if it has not run yet.
    public interface IAdsScheduler
    {
        IDisposable Schedule(float delaySeconds, Action action);
    }
    #endif
}
