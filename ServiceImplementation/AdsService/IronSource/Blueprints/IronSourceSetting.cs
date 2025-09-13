namespace ThirdPartyService.ServiceImplementation.AdsService.IronSource.Blueprints
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "IronSourceSetting", menuName = "ThirdParty/ServiceImplementation/AdsService/IronSource/Settings")]
    public class IronSourceSetting : ScriptableObject
    {
        public string appKey;
        public string interstitialAdKey;
        public string rewardedAdKey;
        public string bannerAdKey;
        public string mrecAdKey;
    }
}