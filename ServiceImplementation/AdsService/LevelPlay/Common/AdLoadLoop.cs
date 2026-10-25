namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common
{
    #if LevelPlay
    using System;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;

    // Keeps one ad unit loading: never two loads at once, and after a failed load the next attempt
    // waits out RetryBackoff instead of firing straight away. Close and display failure call Load()
    // themselves; that is a user event, not a loop.
    //
    // A load can also never answer: on a device the LevelPlay banner went silent after its second
    // no-fill and stayed silent for the rest of the session, and since a new load waits for the
    // answer, nothing ever asked again. A loop given a timeout stops waiting after that long and
    // hands the unit to onTimeout, which replaces it - the same object ignores further loads.
    internal sealed class AdLoadLoop : IDisposable
    {
        private readonly IAdUnit       unit;
        private readonly Func<bool>    isReady;
        private readonly IAdsScheduler scheduler;
        private readonly AdEventLog    log;
        private readonly AdFormat      format;
        private readonly float         timeoutSeconds;
        private readonly Action        onTimeout;

        private bool        loading;
        private int         failures;
        private IDisposable pendingRetry;
        private IDisposable pendingTimeout;
        private bool        disposed;

        public AdLoadLoop(IAdUnit unit, Func<bool> isReady, IAdsScheduler scheduler, AdEventLog log, AdFormat format,
            float timeoutSeconds = 0f, Action onTimeout = null)
        {
            this.unit           = unit;
            this.isReady        = isReady;
            this.scheduler      = scheduler;
            this.log            = log;
            this.format         = format;
            this.timeoutSeconds = timeoutSeconds;
            this.onTimeout      = onTimeout;

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
            if (this.onTimeout != null && this.timeoutSeconds > 0f)
                this.pendingTimeout = this.scheduler.Schedule(this.timeoutSeconds, this.OnTimeoutDue);
            this.unit.Load();
        }

        private void OnTimeoutDue()
        {
            if (this.disposed || !this.loading) return;
            this.pendingTimeout = null;
            this.loading        = false;
            this.log.Record(this.format, AdEventKind.LoadTimedOut, $"no answer after {this.timeoutSeconds}s");
            this.onTimeout();
        }

        private void OnLoaded()
        {
            if (this.disposed) return;
            this.loading  = false;
            this.failures = 0;
            this.CancelRetry();
            this.CancelTimeout();
            this.log.Record(this.format, AdEventKind.Loaded);
        }

        private void OnLoadFailed(string error)
        {
            if (this.disposed) return;
            this.loading = false;
            this.CancelTimeout();
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

        private void CancelTimeout()
        {
            this.pendingTimeout?.Dispose();
            this.pendingTimeout = null;
        }

        public void Dispose()
        {
            this.disposed = true;
            this.CancelRetry();
            this.CancelTimeout();
            this.unit.Loaded     -= this.OnLoaded;
            this.unit.LoadFailed -= this.OnLoadFailed;
        }
    }
    #endif
}
