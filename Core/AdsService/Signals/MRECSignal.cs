namespace ThirdPartyService.Core.AdsService.Signals
{
    public class OnMRECAdLoadedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnMRECAdLoadedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnMRECAdLoadFailedEventSignal : BaseAdsSignal
    {
        public string ErrorMessage { get; set; }
        public OnMRECAdLoadFailedEventSignal(string adsPlatform, string errorMessage) : base(adsPlatform)
        {
            this.ErrorMessage = errorMessage;
        }
    }
    public class OnMRECAdClickedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnMRECAdClickedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnMRECAdRevenuePaidEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public double Revenue     { get; set; }
        public string Currency    { get; set; }
        public OnMRECAdRevenuePaidEventSignal(string adsPlatform, string placementId, double revenue, string currency) : base(adsPlatform)
        {
            this.PlacementId = placementId;
            this.Revenue     = revenue;
            this.Currency    = currency;
        }
    }
    public class OnMRECAdExpandedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnMRECAdExpandedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnMRECAdCollapsedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnMRECAdCollapsedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnShowMRECSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnShowMRECSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnHideMRECSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnHideMRECSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
}
