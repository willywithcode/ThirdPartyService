namespace ThirdPartyService.ServiceImplementation.AdsService.Setting
{
    using System.Collections.Generic;
    using ThirdPartyService.Core.AdsService.AOA;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using ThirdPartyService.Core.AdsService.MRECAds;
    using ThirdPartyService.Core.AdsService.NativeAds;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using UnityEngine;

    [CreateAssetMenu(fileName = "AdsSetting", menuName = "ThirdPartyService/Ads/AdsSetting")]
    public class AdsSetting : ScriptableObject
    {
        [SerializeReference] public List<IAOAAdsService>          AOAAdsServices;
        [SerializeReference] public List<IBannerAdsService>       BannerAdsServices;
        [SerializeReference] public List<IInterstitialAdsService> InterstitialsAdsServices;
        [SerializeReference] public List<IMRECAdsService>         MRECAdsServices;
        [SerializeReference] public List<INativeAdsService>       NativeAdsServices;
        [SerializeReference] public List<IRewardedAdsService>     RewardedAdsServices;
    }
}