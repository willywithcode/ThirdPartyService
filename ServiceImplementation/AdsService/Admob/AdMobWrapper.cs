namespace ThirdPartyService.ServiceImplementation.AdsService.Admob
{
    using System;
    using ThirdPartyService.Core.AdsService.AOA;
    using ThirdPartyService.Core.AdsService.MREC;
    using VContainer.Unity;
    using GoogleMobileAds.Api;
    using GoogleMobileAds.Ump.Api;

    public class AdMobWrapper : IAOAAdService, IMRECAdsService, IInitializable
    {
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public bool IsAOAReady()
        {
            throw new NotImplementedException();
        }

        public void ShowAOAAds()
        {
            throw new NotImplementedException();
        }

        public void ShowMREC(AdViewPosition adViewPosition)
        {
            throw new NotImplementedException();
        }

        public void HideMREC(AdViewPosition adViewPosition)
        {
            throw new NotImplementedException();
        }

        public void StopMRECAutoRefresh(AdViewPosition adViewPosition)
        {
            throw new NotImplementedException();
        }

        public void StartMRECAutoRefresh(AdViewPosition adViewPosition)
        {
            throw new NotImplementedException();
        }

        public void LoadMREC(AdViewPosition adViewPosition)
        {
            throw new NotImplementedException();
        }

        public bool IsMRECReady(AdViewPosition adViewPosition)
        {
            throw new NotImplementedException();
        }

        public void HideAllMREC()
        {
            throw new NotImplementedException();
        }
    }
}