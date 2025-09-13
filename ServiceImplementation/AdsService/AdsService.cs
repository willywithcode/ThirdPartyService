namespace ThirdPartyService.ServiceImplementation.AdsService
{
    using System.Collections.Generic;
    using ThirdPartyService.ServiceImplementation.AdsService.LocalDatas;
    using ThirdPartyService.Core.AdsService;
    using ThirdPartyService.Core.AdsService.AOA;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using ThirdPartyService.Core.AdsService.MRECAds;
    using ThirdPartyService.Core.AdsService.NativeAds;
    using ThirdPartyService.Core.AdsService.RewardedAds;

    public class AdsService : IAdsService
    {
        private readonly AdsLocalDataService adsLocalDataService;

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
            this.adsLocalDataService = adsLocalDataService;
        }

        public void RemoveAds()
        {
            this.adsLocalDataService.RemoveAds();
        }

        public bool IsRemovedAds() => this.adsLocalDataService.IsRemovedAds();
    }
}