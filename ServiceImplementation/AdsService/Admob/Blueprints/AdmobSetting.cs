namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.Blueprints
{
    #if Admob
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.Attributes;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.Services;using Sirenix.OdinInspector;
    using UnityEngine;
    [SOBlueprintAttributes(nameof(AdmobSetting))]
    public class AdmobSettingBlueprintService : BaseSOBlueprintService<AdmobSetting>
    {
        public AdmobSettingBlueprintService(IAssetsManager assetsManager) : base(assetsManager) { }
    }
    [CreateAssetMenu(fileName = "AdmodSetting", menuName = "ThirdParty/ServiceImplementation/AdsService/Admob/AdmodSetting")]
    public class AdmobSetting : ScriptableObject
    {
        public bool   useAOA;
        [HideIf("@!useAOA")]
        public string aoaAdUnitId;
        [HideIf("@!useAOA")]
        public int priorityAOA;

        public bool useBanner;
        [HideIf("@!useBanner")]
        public string bannerAdUnitId;
        [HideIf("@!useBanner")]
        public bool   isCollapseBanner;
        [HideIf("@!useBanner")]
        public int    priorityBanner;

        public bool useInterstitial;
        [HideIf("@!useInterstitial")]
        public string interstitialAdUnitId;
        [HideIf("@!useInterstitial")]
        public int    priorityInterstitial;


        public bool useMREC;
        [HideIf("@!useMREC")]
        public string mrecAdUnitId;
        [HideIf("@!useMREC")]
        public int    priorityMREC;

        public bool useRewarded;
        [HideIf("@!useRewarded")]
        public string rewardedAdUnitId;
        [HideIf("@!useRewarded")]
        public int    priorityRewarded;

        public bool useNative;
        [HideIf("@!useNative")]
        public string nativeAdUnitId;
        [HideIf("@!useNative")]
        public int    priorityNative;
    }
    #endif
}