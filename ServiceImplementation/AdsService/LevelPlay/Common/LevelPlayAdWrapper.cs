namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common
{
    #if LevelPlay
    using System;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;

    // What every LevelPlay wrapper shares: the ThirdPartyService interfaces have no Load method, so
    // Initialize() is the request to start loading. The SDK may not be initialized yet at that
    // point, and ad objects must not exist before it is, so the unit is created on
    // LevelPlaySdkSession.Initialized when needed.
    public abstract class LevelPlayAdWrapper : IDisposable
    {
        protected readonly LevelPlaySdkSession        session;
        protected readonly IAdsSdk                    sdk;
        protected readonly ILevelPlaySettingsProvider settings;
        protected readonly IAdsScheduler              scheduler;
        protected readonly AdEventLog                 log;

        private bool initializeRequested;
        private bool waitingForSdk;
        private bool disposed;

        protected LevelPlayAdWrapper(LevelPlaySdkSession session, IAdsSdk sdk, ILevelPlaySettingsProvider settings, IAdsScheduler scheduler, AdEventLog log)
        {
            this.session   = session;
            this.sdk       = sdk;
            this.settings  = settings;
            this.scheduler = scheduler;
            this.log       = log;
        }

        protected abstract AdFormat Format   { get; }
        protected abstract bool     HasUnit  { get; }

        public int GetPriority() => LevelPlayAds.Priority;

        public void Initialize()
        {
            if (this.disposed) return;
            this.initializeRequested = true;
            if (this.session.IsInitialized)
            {
                this.EnsureUnit();
                return;
            }
            if (this.waitingForSdk) return;
            this.waitingForSdk           =  true;
            this.session.Initialized += this.OnSdkInitialized;
        }

        // The development panel's explicit load control. Before Initialize() it does what
        // Initialize() does; afterwards it asks for a load now, skipping any pending retry wait.
        public void Load()
        {
            if (!this.HasUnit)
            {
                this.Initialize();
                return;
            }
            this.LoadNow();
        }

        private void OnSdkInitialized()
        {
            this.session.Initialized -= this.OnSdkInitialized;
            this.waitingForSdk       =  false;
            if (this.initializeRequested && !this.disposed) this.EnsureUnit();
        }

        private void EnsureUnit()
        {
            if (this.HasUnit) return;
            var current = this.settings.Current;
            if (current == null)
            {
                this.log.Record(this.Format, AdEventKind.LoadFailed, "LevelPlaySettings not loaded");
                return;
            }
            this.CreateUnit(current);
        }

        // Creates the SDK unit from the settings and starts its first load.
        protected abstract void CreateUnit(LevelPlayPlatformSettings current);

        protected abstract void LoadNow();

        protected abstract void DestroyUnit();

        public void Dispose()
        {
            if (this.disposed) return;
            this.disposed = true;
            if (this.waitingForSdk) this.session.Initialized -= this.OnSdkInitialized;
            this.DestroyUnit();
        }
    }
    #endif
}
