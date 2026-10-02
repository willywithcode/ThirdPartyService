namespace ThirdPartyService.ServiceImplementation.AdsService
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using Sirenix.Utilities;
    using ThirdPartyService.Core.AdsService;
    using ThirdPartyService.Core.AdsService.AOA;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using ThirdPartyService.Core.AdsService.MRECAds;
    using ThirdPartyService.Core.AdsService.NativeAds;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using ThirdPartyService.ServiceImplementation.AdsService.DummyAds.BannerAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LocalDatas;
    using UnityEngine.Events;
    using ZLinq;

    public class AdsService : IAdsService
    {
        #region Inject

        private readonly AdsLocalDataService                  adsLocalDataService;
        private readonly IEnumerable<IAOAAdsService>          aoaAdsServices;
        private readonly IEnumerable<IBannerAdsService>       bannerAdsServices;
        private readonly IEnumerable<IInterstitialAdsService> interstitialsAdsServices;
        private readonly IEnumerable<IMRECAdsService>         mrecAdsServices;
        private readonly IEnumerable<INativeAdsService>       nativeAdsServices;
        private readonly IEnumerable<IRewardedAdsService>     rewardedAdsServices;
        private readonly SignalBus                            signalBus;

        public AdsService(
            AdsLocalDataService                  adsLocalDataService,
            IEnumerable<IAOAAdsService>          aoaAdsServices,
            IEnumerable<IBannerAdsService>       bannerAdsServices,
            IEnumerable<IInterstitialAdsService> interstitialsAdsServices,
            IEnumerable<IMRECAdsService>         mrecAdsServices,
            IEnumerable<INativeAdsService>       nativeAdsServices,
            IEnumerable<IRewardedAdsService>     rewardedAdsServices,
            SignalBus                            signalBus
        )
        {
            this.adsLocalDataService      = adsLocalDataService;
            this.aoaAdsServices           = aoaAdsServices;
            this.bannerAdsServices        = bannerAdsServices;
            this.interstitialsAdsServices = interstitialsAdsServices;
            this.mrecAdsServices          = mrecAdsServices;
            this.nativeAdsServices        = nativeAdsServices;
            this.rewardedAdsServices      = rewardedAdsServices;
            this.signalBus                = signalBus;

            foreach (var banner in this.bannerAdsServices)
                if (banner is IBannerLoadState state) state.BannerLoadStateChanged += this.RefreshBanner;
        }

        #endregion

        public void RemoveAds()
        {
            this.adsLocalDataService.RemoveAds();
            this.HideBannerAd();
            this.signalBus.Fire<OnRemoveAdsPurchasedSignal>(new());
        }

        public bool IsRemovedAds() => this.adsLocalDataService.IsRemovedAds();
        #region Banner Ads

        private IBannerAdsService currentBannerAdsService;
        private bool              isShowingBannerAd = false;

        // Asks for a banner once; which network's banner is on screen then follows load state, see
        // RefreshBanner.
        public void ShowBannerAd()
        {
            if (this.IsRemovedAds()) return;
            this.isShowingBannerAd = this.bannerAdsServices.AsValueEnumerable().Any();
            this.RefreshBanner();
            this.signalBus.Fire<OnShowBannerSignal>(new("AdsService", ""));
        }

        // Not gated by IsRemovedAds: RemoveAds() marks the purchase first and then hides through here.
        public void HideBannerAd()
        {
            this.currentBannerAdsService?.HideBanner();
            this.currentBannerAdsService = null;
            this.isShowingBannerAd       = false;
            this.signalBus.Fire<OnHideBannerSignal>(new("AdsService", ""));
        }

        // While a banner is wanted, shows the highest-priority banner that has an ad, so a starved top
        // network falls through and a later load on it takes the screen back. The banner it replaces is
        // hidden first: AdsGate keeps one "banner on screen" flag, so the old banner's hidden report
        // must come before the new one's visible report. With nothing loaded, nothing changes.
        private void RefreshBanner()
        {
            if (!this.isShowingBannerAd || this.IsRemovedAds()) return;
            var best = this.bannerAdsServices
                .AsValueEnumerable()
                .OrderByDescending(b => b.GetPriority())
                .FirstOrDefault(IsBannerLoaded);
            if (best is null || ReferenceEquals(best, this.currentBannerAdsService)) return;

            this.currentBannerAdsService?.HideBanner();
            this.currentBannerAdsService = best;
            best.ShowBanner();
        }

        private static bool IsBannerLoaded(IBannerAdsService banner) => banner is not IBannerLoadState state || state.IsBannerLoaded();
        public float GetBannerAdHeight()
        {
            if (this.IsRemovedAds()) return 0f;
            return this.currentBannerAdsService?.GetBannerHeight() ?? 0f;
        }
        public bool IsShowingBannerAd()
        {
            if (this.IsRemovedAds()) return false;
            return this.isShowingBannerAd;
        }

        #endregion
        #region Interstitial Ads

        public bool IsInterstitialAdReady()
        {
            if (this.IsRemovedAds()) return false;
            return this.interstitialsAdsServices
                .AsValueEnumerable()
                .OrderByDescending(i => i.GetPriority())
                .FirstOrDefault(i => i.IsInterstitialReady()) is { };
        }

        public void ShowInterstitialAd(string where, UnityAction onShowFail = null, UnityAction onShowSuccess = null)
        {
            if (this.IsRemovedAds())
            {
                onShowSuccess?.Invoke();
                return;
            }
            var interstitial = this.interstitialsAdsServices
                .AsValueEnumerable()
                .OrderByDescending(i => i.GetPriority())
                .FirstOrDefault(i => i.IsInterstitialReady());
            if (interstitial is { })
            {
                interstitial.ShowInterstitial(where, onAdClosed: onShowSuccess, onAdFailedToShow: onShowFail);
                return;
            }
            onShowFail?.Invoke();
        }

        #endregion
        #region Rewarded Ads

        // Ready when ANY registered service has an ad, so a starved top-priority network falls
        // through to the next one instead of reporting "no ads" over a filled lower network.
        // Deliberately ungated by IsRemovedAds: rewarded ads keep serving after Remove Ads.
        public bool IsRewardedAdReady() => this.ReadyRewarded() is { };

        // The whole attempt belongs to one service: the highest-priority one that is ready when the
        // caller asks. A show that then fails completes false; it is not retried on another network.
        public void ShowRewardedAd(UnityAction<bool> onComplete, string where)
        {
            var rewarded = this.ReadyRewarded();
            if (rewarded is { })
            {
                rewarded.ShowAd(onComplete, where);
                return;
            }
            onComplete?.Invoke(false);
        }

        private IRewardedAdsService ReadyRewarded() => this.rewardedAdsServices
            .AsValueEnumerable()
            .OrderByDescending(r => r.GetPriority())
            .FirstOrDefault(r => r.IsAdReady());

        #endregion
        #region MREC Ads

        private IMRECAdsService currentMRECAdsService;
        private bool            isShowingMRECAd = false;
        public void ShowMRECAd(MRECAdsPosition position)
        {
            if (this.IsRemovedAds()) return;
            var mrec = this.mrecAdsServices
                .AsValueEnumerable()
                .OrderByDescending(m => m.GetPriority())
                .FirstOrDefault();
            if (mrec is { })
            {
                mrec.ShowMREC(position);
                this.currentMRECAdsService = mrec;
                this.isShowingMRECAd       = true;
            }
            this.signalBus.Fire<OnShowMRECSignal>(new("AdsService", ""));
        }
        public void HideMRECAd()
        {
            if (this.IsRemovedAds()) return;
            this.currentMRECAdsService?.HideMREC();
            this.signalBus.Fire<OnHideMRECSignal>(new("AdsService", ""));
            this.isShowingMRECAd = false;
        }
        public bool IsShowingMRECAd()
        {
            if (this.IsRemovedAds()) return false;
            return this.isShowingMRECAd;
        }

        #endregion
    }
}
