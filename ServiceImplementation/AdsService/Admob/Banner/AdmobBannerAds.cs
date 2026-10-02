namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.Banner
{
    #if Admob
    using System;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using GoogleMobileAds.Api;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Common;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using UnityEngine;

    // AdMob's adaptive bottom banner behind IBannerAdsService. The AdsService aggregator shows it only
    // once it reports an ad (IBannerLoadState), ahead of or behind LevelPlay's banner by priority.
    // The view is preloaded hidden - a BannerView shows itself on load otherwise - and a failed first
    // load retries on AdmobLoadBackoff; after one success AdMob refreshes the ad by itself and keeps
    // the last one on screen when a refresh fails. Like LevelPlay's banner it reports when it really
    // comes onto the screen and leaves it, with its height, so the game's layout makes room for it.
    public class AdmobBannerAds : IBannerAdsService, IBannerLoadState
    {
        private readonly AdmobSettingBlueprintService admobSettingBlueprintService;
        private readonly SignalBus                    signalBus;

        public AdmobBannerAds(
            AdmobSettingBlueprintService admobSettingBlueprintService,
            SignalBus                    signalBus
        )
        {
            this.admobSettingBlueprintService = admobSettingBlueprintService;
            this.signalBus                    = signalBus;
        }

        private BannerView bannerView;
        private bool       loaded;
        private bool       isShown;
        private bool       announcedVisible;
        private int        loadFailures;

        private readonly string AD_FLATFORM = "Admob";

        public event Action BannerLoadStateChanged;

        public int GetPriority() => this.admobSettingBlueprintService.GetBlueprint().priorityBanner;

        public void Initialize()
        {
            this.DestroyView();
            this.loadFailures = 0;

            var adUnitId = this.admobSettingBlueprintService.GetBlueprint().bannerAdUnitId;
            // Nothing configured: never create a view, so the banner never reports loaded and the
            // aggregator keeps the other network's banner.
            if (string.IsNullOrEmpty(adUnitId)) return;

            var adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            var view         = new BannerView(adUnitId, adaptiveSize, AdPosition.Bottom);
            this.bannerView = view;
            view.Hide();
            this.RegisterEventHandlers(view);
            view.LoadAd(this.NewRequest());
        }

        public bool IsBannerLoaded() => this.loaded;

        public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter)
        {
            if (this.bannerView == null) return;
            if (position != BannerPosition.BottomCenter) this.bannerView.SetPosition(this.Convert(position));
            this.bannerView.Show();
            this.isShown = true;
            this.signalBus.Fire<OnShowBannerSignal>(new(this.AD_FLATFORM, ""));
            if (this.loaded) this.AnnounceVisible(true);
        }

        public void HideBanner()
        {
            this.isShown = false;
            this.AnnounceVisible(false);
            if (this.bannerView == null) return;
            this.bannerView.Hide();
            this.signalBus.Fire<OnHideBannerSignal>(new(this.AD_FLATFORM, ""));
        }

        public float GetBannerHeight() => this.isShown && this.loaded && this.bannerView != null ? this.bannerView.GetHeightInPixels() : 0f;

        public bool IsInitialized() => this.bannerView != null;

        public bool IsShown() => this.isShown;

        private AdRequest NewRequest()
        {
            var adRequest = new AdRequest();
            if (this.admobSettingBlueprintService.GetBlueprint().isCollapseBanner) adRequest.Extras.Add("collapsible", "bottom");
            return adRequest;
        }

        // Once per change, as LevelPlay's banner does: AdMob raises a load on every refresh, and a
        // layout that moved on each one would jitter.
        private void AnnounceVisible(bool visible)
        {
            if (visible == this.announcedVisible) return;
            this.announcedVisible = visible;
            this.signalBus.Fire(new OnBannerVisibilityChangedSignal(visible, visible ? this.bannerView.GetHeightInPixels() : 0f));
        }

        private void RegisterEventHandlers(BannerView view)
        {
            view.OnBannerAdLoaded += () => AdmobMainThread.Run(() =>
            {
                if (view != this.bannerView) return;
                this.loadFailures = 0;
                var first = !this.loaded;
                this.loaded = true;
                if (this.isShown) this.AnnounceVisible(true);
                else view.Hide();
                this.signalBus.Fire<OnBannerAdLoadedEventSignal>(new(this.AD_FLATFORM, ""));
                if (first) this.BannerLoadStateChanged?.Invoke();
            });

            view.OnBannerAdLoadFailed += error => AdmobMainThread.Run(() =>
            {
                if (view != this.bannerView) return;
                var message = error != null ? error.GetMessage() : "the load reported no error";
                Debug.LogWarning($"Admob banner failed to load: {message}");
                this.signalBus.Fire<OnBannerAdLoadFailedEventSignal>(new(this.AD_FLATFORM, message));
                if (this.loaded) return; // a failed refresh: AdMob keeps the last ad and tries again itself
                this.loadFailures++;
                this.RetryLoadAsync(view, this.loadFailures).Forget();
            });

            view.OnAdPaid += adValue => AdmobMainThread.Run(() =>
                this.signalBus.Fire<OnBannerAdRevenuePaidEventSignal>(new(this.AD_FLATFORM, "", adValue.Value, adValue.CurrencyCode)));

            view.OnAdClicked += () => AdmobMainThread.Run(() =>
                this.signalBus.Fire<OnBannerAdClickedEventSignal>(new(this.AD_FLATFORM, "")));

            view.OnAdFullScreenContentOpened += () => AdmobMainThread.Run(() =>
                this.signalBus.Fire<OnBannerAdExpandedEventSignal>(new(this.AD_FLATFORM, "")));

            view.OnAdFullScreenContentClosed += () => AdmobMainThread.Run(() =>
                this.signalBus.Fire<OnBannerAdCollapsedEventSignal>(new(this.AD_FLATFORM, "")));
        }

        private async UniTaskVoid RetryLoadAsync(BannerView view, int failures)
        {
            // Real time, so the wait still passes while a fullscreen ad has the game paused.
            await UniTask.Delay(TimeSpan.FromSeconds(AdmobLoadBackoff.DelaySeconds(failures)), DelayType.Realtime);
            if (view != this.bannerView || this.loaded) return;
            view.LoadAd(this.NewRequest());
        }

        private void DestroyView()
        {
            if (this.bannerView == null) return;
            this.HideBanner();
            var wasLoaded = this.loaded;
            this.bannerView.Destroy();
            this.bannerView = null;
            this.loaded     = false;
            if (wasLoaded) this.BannerLoadStateChanged?.Invoke();
        }

        private AdPosition Convert(BannerPosition position)
        {
            return position switch
            {
                BannerPosition.BottomCenter => AdPosition.Bottom,
                BannerPosition.BottomRight  => AdPosition.BottomRight,
                BannerPosition.BottomLeft   => AdPosition.BottomLeft,
                BannerPosition.TopCenter    => AdPosition.Top,
                BannerPosition.TopRight     => AdPosition.TopRight,
                BannerPosition.TopLeft      => AdPosition.TopLeft,
                _                           => AdPosition.Bottom
            };
        }
    }
    #endif
}
