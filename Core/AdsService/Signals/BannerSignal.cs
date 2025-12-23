namespace ThirdPartyService.Core.AdsService.Signals
{
    public class OnBannerAdLoadedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnBannerAdLoadedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnBannerAdLoadFailedEventSignal : BaseAdsSignal
    {
        public string ErrorMessage { get; set; }
        public OnBannerAdLoadFailedEventSignal(string adsPlatform, string errorMessage) : base(adsPlatform)
        {
            this.ErrorMessage = errorMessage;
        }
    }
    public class OnBannerAdClickedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnBannerAdClickedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnBannerAdRevenuePaidEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public double Revenue     { get; set; }
        public string Currency    { get; set; }
        public OnBannerAdRevenuePaidEventSignal(string adsPlatform, string placementId, double revenue, string currency) : base(adsPlatform)
        {
            this.PlacementId = placementId;
            this.Revenue     = revenue;
            this.Currency    = currency;
        }
    }
    public class OnBannerAdExpandedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnBannerAdExpandedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnBannerAdCollapsedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnBannerAdCollapsedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnShowBannerSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnShowBannerSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnHideBannerSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnHideBannerSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
}
