namespace ThirdPartyService.Core.AdsService.Signals
{
    public class OnNativeAdLoadedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnNativeAdLoadedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnNativeAdLoadFailedEventSignal : BaseAdsSignal
    {
        public string ErrorMessage { get; set; }
        public OnNativeAdLoadFailedEventSignal(string adsPlatform, string errorMessage) : base(adsPlatform)
        {
            this.ErrorMessage = errorMessage;
        }
    }
    public class OnNativeAdDisplayedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnNativeAdDisplayedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnNativeAdClickedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnNativeAdClickedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnNativeAdHiddenEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnNativeAdHiddenEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnNativeAdRevenuePaidEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public double Revenue     { get; set; }
        public string Currency    { get; set; }
        public OnNativeAdRevenuePaidEventSignal(string adsPlatform, string placementId, double revenue, string currency) : base(adsPlatform)
        {
            this.PlacementId = placementId;
            this.Revenue     = revenue;
            this.Currency    = currency;
        }
    }
    public class OnNativeShowSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnNativeShowSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnNativeHideSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnNativeHideSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
}
