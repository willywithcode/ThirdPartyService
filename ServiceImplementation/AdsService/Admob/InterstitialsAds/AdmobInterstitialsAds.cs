namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.InterstitialsAds
{
    #if Admob
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using GoogleMobileAds.Api;
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Common;
    using UnityEngine;
    using UnityEngine.Events;

    // AdMob's interstitial behind IInterstitialAdsService. The contract the AdsService aggregator and
    // the game's AdsGate rely on: exactly one of onAdClosed / onAdFailedToShow fires for every
    // ShowInterstitial call, because AdsGate awaits that callback before it loads the next level.
    // One ad is kept loaded at a time: Initialize starts the first load, a close starts the next, and
    // a failed load retries on AdmobLoadBackoff instead of giving up for the session.
    public class AdmobInterstitialsAds : IInterstitialAdsService
    {
        private readonly AdmobSettingBlueprintService admobSettingBlueprintService;
        private readonly SignalBus                    signalBus;

        public AdmobInterstitialsAds(
            AdmobSettingBlueprintService admobSettingBlueprintService,
            SignalBus                    signalBus
        )
        {
            this.admobSettingBlueprintService = admobSettingBlueprintService;
            this.signalBus                    = signalBus;
        }

        private InterstitialAd interstitialAd;
        private UnityAction    onAdClosed;
        private UnityAction    onAdFailedToShow;
        private bool           showing;
        private bool           loading;
        private int            loadFailures;

        private readonly string AD_FLATFORM = "Admob";

        public int GetPriority() => this.admobSettingBlueprintService.GetBlueprint().priorityInterstitial;

        public void Initialize()
        {
            this.DestroyAd();
            this.loadFailures = 0;
            this.Load();
        }

        public bool IsInitialized() => this.interstitialAd != null;

        public bool IsInterstitialReady() =>
            !this.showing && this.interstitialAd != null && this.interstitialAd.CanShowAd();

        public void ShowInterstitial(string where, UnityAction onAdClosed = null, UnityAction onAdFailedToShow = null)
        {
            if (!this.IsInterstitialReady())
            {
                onAdFailedToShow?.Invoke();
                return;
            }

            this.showing          = true;
            this.onAdClosed       = onAdClosed;
            this.onAdFailedToShow = onAdFailedToShow;
            this.interstitialAd.Show();
            this.signalBus.Fire<OnInterstitialShowSignal>(new(this.AD_FLATFORM, where));
        }

        private void Load()
        {
            if (this.loading || this.interstitialAd != null) return;
            var adUnitId = this.admobSettingBlueprintService.GetBlueprint().interstitialAdUnitId;
            // Nothing configured for this format: stay silent and never report ready, so the
            // aggregator simply passes this provider over.
            if (string.IsNullOrEmpty(adUnitId)) return;

            this.loading = true;
            InterstitialAd.Load(adUnitId, new AdRequest(), (ad, error) =>
                AdmobMainThread.Run(() => this.OnLoadComplete(ad, error)));
        }

        private void OnLoadComplete(InterstitialAd ad, LoadAdError error)
        {
            this.loading = false;
            if (error != null || ad == null)
            {
                var message = error != null ? error.GetMessage() : "the load reported neither an ad nor an error";
                Debug.LogWarning($"Admob interstitial failed to load: {message}");
                this.signalBus.Fire<OnInterstitialAdLoadFailedEventSignal>(new(this.AD_FLATFORM, message));
                this.loadFailures++;
                this.RetryLoadAsync(this.loadFailures).Forget();
                return;
            }

            this.loadFailures   = 0;
            this.interstitialAd = ad;
            this.signalBus.Fire<OnInterstitialAdLoadedEventSignal>(new(this.AD_FLATFORM, ""));
            this.RegisterEventHandlers(ad);
        }

        private async UniTaskVoid RetryLoadAsync(int failures)
        {
            // Real time, so the wait still passes while a fullscreen ad has the game paused.
            await UniTask.Delay(TimeSpan.FromSeconds(AdmobLoadBackoff.DelaySeconds(failures)), DelayType.Realtime);
            this.Load();
        }

        private void RegisterEventHandlers(InterstitialAd ad)
        {
            ad.OnAdPaid += adValue => this.signalBus.Fire<OnInterstitialAdRevenuePaidEventSignal>(
                new(this.AD_FLATFORM, "", adValue.Value, adValue.CurrencyCode, this.admobSettingBlueprintService.GetBlueprint().interstitialAdUnitId));

            ad.OnAdClicked += () => this.signalBus.Fire<OnInterstitialAdClickedEventSignal>(new(this.AD_FLATFORM, ""));

            ad.OnAdFullScreenContentOpened += () => this.signalBus.Fire<OnInterstitialAdDisplayedEventSignal>(
                new(this.AD_FLATFORM, "", this.admobSettingBlueprintService.GetBlueprint().interstitialAdUnitId));

            // A shown interstitial is spent either way, so both endings drop the ad object and load
            // the next one before handing the caller back its callback.
            ad.OnAdFullScreenContentClosed += () => AdmobMainThread.Run(() =>
            {
                var callback = this.onAdClosed;
                this.EndShow();
                this.DestroyAd();
                this.signalBus.Fire<OnInterstitialAdHiddenEventSignal>(new(this.AD_FLATFORM, ""));
                this.Load();
                callback?.Invoke();
            });

            ad.OnAdFullScreenContentFailed += error => AdmobMainThread.Run(() =>
            {
                var callback = this.onAdFailedToShow;
                this.EndShow();
                this.DestroyAd();
                this.signalBus.Fire<OnInterstitialAdDisplayFailedEventSignal>(new(this.AD_FLATFORM, "", error.GetMessage()));
                this.Load();
                callback?.Invoke();
            });
        }

        private void EndShow()
        {
            this.showing          = false;
            this.onAdClosed       = null;
            this.onAdFailedToShow = null;
        }

        private void DestroyAd()
        {
            if (this.interstitialAd == null) return;
            this.interstitialAd.Destroy();
            this.interstitialAd = null;
        }
    }
    #endif
}
