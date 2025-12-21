#if MAX
namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin.Blueprints
{
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.Attributes;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.Services;
    using Sirenix.OdinInspector;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using UnityEngine;
    [SOBlueprintAttributes(nameof(APPLOVINSetting))]
    public class APPLOVINBlueprintService : BaseSOBlueprintService<APPLOVINSetting>
    {
        public APPLOVINBlueprintService(IAssetsManager assetsManager) : base(assetsManager) { }
    }

    [CreateAssetMenu(fileName = "APPLOVINSetting", menuName = "ThirdParty/ServiceImplementation/AdsService/AppLovin/APPLOVINSetting")]
    public class APPLOVINSetting : ScriptableObject
    {
        public string         androidKey;
        public string         iosKey;
        public bool useInterstitialsAds;
        [HideIf("@!useInterstitialsAds")]
        public string         interstitialAdUnitId;
        [HideIf("@!useInterstitialsAds")]
        public int priorityInterstitialsAds;

        public bool useRewardedAds;
        [HideIf("@!useRewardedAds")]
        public string         rewardedAdUnitId;
        [HideIf("@!useRewardedAds")]
        public int priorityRewardedAds;

        public bool useBannerAds;
        [HideIf("@!useBannerAds")]
        public string         bannerAdUnitId;
        [HideIf("@!useBannerAds")]
        public BannerPosition bannerPosition = BannerPosition.BottomCenter;
        [HideIf("@!useBannerAds")]
        public int priorityBannerAds;

        public bool useMRECAds;
        [HideIf("@!useMRECAds")]
        public string         mrecAdUnitId;
        [HideIf("@!useMRECAds")]
        public int priorityMRECAds;

        public bool useAoaAds;
        [HideIf("@!useAoaAds")]
        public string         aoaAdUnitId;
        [HideIf("@!useAoaAds")]
        public int priorityAoaAds;
    }
}
#endif