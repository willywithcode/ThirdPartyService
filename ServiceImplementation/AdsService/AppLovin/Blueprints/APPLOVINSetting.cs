namespace ThirdParty.ServiceImplementation.AdsService.AppLovin.Blueprints
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "APPLOVINSetting", menuName = "ThirdParty/ServiceImplementation/AdsService/AppLovin/APPLOVINSetting")]
    public class APPLOVINSetting : ScriptableObject
    {
        public string androidKey;
        public string iosKey;
        public string interstitialAdUnitId;
        public string rewardedAdUnitId;
        public string bannerAdUnitId;
        public string mrecAdUnitId;
    }
}