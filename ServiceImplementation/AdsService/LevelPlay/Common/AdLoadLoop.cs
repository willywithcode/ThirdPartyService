namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common
{
    #if LevelPlay
    using System;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;

    // Keeps one ad unit loading: never two loads at once, and after a failed load the next attempt
    // waits out RetryBackoff instead of firing straight away. Close and display failure call Load()
    // themselves; that is a user event, not a loop.
    internal sealed class AdLoadLoop : IDisposable
    {
        private readonly IAdUnit       unit;
        private readonly Func<bool>    isReady;
        private readonly IAdsScheduler scheduler;
        private readonly AdEventLog    log;
        private readonly AdFormat      format;

        private bool        loading;
        private int         failures;
        private IDisposable pendingRetry;
        private bool        disposed;

        public AdLoadLoop(IAdUnit unit, Func<bool> isReady, IAdsScheduler scheduler, AdEventLog log, AdFormat format)
        {
            this.unit      = unit;
            this.isReady   = isReady;
            this.scheduler = scheduler;
            this.log       = log;
            this.format    = format;

            this.unit.Loaded     += this.OnLoaded;
            this.unit.LoadFailed += this.OnLoadFailed;
        }

        public bool IsLoading => this.loading;

        public void Load()
        {
            if (this.disposed || this.loading || this.isReady()) return;
            this.CancelRetry();
            this.loading = true;
            this.log.Record(this.format, AdEventKind.LoadRequested);
            this.unit.Load();
        }

        private void OnLoaded()
        {
            if (this.disposed) return;
            this.loading  = false;
            this.failures = 0;
            this.CancelRetry();
            this.log.Record(this.format, AdEventKind.Loaded);
        }

        private void OnLoadFailed(string error)
        {
            if (this.disposed) return;
            this.loading = false;
            this.failures++;
            var delay = RetryBackoff.DelaySeconds(this.failures);
            this.log.Record(this.format, AdEventKind.LoadFailed, error);
            this.log.Record(this.format, AdEventKind.RetryScheduled, $"{delay}s");
            this.CancelRetry();
            this.pendingRetry = this.scheduler.Schedule(delay, this.OnRetryDue);
        }

        private void OnRetryDue()
        {
            if (this.disposed) return;
            this.pendingRetry = null;
            this.Load();
        }

        private void CancelRetry()
        {
            this.pendingRetry?.Dispose();
            this.pendingRetry = null;
        }

        public void Dispose()
        {
            this.disposed = true;
            this.CancelRetry();
            this.unit.Loaded     -= this.OnLoaded;
            this.unit.LoadFailed -= this.OnLoadFailed;
        }
    }
    #endif
}
