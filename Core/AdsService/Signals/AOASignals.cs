namespace ThirdPartyService.Core.AdsService.Signals
{
    public class OnAOAAdLoadedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnAOAAdLoadedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnAOAAdLoadFailedEventSignal : BaseAdsSignal
    {
        public string ErrorMessage { get; set; }
        public OnAOAAdLoadFailedEventSignal(string adsPlatform, string errorMessage) : base(adsPlatform)
        {
            this.ErrorMessage = errorMessage;
        }
    }
    public class OnAOAAdDisplayedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnAOAAdDisplayedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnAOAAdDisplayFailedEventSignal : BaseAdsSignal
    {
        public string PlacementId  { get; set; }
        public string ErrorMessage { get; set; }
        public OnAOAAdDisplayFailedEventSignal(string adsPlatform, string placementId, string errorMessage) : base(adsPlatform)
        {
            this.PlacementId  = placementId;
            this.ErrorMessage = errorMessage;
        }
    }
    public class OnAOAAdClickedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnAOAAdClickedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnAOAAdHiddenEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnAOAAdHiddenEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnAOAAdRevenuePaidEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public double Revenue     { get; set; }
        public string Currency    { get; set; }
        public OnAOAAdRevenuePaidEventSignal(string adsPlatform, string placementId, double revenue, string currency) : base(adsPlatform)
        {
            this.PlacementId = placementId;
            this.Revenue     = revenue;
            this.Currency    = currency;
        }
    }
    public class OnAOAShowSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnAOAShowSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
}
