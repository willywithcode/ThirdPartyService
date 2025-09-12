namespace ThirdParty.ServiceImplementation.AdsService.Admob.Blueprints
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "AdmodSetting", menuName = "ThirdParty/ServiceImplementation/AdsService/Admob/AdmodSetting")]
    public class AdmobSetting : ScriptableObject
    {
        public string aoaAdUnitId;
        public string bannerAdUnitId;
        public bool   isCollapseBanner;
        public string interstitialAdUnitId;
        public string mrecAdUnitId;
        public string rewardedAdUnitId;
    }
}