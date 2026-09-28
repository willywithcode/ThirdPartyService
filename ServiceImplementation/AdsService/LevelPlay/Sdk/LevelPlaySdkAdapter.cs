namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk
{
    #if LevelPlay
    using System;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using global::Unity.Services.LevelPlay;
    using UnityEngine;
    using LevelPlaySdk = global::Unity.Services.LevelPlay.LevelPlay;

    // The only code that touches LevelPlay's own types. It translates the IAdsSdk seam to the
    // com.unity.services.levelplay 9.5.1 C# API and nothing more; all policy lives in the wrappers.
    public class LevelPlaySdkAdapter : IAdsSdk
    {
        private Action         pendingSuccess;
        private Action<string> pendingFailure;

        public void Init(string appKey, Action onSuccess, Action<string> onFailed)
        {
            this.pendingSuccess = onSuccess;
            this.pendingFailure = onFailed;
            // LevelPlay's guide: subscribe before Init.
            LevelPlaySdk.OnInitSuccess += this.OnInitSuccess;
            LevelPlaySdk.OnInitFailed  += this.OnInitFailed;
            LevelPlaySdk.Init(appKey);
        }

        private void OnInitSuccess(LevelPlayConfiguration configuration)
        {
            var callback = this.pendingSuccess;
            this.ClearInit();
            #if DEVELOPMENT_BUILD
            LevelPlaySdk.ValidateIntegration();
            #endif
            callback?.Invoke();
        }

        private void OnInitFailed(LevelPlayInitError error)
        {
            var callback = this.pendingFailure;
            this.ClearInit();
            callback?.Invoke(error?.ToString());
        }

        private void ClearInit()
        {
            LevelPlaySdk.OnInitSuccess -= this.OnInitSuccess;
            LevelPlaySdk.OnInitFailed  -= this.OnInitFailed;
            this.pendingSuccess        =  null;
            this.pendingFailure        =  null;
        }

        public IFullscreenAdUnit CreateInterstitial(string adUnitId) => new InterstitialUnit(new LevelPlayInterstitialAd(adUnitId));

        public IRewardedAdUnit CreateRewarded(string adUnitId) => new RewardedUnit(new LevelPlayRewardedAd(adUnitId));

        public IBannerAdUnit CreateBanner(string adUnitId, BannerAdSize size, BannerPosition position)
        {
            var config = new LevelPlayBannerAd.Config.Builder()
                .SetSize(ToLevelPlaySize(size))
                .SetPosition(ToLevelPlayPosition(position))
                .SetDisplayOnLoad(false)
                .Build();
            return new BannerUnit(new LevelPlayBannerAd(adUnitId, config), position);
        }

        public static LevelPlayAdSize ToLevelPlaySize(BannerAdSize size) =>
            size == BannerAdSize.MediumRectangle ? LevelPlayAdSize.MEDIUM_RECTANGLE : LevelPlayAdSize.BANNER;

        public static LevelPlayBannerPosition ToLevelPlayPosition(BannerPosition position) => position switch
        {
            BannerPosition.TopLeft      => LevelPlayBannerPosition.TopLeft,
            BannerPosition.TopCenter    => LevelPlayBannerPosition.TopCenter,
            BannerPosition.TopRight     => LevelPlayBannerPosition.TopRight,
            BannerPosition.CenterLeft   => LevelPlayBannerPosition.CenterLeft,
            BannerPosition.Centered     => LevelPlayBannerPosition.Center,
            BannerPosition.CenterRight  => LevelPlayBannerPosition.CenterRight,
            BannerPosition.BottomLeft   => LevelPlayBannerPosition.BottomLeft,
            BannerPosition.BottomCenter => LevelPlayBannerPosition.BottomCenter,
            BannerPosition.BottomRight  => LevelPlayBannerPosition.BottomRight,
            _                           => LevelPlayBannerPosition.BottomCenter,
        };

        private static string Describe(LevelPlayAdError error) => error?.ToString() ?? "unknown error";

        private class InterstitialUnit : IFullscreenAdUnit
        {
            private readonly LevelPlayInterstitialAd ad;

            public InterstitialUnit(LevelPlayInterstitialAd ad)
            {
                this.ad                   =  ad;
                this.ad.OnAdLoaded        += this.HandleLoaded;
                this.ad.OnAdLoadFailed    += this.HandleLoadFailed;
                this.ad.OnAdDisplayed     += this.HandleDisplayed;
                this.ad.OnAdDisplayFailed += this.HandleDisplayFailed;
                this.ad.OnAdClicked       += this.HandleClicked;
                this.ad.OnAdClosed        += this.HandleClosed;
            }

            public event Action         Loaded;
            public event Action<string> LoadFailed;
            public event Action         Displayed;
            public event Action<string> DisplayFailed;
            public event Action         Clicked;
            public event Action         Closed;

            public bool IsReady => this.ad.IsAdReady();

            public void Load() => this.ad.LoadAd();

            public void Show(string placement) => this.ad.ShowAd(placement);

            private void HandleLoaded(LevelPlayAdInfo info) => this.Loaded?.Invoke();
            private void HandleLoadFailed(LevelPlayAdError error) => this.LoadFailed?.Invoke(Describe(error));
            private void HandleDisplayed(LevelPlayAdInfo info) => this.Displayed?.Invoke();
            private void HandleDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error) => this.DisplayFailed?.Invoke(Describe(error));
            private void HandleClicked(LevelPlayAdInfo info) => this.Clicked?.Invoke();
            private void HandleClosed(LevelPlayAdInfo info) => this.Closed?.Invoke();

            public void Dispose()
            {
                this.ad.OnAdLoaded        -= this.HandleLoaded;
                this.ad.OnAdLoadFailed    -= this.HandleLoadFailed;
                this.ad.OnAdDisplayed     -= this.HandleDisplayed;
                this.ad.OnAdDisplayFailed -= this.HandleDisplayFailed;
                this.ad.OnAdClicked       -= this.HandleClicked;
                this.ad.OnAdClosed        -= this.HandleClosed;
                this.ad.DestroyAd();
            }
        }

        private class RewardedUnit : IRewardedAdUnit
        {
            private readonly LevelPlayRewardedAd ad;

            public RewardedUnit(LevelPlayRewardedAd ad)
            {
                this.ad                   =  ad;
                this.ad.OnAdLoaded        += this.HandleLoaded;
                this.ad.OnAdLoadFailed    += this.HandleLoadFailed;
                this.ad.OnAdDisplayed     += this.HandleDisplayed;
                this.ad.OnAdDisplayFailed += this.HandleDisplayFailed;
                this.ad.OnAdClicked       += this.HandleClicked;
                this.ad.OnAdClosed        += this.HandleClosed;
                this.ad.OnAdRewarded      += this.HandleRewarded;
            }

            public event Action         Loaded;
            public event Action<string> LoadFailed;
            public event Action         Displayed;
            public event Action<string> DisplayFailed;
            public event Action         Clicked;
            public event Action         Closed;
            public event Action<string> Rewarded;

            public bool IsReady => this.ad.IsAdReady();

            public void Load() => this.ad.LoadAd();

            public void Show(string placement) => this.ad.ShowAd(placement);

            private void HandleLoaded(LevelPlayAdInfo info) => this.Loaded?.Invoke();
            private void HandleLoadFailed(LevelPlayAdError error) => this.LoadFailed?.Invoke(Describe(error));
            private void HandleDisplayed(LevelPlayAdInfo info) => this.Displayed?.Invoke();
            private void HandleDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error) => this.DisplayFailed?.Invoke(Describe(error));
            private void HandleClicked(LevelPlayAdInfo info) => this.Clicked?.Invoke();
            private void HandleClosed(LevelPlayAdInfo info) => this.Closed?.Invoke();
            private void HandleRewarded(LevelPlayAdInfo info, LevelPlayReward reward) => this.Rewarded?.Invoke(reward == null ? "reward" : $"{reward.Name} x{reward.Amount}");

            public void Dispose()
            {
                this.ad.OnAdLoaded        -= this.HandleLoaded;
                this.ad.OnAdLoadFailed    -= this.HandleLoadFailed;
                this.ad.OnAdDisplayed     -= this.HandleDisplayed;
                this.ad.OnAdDisplayFailed -= this.HandleDisplayFailed;
                this.ad.OnAdClicked       -= this.HandleClicked;
                this.ad.OnAdClosed        -= this.HandleClosed;
                this.ad.OnAdRewarded      -= this.HandleRewarded;
                this.ad.DestroyAd();
            }
        }

        private class BannerUnit : IBannerAdUnit
        {
            private readonly LevelPlayBannerAd ad;

            public BannerUnit(LevelPlayBannerAd ad, BannerPosition position)
            {
                this.ad                     =  ad;
                this.Position               =  position;
                this.ad.OnAdLoaded          += this.HandleLoaded;
                this.ad.OnAdLoadFailed      += this.HandleLoadFailed;
                this.ad.OnAdDisplayed       += this.HandleDisplayed;
                this.ad.OnAdDisplayFailed   += this.HandleDisplayFailed;
                this.ad.OnAdClicked         += this.HandleClicked;
                this.ad.OnAdExpanded        += this.HandleExpanded;
                this.ad.OnAdCollapsed       += this.HandleCollapsed;
                this.ad.OnAdLeftApplication += this.HandleLeftApplication;
            }

            public event Action         Loaded;
            public event Action<string> LoadFailed;
            public event Action         Displayed;
            public event Action<string> DisplayFailed;
            public event Action         Clicked;
            public event Action         Expanded;
            public event Action         Collapsed;
            public event Action         LeftApplication;

            public BannerPosition Position { get; }

            // LevelPlay reports the size in density-independent points.
            public float HeightPixels
            {
                get
                {
                    var heightDp = this.ad.GetAdSize()?.Height ?? 0;
                    var dpi      = Screen.dpi;
                    return dpi > 0f ? heightDp * dpi / 160f : heightDp;
                }
            }

            public void Load() => this.ad.LoadAd();
            public void Show() => this.ad.ShowAd();
            public void Hide() => this.ad.HideAd();

            private void HandleLoaded(LevelPlayAdInfo info) => this.Loaded?.Invoke();
            private void HandleLoadFailed(LevelPlayAdError error) => this.LoadFailed?.Invoke(Describe(error));
            private void HandleDisplayed(LevelPlayAdInfo info) => this.Displayed?.Invoke();
            private void HandleDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error) => this.DisplayFailed?.Invoke(Describe(error));
            private void HandleClicked(LevelPlayAdInfo info) => this.Clicked?.Invoke();
            private void HandleExpanded(LevelPlayAdInfo info) => this.Expanded?.Invoke();
            private void HandleCollapsed(LevelPlayAdInfo info) => this.Collapsed?.Invoke();
            private void HandleLeftApplication(LevelPlayAdInfo info) => this.LeftApplication?.Invoke();

            public void Dispose()
            {
                this.ad.OnAdLoaded          -= this.HandleLoaded;
                this.ad.OnAdLoadFailed      -= this.HandleLoadFailed;
                this.ad.OnAdDisplayed       -= this.HandleDisplayed;
                this.ad.OnAdDisplayFailed   -= this.HandleDisplayFailed;
                this.ad.OnAdClicked         -= this.HandleClicked;
                this.ad.OnAdExpanded        -= this.HandleExpanded;
                this.ad.OnAdCollapsed       -= this.HandleCollapsed;
                this.ad.OnAdLeftApplication -= this.HandleLeftApplication;
                this.ad.DestroyAd();
            }
        }
    }
    #endif
}
