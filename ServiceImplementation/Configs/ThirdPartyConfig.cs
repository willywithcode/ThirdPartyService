namespace ThirdPartyService.ServiceImplementation.Configs
{
    using ThirdPartyService.ServiceImplementation.AdsService.AppLovin;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ThirdPartyConfig", menuName = "ThirdPartyService/Configs/ThirdPartyConfig")]
    public class ThirdPartyConfig : ScriptableObject
    {
        public AppLovinSetting appLovinSetting;
    }
}