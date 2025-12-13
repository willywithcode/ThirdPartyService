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
        }

        #endregion

        public void RemoveAds()
        {
            this.adsLocalDataService.RemoveAds();
            this.HideBannerAd();
            this.signalBus.Fire<OnRemoveAdsPurchasedSignal>(new());
        }

        public bool IsRemovedAds() => this.adsLocalDataService.IsRemovedAds();
        public void ShowBannerAd()
        {
            if (this.IsRemovedAds()) return;
            this.bannerAdsServices.AsValueEnumerable().FirstOrDefault(banner => banner is DummyBannerAds)?.ShowBanner();
            this.signalBus.Fire<OnShowBannerSignal>(new());
        }
        public void HideBannerAd()
        {
            this.bannerAdsServices.ForEach(banner => banner.HideBanner());
            this.signalBus.Fire<OnHideBannerSignal>(new());
        }
        public float GetBannerAdHeight()
        {
            if (this.IsRemovedAds()) return 0f;
            var banner = this.bannerAdsServices.AsValueEnumerable().FirstOrDefault(b => b.IsShown());
            return banner is null ? 0f : banner.GetBannerHeight();
        }

    }
}