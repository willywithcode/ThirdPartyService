namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common
{
    #if LevelPlay
    using System;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;

    // Owns the one SDK initialization per app lifetime. Ad objects may only be created after it
    // succeeds, so the wrappers wait on Initialized rather than calling the SDK themselves.
    public class LevelPlaySdkSession : IDisposable
    {
        private readonly IAdsSdk                    sdk;
        private readonly ILevelPlaySettingsProvider settings;
        private readonly IAdsScheduler              scheduler;
        private readonly AdEventLog                 log;

        private bool        started;
        private bool        initializing;
        private int         failures;
        private IDisposable pendingRetry;
        private bool        disposed;

        public LevelPlaySdkSession(IAdsSdk sdk, ILevelPlaySettingsProvider settings, IAdsScheduler scheduler, AdEventLog log)
        {
            this.sdk       = sdk;
            this.settings  = settings;
            this.scheduler = scheduler;
            this.log       = log;
        }

        public event Action Initialized;

        public bool IsInitialized { get; private set; }
        public bool IsStarted     => this.started;

        // Safe to call repeatedly: only the first call initializes; failures retry on their own.
        public void Start()
        {
            if (this.started || this.disposed) return;
            this.started = true;
            this.RequestInit();
        }

        private void RequestInit()
        {
            this.pendingRetry = null;
            if (this.IsInitialized || this.initializing || this.disposed) return;

            var appKey = this.settings.Current?.appKey;
            if (string.IsNullOrEmpty(appKey))
            {
                this.log.Record(AdFormat.Sdk, AdEventKind.InitFailed, "no app key in LevelPlaySettings for this platform");
                return;
            }

            this.initializing = true;
            this.log.Record(AdFormat.Sdk, AdEventKind.InitRequested);
            this.sdk.Init(appKey, this.OnInitSucceeded, this.OnInitFailed);
        }

        private void OnInitSucceeded()
        {
            if (this.disposed || this.IsInitialized) return;
            this.initializing  = false;
            this.failures      = 0;
            this.IsInitialized = true;
            this.log.Record(AdFormat.Sdk, AdEventKind.InitSucceeded);
            this.Initialized?.Invoke();
        }

        private void OnInitFailed(string error)
        {
            if (this.disposed || this.IsInitialized) return;
            this.initializing = false;
            this.failures++;
            var delay = RetryBackoff.DelaySeconds(this.failures);
            this.log.Record(AdFormat.Sdk, AdEventKind.InitFailed, error);
            this.log.Record(AdFormat.Sdk, AdEventKind.RetryScheduled, $"{delay}s");
            this.pendingRetry = this.scheduler.Schedule(delay, this.RequestInit);
        }

        public void Dispose()
        {
            this.disposed = true;
            this.pendingRetry?.Dispose();
            this.pendingRetry = null;
            this.Initialized  = null;
        }
    }
    #endif
}
