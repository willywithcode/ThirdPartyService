namespace ThirdPartyService.ServiceImplementation.AdsService
{
    using System.Collections.Generic;
    using MessagePipe;
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

        private readonly AdsLocalDataService                     adsLocalDataService;
        private readonly IEnumerable<IAOAAdsService>             aoaAdsServices;
        private readonly IEnumerable<IBannerAdsService>          bannerAdsServices;
        private readonly IEnumerable<IInterstitialAdsService>    interstitialsAdsServices;
        private readonly IEnumerable<IMRECAdsService>            mrecAdsServices;
        private readonly IEnumerable<INativeAdsService>          nativeAdsServices;
        private readonly IEnumerable<IRewardedAdsService>        rewardedAdsServices;
        private readonly IPublisher<OnRemoveAdsPurchasedSignal>  removeAdsPurchasedPublisher;
        private readonly IPublisher<OnShowBannerSignal>          showBannerPublisher;
        private readonly IPublisher<OnHideBannerSignal>          hideBannerPublisher;
        private readonly IPublisher<OnShowMRECSignal>            showMRECPublisher;
        private readonly IPublisher<OnHideMRECSignal>            hideMRECPublisher;

        public AdsService(
            AdsLocalDataService                     adsLocalDataService,
            IEnumerable<IAOAAdsService>             aoaAdsServices,
            IEnumerable<IBannerAdsService>          bannerAdsServices,
            IEnumerable<IInterstitialAdsService>    interstitialsAdsServices,
            IEnumerable<IMRECAdsService>            mrecAdsServices,
            IEnumerable<INativeAdsService>          nativeAdsServices,
            IEnumerable<IRewardedAdsService>        rewardedAdsServices,
            IPublisher<OnRemoveAdsPurchasedSignal>  removeAdsPurchasedPublisher,
            IPublisher<OnShowBannerSignal>          showBannerPublisher,
            IPublisher<OnHideBannerSignal>          hideBannerPublisher,
            IPublisher<OnShowMRECSignal>            showMRECPublisher,
            IPublisher<OnHideMRECSignal>            hideMRECPublisher
        )
        {
            this.adsLocalDataService         = adsLocalDataService;
            this.aoaAdsServices              = aoaAdsServices;
            this.bannerAdsServices           = bannerAdsServices;
            this.interstitialsAdsServices    = interstitialsAdsServices;
            this.mrecAdsServices             = mrecAdsServices;
            this.nativeAdsServices           = nativeAdsServices;
            this.rewardedAdsServices         = rewardedAdsServices;
            this.removeAdsPurchasedPublisher = removeAdsPurchasedPublisher;
            this.showBannerPublisher         = showBannerPublisher;
            this.hideBannerPublisher         = hideBannerPublisher;
            this.showMRECPublisher           = showMRECPublisher;
            this.hideMRECPublisher           = hideMRECPublisher;
        }

        #endregion

        public void RemoveAds()
        {
            this.adsLocalDataService.RemoveAds();
            this.HideBannerAd();
            this.removeAdsPurchasedPublisher.Publish(new OnRemoveAdsPurchasedSignal());
        }

        public bool IsRemovedAds() => this.adsLocalDataService.IsRemovedAds();
        #region Banner Ads

        private IBannerAdsService currentBannerAdsService;
        private bool              isShowingBannerAd = false;
        public void ShowBannerAd()
        {
            if (this.IsRemovedAds()) return;
            var banner = this.bannerAdsServices
                .AsValueEnumerable()
                .OrderByDescending(b => b.GetPriority())
                .FirstOrDefault();
            if (banner is { })
            {
                banner.ShowBanner();
                this.currentBannerAdsService = banner;
                this.isShowingBannerAd       = true;
            }
            this.showBannerPublisher.Publish(new OnShowBannerSignal("AdsService", ""));
        }
        public void HideBannerAd()
        {
            if (this.IsRemovedAds()) return;
            this.currentBannerAdsService?.HideBanner();
            this.isShowingBannerAd = false;
            this.hideBannerPublisher.Publish(new OnHideBannerSignal("AdsService", ""));
        }
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
                interstitial.ShowInterstitial(where, onShowFail, onShowSuccess);
                return;
            }
            onShowFail?.Invoke();
        }

        #endregion
        #region Rewarded Ads

        public void ShowRewardedAd(UnityAction<bool> onComplete, string where)
        {
            var rewarded = this.rewardedAdsServices
                .AsValueEnumerable()
                .OrderByDescending(r => r.GetPriority())
                .FirstOrDefault();
            if (rewarded is { })
            {
                rewarded.ShowAd(onComplete, where);
                return;
            }
            onComplete?.Invoke(false);
        }

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
            this.showMRECPublisher.Publish(new OnShowMRECSignal("AdsService", ""));
        }
        public void HideMRECAd()
        {
            if (this.IsRemovedAds()) return;
            this.currentMRECAdsService?.HideMREC();
            this.hideMRECPublisher.Publish(new OnHideMRECSignal("AdsService", ""));
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