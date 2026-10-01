namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Banner
{
    #if LevelPlay
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;

    // One LevelPlay banner-API ad view, used by both the banner and the MREC wrapper. LevelPlay fixes
    // a banner's position when the ad object is created, so showing at another position replaces
    // the object. The unit never shows on load; Show() does, as soon as the ad has loaded. A show
    // requested before the SDK is ready is remembered, position included, and applied once the ad
    // object exists: the AdsService aggregator picks this wrapper without a readiness check.
    // LevelPlay refreshes a loaded banner by itself, so retries only run until the first load succeeds.
    public abstract class LevelPlayBannerSlot : LevelPlayAdWrapper
    {
        public const BannerPosition PreloadPosition = BannerPosition.BottomCenter;

        private readonly BannerAdSize size;

        private IBannerAdUnit unit;
        private AdLoadLoop    loop;
        private bool           loaded;
        private bool           wantShown;
        private bool           visible;
        private bool           announcedVisible;
        private BannerPosition requestedPosition = PreloadPosition;

        protected LevelPlayBannerSlot(BannerAdSize size, LevelPlaySdkSession session, IAdsSdk sdk, ILevelPlaySettingsProvider settings, IAdsScheduler scheduler, AdEventLog log)
            : base(session, sdk, settings, scheduler, log)
        {
            this.size = size;
        }

        protected override bool HasUnit => this.unit != null;

        protected abstract string AdUnitId(LevelPlayPlatformSettings current);

        public bool IsInitialized() => this.unit != null;

        public bool IsShown() => this.visible;

        protected float ShownHeightPixels => this.visible ? this.unit.HeightPixels : 0f;

        // Told when the ad really comes onto the screen and when it leaves, once per change: LevelPlay
        // can raise Displayed again on every refresh, and a layout that moves on each one would jitter.
        protected virtual void OnVisibilityChanged(bool visible, float heightPixels) { }

        private void AnnounceVisible(bool visible)
        {
            if (visible == this.announcedVisible) return;
            this.announcedVisible = visible;
            this.OnVisibilityChanged(visible, visible && this.unit != null ? this.unit.HeightPixels : 0f);
        }

        protected void ShowAt(BannerPosition position)
        {
            this.wantShown         = true;
            this.requestedPosition = position;
            if (this.unit == null)
            {
                this.log.Record(this.Format, AdEventKind.ShowRequested, $"{position}, shows once the SDK is ready and the ad has loaded");
                this.Initialize();
                return;
            }

            if (this.unit.Position != position)
            {
                var current = this.settings.Current;
                this.DestroyUnit();
                this.wantShown = true;
                this.Create(current, position);
                return;
            }

            if (this.loaded) this.ShowLoaded();
            else this.log.Record(this.Format, AdEventKind.ShowRequested, $"{position}, shows once loaded");
        }

        protected void HideSlot()
        {
            this.wantShown = false;
            this.visible   = false;
            this.AnnounceVisible(false);
            if (this.unit == null) return;
            this.unit.Hide();
            this.log.Record(this.Format, AdEventKind.Hidden);
        }

        // Development control: drops the ad object so the next Load starts from a fresh one.
        public void Destroy()
        {
            if (this.unit == null) return;
            this.DestroyUnit();
            this.log.Record(this.Format, AdEventKind.Destroyed);
        }

        protected override void CreateUnit(LevelPlayPlatformSettings current) => this.Create(current, this.requestedPosition);

        private void Create(LevelPlayPlatformSettings current, BannerPosition position)
        {
            var adUnitId = current == null ? null : this.AdUnitId(current);
            if (string.IsNullOrEmpty(adUnitId))
            {
                this.log.Record(this.Format, AdEventKind.LoadFailed, "no ad unit id in LevelPlaySettings");
                return;
            }

            this.unit                 =  this.sdk.CreateBanner(adUnitId, this.size, position);
            this.loop                 =  new AdLoadLoop(this.unit, () => this.loaded, this.scheduler, this.log, this.Format);
            this.unit.Loaded          += this.OnLoaded;
            this.unit.Displayed       += this.OnDisplayed;
            this.unit.DisplayFailed   += this.OnDisplayFailed;
            this.unit.Clicked         += this.OnClicked;
            this.unit.Expanded        += this.OnExpanded;
            this.unit.Collapsed       += this.OnCollapsed;
            this.unit.LeftApplication += this.OnLeftApplication;
            this.loop.Load();
        }

        protected override void LoadNow() => this.loop.Load();

        private void ShowLoaded()
        {
            this.visible = true;
            this.log.Record(this.Format, AdEventKind.ShowRequested, this.unit.Position.ToString());
            this.unit.Show();
        }

        private void OnLoaded()
        {
            this.loaded = true;
            if (this.wantShown && !this.visible) this.ShowLoaded();
        }

        private void OnDisplayed()
        {
            this.log.Record(this.Format, AdEventKind.Shown);
            if (this.visible) this.AnnounceVisible(true);
        }

        private void OnDisplayFailed(string error)
        {
            this.visible = false;
            this.AnnounceVisible(false);
            this.log.Record(this.Format, AdEventKind.ShowFailed, error);
        }

        private void OnClicked()         => this.log.Record(this.Format, AdEventKind.Clicked);
        private void OnExpanded()        => this.log.Record(this.Format, AdEventKind.Expanded);
        private void OnCollapsed()       => this.log.Record(this.Format, AdEventKind.Collapsed);
        private void OnLeftApplication() => this.log.Record(this.Format, AdEventKind.LeftApplication);

        protected override void DestroyUnit()
        {
            if (this.unit == null) return;
            this.AnnounceVisible(false);
            this.loop.Dispose();
            this.unit.Loaded          -= this.OnLoaded;
            this.unit.Displayed       -= this.OnDisplayed;
            this.unit.DisplayFailed   -= this.OnDisplayFailed;
            this.unit.Clicked         -= this.OnClicked;
            this.unit.Expanded        -= this.OnExpanded;
            this.unit.Collapsed       -= this.OnCollapsed;
            this.unit.LeftApplication -= this.OnLeftApplication;
            this.unit.Dispose();
            this.unit      = null;
            this.loop      = null;
            this.loaded    = false;
            this.wantShown = false;
            this.visible   = false;
        }
    }
    #endif
}
