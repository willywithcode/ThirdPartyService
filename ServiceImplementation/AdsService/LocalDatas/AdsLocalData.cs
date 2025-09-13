namespace ThirdPartyService.ServiceImplementation.AdsService.LocalDatas
{
    using GameFoundation.Scripts.LocalData.Interfaces;
    using GameFoundation.Scripts.LocalData.Service;

    public class AdsLocalData : ILocalData
    {
        public bool IsRemovedAds { get; set; } = false;

        public string GetKey() => this.GetType().Name;

        public void Reset()
        {
            this.IsRemovedAds = false;
        }
    }

    public class AdsLocalDataService : BaseLocalDataService<AdsLocalData>
    {
        public void RemoveAds()
        {
            this.Data.IsRemovedAds = true;
            this.Save();
        }

        public bool IsRemovedAds() => this.Data.IsRemovedAds;
    }
}