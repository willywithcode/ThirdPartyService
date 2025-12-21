namespace ThirdPartyService.ServiceImplementation.AdsService.DummyAds.AOA
{
    using ThirdPartyService.Core.AdsService.AOA;
    using UnityEngine;

    public class DummyAOAAdsService : IAOAAdsService
    {
        public int GetPriority() => 1;
        public void Initialize()
        {
        }

        public void ShowAd()
        {
            Debug.Log("DummyAOAAdsService: ShowAd called");
        }

        public void HideAd()
        {
        }

        public bool IsShown() => false;
        public bool IsReady() => true;
    }
}