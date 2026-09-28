namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.InterstitialsAds
{
    #if LevelPlay
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;
    using UnityEngine.Events;

    // Callbacks follow IInterstitialAdsService's own parameter names: onAdClosed when the ad closes,
    // onAdFailedToShow when it cannot show. The AdsService aggregator maps them to its callers'
    // onShowSuccess and onShowFail.
    public class LevelPlayInterstitialAds : LevelPlayAdWrapper, IInterstitialAdsService
    {
        private IFullscreenAdUnit unit;
        private AdLoadLoop        loop;
        private bool              showing;
        private UnityAction       pendingClosed;
        private UnityAction       pendingFailed;

        public LevelPlayInterstitialAds(LevelPlaySdkSession session, IAdsSdk sdk, ILevelPlaySettingsProvider settings, IAdsScheduler scheduler, AdEventLog log)
            : base(session, sdk, settings, scheduler, log) { }

        protected override AdFormat Format  => AdFormat.Interstitial;
        protected override bool     HasUnit => this.unit != null;

        public bool IsInitialized() => this.unit != null;

        public bool IsInterstitialReady() => this.unit != null && !this.showing && this.unit.IsReady;

        public void ShowInterstitial(string where, UnityAction onAdClosed = null, UnityAction onAdFailedToShow = null)
        {
            if (!this.IsInterstitialReady())
            {
                this.log.Record(AdFormat.Interstitial, AdEventKind.ShowFailed, this.showing ? "another interstitial is showing" : "not ready");
                onAdFailedToShow?.Invoke();
                return;
            }

            this.showing       = true;
            this.pendingClosed = onAdClosed;
            this.pendingFailed = onAdFailedToShow;
            this.log.Record(AdFormat.Interstitial, AdEventKind.ShowRequested, where);
            this.unit.Show(where);
        }

        protected override void CreateUnit(LevelPlayPlatformSettings current)
        {
            if (string.IsNullOrEmpty(current.interstitialAdUnitId))
            {
                this.log.Record(AdFormat.Interstitial, AdEventKind.LoadFailed, "no interstitial ad unit id in LevelPlaySettings");
                return;
            }

            this.unit               =  this.sdk.CreateInterstitial(current.interstitialAdUnitId);
            this.loop               =  new AdLoadLoop(this.unit, () => this.unit.IsReady, this.scheduler, this.log, AdFormat.Interstitial);
            this.unit.Displayed     += this.OnDisplayed;
            this.unit.DisplayFailed += this.OnDisplayFailed;
            this.unit.Clicked       += this.OnClicked;
            this.unit.Closed        += this.OnClosed;
            this.loop.Load();
        }

        protected override void LoadNow() => this.loop.Load();

        private void OnDisplayed() => this.log.Record(AdFormat.Interstitial, AdEventKind.Shown);

        private void OnClicked() => this.log.Record(AdFormat.Interstitial, AdEventKind.Clicked);

        private void OnDisplayFailed(string error)
        {
            this.log.Record(AdFormat.Interstitial, AdEventKind.ShowFailed, error);
            var callback = this.pendingFailed;
            this.EndShow();
            this.loop.Load();
            callback?.Invoke();
        }

        private void OnClosed()
        {
            this.log.Record(AdFormat.Interstitial, AdEventKind.Closed);
            var callback = this.pendingClosed;
            this.EndShow();
            this.loop.Load();
            callback?.Invoke();
        }

        private void EndShow()
        {
            this.showing       = false;
            this.pendingClosed = null;
            this.pendingFailed = null;
        }

        protected override void DestroyUnit()
        {
            if (this.unit == null) return;
            this.loop.Dispose();
            this.unit.Displayed     -= this.OnDisplayed;
            this.unit.DisplayFailed -= this.OnDisplayFailed;
            this.unit.Clicked       -= this.OnClicked;
            this.unit.Closed        -= this.OnClosed;
            this.unit.Dispose();
            this.unit = null;
            this.loop = null;
            this.EndShow();
        }
    }
    #endif
}
