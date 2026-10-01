namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.RewardedAds
{
    #if Admob
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using GoogleMobileAds.Api;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Common;
    using UnityEngine;
    using UnityEngine.Events;

    // AdMob's rewarded ad behind IRewardedAdsService. Only AdMob's reward callback earns the reward,
    // and every attempt completes exactly once - true when the reward arrived, false when the player
    // closed early or the ad could not be shown. Completing is what unlocks the popup that asked, so
    // an attempt that never completed would leave its buttons dead for the rest of the session.
    //
    // The reward and the close arrive in either order, so an attempt finishes when both have happened
    // (true), or when the ad closed and no reward followed within RewardGraceSeconds (false). This is
    // the same shape as LevelPlayRewardedAds, deliberately: the two must not differ in what pays.
    public class AdmobRewardedAds : IRewardedAdsService
    {
        public const float RewardGraceSeconds = 3f;

        private readonly AdmobSettingBlueprintService admobSettingBlueprintService;
        private readonly SignalBus                    signalBus;

        public AdmobRewardedAds(
            AdmobSettingBlueprintService admobSettingBlueprintService,
            SignalBus                    signalBus
        )
        {
            this.admobSettingBlueprintService = admobSettingBlueprintService;
            this.signalBus                    = signalBus;
        }

        private sealed class Attempt
        {
            public UnityAction<bool> OnComplete;
            public bool              Rewarded;
            public bool              Closed;
        }

        private RewardedAd rewardedAd;
        private Attempt    attempt;
        private bool       loading;
        private int        loadFailures;

        private readonly string AD_FLATFORM = "Admob";

        public int GetPriority() => this.admobSettingBlueprintService.GetBlueprint().priorityRewarded;

        public void Initialize()
        {
            this.DestroyAd();
            this.loadFailures = 0;
            this.Load();
        }

        public bool IsAdReady() =>
            this.attempt == null && this.rewardedAd != null && this.rewardedAd.CanShowAd();

        public void ShowAd(UnityAction<bool> onAdComplete, string where)
        {
            if (!this.IsAdReady())
            {
                onAdComplete?.Invoke(false);
                return;
            }

            this.attempt = new Attempt { OnComplete = onAdComplete };
            this.rewardedAd.Show(reward => AdmobMainThread.Run(() => this.OnRewarded(reward)));
            this.signalBus.Fire<OnRewardedShowSignal>(new(this.AD_FLATFORM, where));
        }

        private void Load()
        {
            if (this.loading || this.rewardedAd != null) return;
            var adUnitId = this.admobSettingBlueprintService.GetBlueprint().rewardedAdUnitId;
            // Nothing configured for this format: stay silent and never report ready, so the
            // aggregator simply passes this provider over.
            if (string.IsNullOrEmpty(adUnitId)) return;

            this.loading = true;
            RewardedAd.Load(adUnitId, new AdRequest(), (ad, error) =>
                AdmobMainThread.Run(() => this.OnLoadComplete(ad, error)));
        }

        private void OnLoadComplete(RewardedAd ad, LoadAdError error)
        {
            this.loading = false;
            if (error != null || ad == null)
            {
                var message = error != null ? error.GetMessage() : "the load reported neither an ad nor an error";
                Debug.LogWarning($"Admob rewarded failed to load: {message}");
                this.signalBus.Fire<OnRewardedAdLoadFailedEventSignal>(new(this.AD_FLATFORM, message));
                this.loadFailures++;
                this.RetryLoadAsync(this.loadFailures).Forget();
                return;
            }

            this.loadFailures = 0;
            this.rewardedAd   = ad;
            this.signalBus.Fire<OnRewardedAdLoadedEventSignal>(new(this.AD_FLATFORM, ""));
            this.RegisterEventHandlers(ad);
        }

        private async UniTaskVoid RetryLoadAsync(int failures)
        {
            // Real time, so the wait still passes while a fullscreen ad has the game paused.
            await UniTask.Delay(TimeSpan.FromSeconds(AdmobLoadBackoff.DelaySeconds(failures)), DelayType.Realtime);
            this.Load();
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            ad.OnAdPaid += adValue => this.signalBus.Fire<OnRewardedAdRevenuePaidEventSignal>(
                new(this.AD_FLATFORM, "", adValue.Value, adValue.CurrencyCode));

            ad.OnAdClicked += () => this.signalBus.Fire<OnRewardedAdClickedEventSignal>(new(this.AD_FLATFORM, ""));

            ad.OnAdFullScreenContentOpened += () => this.signalBus.Fire<OnRewardedAdDisplayedEventSignal>(new(this.AD_FLATFORM, ""));

            ad.OnAdFullScreenContentClosed += () => AdmobMainThread.Run(this.OnClosed);

            ad.OnAdFullScreenContentFailed += error => AdmobMainThread.Run(() =>
            {
                this.signalBus.Fire<OnRewardedAdDisplayFailedEventSignal>(new(this.AD_FLATFORM, "", error.GetMessage()));
                this.Complete(false);
            });
        }

        private void OnRewarded(Reward reward)
        {
            this.signalBus.Fire<OnRewardedAdReceivedRewardEventSignal>(
                new(this.AD_FLATFORM, "", reward == null ? "reward" : reward.Type, reward?.Amount ?? 0));
            // Too late to pay: the grace window closed and the attempt already completed false.
            if (this.attempt == null) return;

            this.attempt.Rewarded = true;
            if (this.attempt.Closed) this.Complete(true);
        }

        private void OnClosed()
        {
            this.signalBus.Fire<OnRewardedAdHiddenEventSignal>(new(this.AD_FLATFORM, ""));
            // The spent ad is replaced only once the attempt has been decided, never here: destroying
            // it is what would silence a reward callback that arrives after the close, and that
            // callback is the only thing that pays.
            if (this.attempt == null || this.attempt.Closed)
            {
                this.ReplaceSpentAd();
                return;
            }
            this.attempt.Closed = true;
            if (this.attempt.Rewarded)
            {
                this.Complete(true);
                return;
            }
            this.CompleteUnrewardedAfterGraceAsync(this.attempt).Forget();
        }

        private async UniTaskVoid CompleteUnrewardedAfterGraceAsync(Attempt closedAttempt)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(RewardGraceSeconds), DelayType.Realtime);
            if (this.attempt == closedAttempt) this.Complete(false);
        }

        private void Complete(bool rewarded)
        {
            var finished = this.attempt;
            if (finished == null) return;
            this.attempt = null;
            this.ReplaceSpentAd();
            finished.OnComplete?.Invoke(rewarded);
        }

        // A rewarded ad cannot be shown twice, so the attempt that just ended takes the ad with it and
        // the next load starts at once - the waterfall needs this network ready again as soon as it can be.
        private void ReplaceSpentAd()
        {
            this.DestroyAd();
            this.Load();
        }

        private void DestroyAd()
        {
            if (this.rewardedAd == null) return;
            this.rewardedAd.Destroy();
            this.rewardedAd = null;
        }
    }
    #endif
}
