namespace ThirdPartyService.ServiceImplementation.AdsService.DummyAds.MRECAds
{
    using ThirdPartyService.Core.AdsService.MRECAds;
    using UnityEngine;

    public class DummyMRECAds : IMRECAdsService
    {
        public void Initialize()
        {
            // Do nothing
        }

        public void ShowMREC(MRECAdsPosition position)
        {
            Debug.Log("DummyMRECAds: ShowMREC called");
        }

        public void HideMREC()
        {
        }

        public bool IsShown()       => false;
        public bool IsInitialized() => true;
    }
}