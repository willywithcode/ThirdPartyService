namespace ThirdPartyService.ServiceImplementation.AdsService
{
    using System.Collections.Generic;
    using ThirdPartyService.Core.AdsService;
    using ThirdPartyService.Core.AdsService.AOA;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using ThirdPartyService.Core.AdsService.MRECAds;
    using ThirdPartyService.Core.AdsService.NativeAds;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LocalDatas;

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

        public AdsService(
            AdsLocalDataService                  adsLocalDataService,
            IEnumerable<IAOAAdsService>          aoaAdsServices,
            IEnumerable<IBannerAdsService>       bannerAdsServices,
            IEnumerable<IInterstitialAdsService> interstitialsAdsServices,
            IEnumerable<IMRECAdsService>         mrecAdsServices,
            IEnumerable<INativeAdsService>       nativeAdsServices,
            IEnumerable<IRewardedAdsService>     rewardedAdsServices
        )
        {
            this.adsLocalDataService      = adsLocalDataService;
            this.aoaAdsServices           = aoaAdsServices;
            this.bannerAdsServices        = bannerAdsServices;
            this.interstitialsAdsServices = interstitialsAdsServices;
            this.mrecAdsServices          = mrecAdsServices;
            this.nativeAdsServices        = nativeAdsServices;
            this.rewardedAdsServices      = rewardedAdsServices;
        }

        #endregion

        public void RemoveAds()
        {
            this.adsLocalDataService.RemoveAds();
        }

        public bool IsRemovedAds() => this.adsLocalDataService.IsRemovedAds();
    }
}