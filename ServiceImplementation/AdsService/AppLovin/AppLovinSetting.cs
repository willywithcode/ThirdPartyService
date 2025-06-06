namespace ThirdPartyService.ServiceImplementation.AdsService.AppLovin
{
    using System;
    using UnityEngine;

    [Serializable]
    public class AppLovinSetting
    {
        public bool   CreativeDebugger;
        public string SDKKey;
        public bool   MediationDebugger;
        public string InterAdUnitId;
        public string RewardedAdUnitId;
        public string BannerAdUnitId;
        public Color  BannerBackgroundColor;
    }
}