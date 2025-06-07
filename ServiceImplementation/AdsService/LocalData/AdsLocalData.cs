namespace ThirdPartyService.ServiceImplementation.AdsService.LocalData
{
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;

    public class AdsLocalData : ILocalData
    {
        public bool   IsAdsEnabled { get; set; } = true;
        public string GetKey()     => this.GetType().Name;

        public void Reset()
        {
            this.IsAdsEnabled = true;
        }
    }

    public class AdsLocalDataService : BaseLocalDataService<AdsLocalData>
    {
        public void DisableAds()
        {
            this.Data.IsAdsEnabled = false;
            this.Save();
        }

        public bool IsAdsEnabled() => this.Data.IsAdsEnabled;
    }
}