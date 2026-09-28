namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.RewardedAds
{
    #if LevelPlay
    using System;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;
    using UnityEngine.Events;

    // Only LevelPlay's reward callback earns a reward. OnAdRewarded and OnAdClosed arrive in either
    // order, so an attempt completes when both have happened (true), or when the ad closed and no
    // reward followed within RewardGraceSeconds (false). A show that fails completes false at once.
    // Each attempt completes exactly once.
    public class LevelPlayRewardedAds : LevelPlayAdWrapper, IRewardedAdsService
    {
        public const float RewardGraceSeconds = 3f;

        private sealed class Attempt
        {
            public UnityAction<bool> OnComplete;
            public bool              Rewarded;
            public bool              Closed;
            public IDisposable       Grace;
        }

        private IRewardedAdUnit unit;
        private AdLoadLoop      loop;
        private Attempt         attempt;

        public LevelPlayRewardedAds(LevelPlaySdkSession session, IAdsSdk sdk, ILevelPlaySettingsProvider settings, IAdsScheduler scheduler, AdEventLog log)
            : base(session, sdk, settings, scheduler, log) { }

        protected override AdFormat Format  => AdFormat.Rewarded;
        protected override bool     HasUnit => this.unit != null;

        public bool IsAdReady() => this.unit != null && this.attempt == null && this.unit.IsReady;

        public void ShowAd(UnityAction<bool> onAdComplete, string where)
        {
            if (!this.IsAdReady())
            {
                this.log.Record(AdFormat.Rewarded, AdEventKind.ShowFailed, this.attempt != null ? "another rewarded ad is in progress" : "not ready");
                onAdComplete?.Invoke(false);
                return;
            }

            this.attempt = new Attempt { OnComplete = onAdComplete };
            this.log.Record(AdFormat.Rewarded, AdEventKind.ShowRequested, where);
            this.unit.Show(where);
        }

        protected override void CreateUnit(LevelPlayPlatformSettings current)
        {
            if (string.IsNullOrEmpty(current.rewardedAdUnitId))
            {
                this.log.Record(AdFormat.Rewarded, AdEventKind.LoadFailed, "no rewarded ad unit id in LevelPlaySettings");
                return;
            }

            this.unit               =  this.sdk.CreateRewarded(current.rewardedAdUnitId);
            this.loop               =  new AdLoadLoop(this.unit, () => this.unit.IsReady, this.scheduler, this.log, AdFormat.Rewarded);
            this.unit.Displayed     += this.OnDisplayed;
            this.unit.DisplayFailed += this.OnDisplayFailed;
            this.unit.Clicked       += this.OnClicked;
            this.unit.Closed        += this.OnClosed;
            this.unit.Rewarded      += this.OnRewarded;
            this.loop.Load();
        }

        protected override void LoadNow() => this.loop.Load();

        private void OnDisplayed() => this.log.Record(AdFormat.Rewarded, AdEventKind.Shown);

        private void OnClicked() => this.log.Record(AdFormat.Rewarded, AdEventKind.Clicked);

        private void OnRewarded(string reward)
        {
            if (this.attempt == null)
            {
                // Too late: the grace window closed and the attempt already completed false.
                this.log.Record(AdFormat.Rewarded, AdEventKind.Rewarded, $"{reward} (ignored: no attempt waiting for it)");
                return;
            }

            this.log.Record(AdFormat.Rewarded, AdEventKind.Rewarded, reward);
            this.attempt.Rewarded = true;
            if (this.attempt.Closed) this.Complete(true);
        }

        private void OnClosed()
        {
            this.log.Record(AdFormat.Rewarded, AdEventKind.Closed);
            this.loop.Load();
            if (this.attempt == null || this.attempt.Closed) return;

            this.attempt.Closed = true;
            if (this.attempt.Rewarded)
            {
                this.Complete(true);
                return;
            }
            var closedAttempt = this.attempt;
            closedAttempt.Grace = this.scheduler.Schedule(RewardGraceSeconds, () =>
            {
                if (this.attempt == closedAttempt) this.Complete(false);
            });
        }

        private void OnDisplayFailed(string error)
        {
            this.log.Record(AdFormat.Rewarded, AdEventKind.ShowFailed, error);
            this.loop.Load();
            this.Complete(false);
        }

        private void Complete(bool rewarded)
        {
            var finished = this.attempt;
            if (finished == null) return;
            this.attempt = null;
            finished.Grace?.Dispose();
            if (!rewarded) this.log.Record(AdFormat.Rewarded, AdEventKind.NotRewarded);
            finished.OnComplete?.Invoke(rewarded);
        }

        protected override void DestroyUnit()
        {
            if (this.unit == null) return;
            this.loop.Dispose();
            this.unit.Displayed     -= this.OnDisplayed;
            this.unit.DisplayFailed -= this.OnDisplayFailed;
            this.unit.Clicked       -= this.OnClicked;
            this.unit.Closed        -= this.OnClosed;
            this.unit.Rewarded      -= this.OnRewarded;
            this.unit.Dispose();
            this.unit = null;
            this.loop = null;
            this.attempt?.Grace?.Dispose();
            this.attempt = null;
        }
    }
    #endif
}
